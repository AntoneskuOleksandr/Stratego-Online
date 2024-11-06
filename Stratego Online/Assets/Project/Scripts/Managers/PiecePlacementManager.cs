using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PiecePlacementManager : NetworkBehaviour
{
    private IBoardManager boardManager;
    private ConfigManager config;
    private Dictionary<ulong, Dictionary<string, int>> pieceCountsByPlayer = new Dictionary<ulong, Dictionary<string, int>>();
    private UIManager uiManager;

    public void Initialize(IBoardManager boardManager, ConfigManager configManager, UIManager uiManager)
    {
        this.boardManager = boardManager;
        this.config = configManager;
        this.uiManager = uiManager;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlacePiecesRandomlyServerRpc(ulong clientId)
    {
        PlacePieces(clientId);
    }

    private void PlacePieces(ulong clientId)
    {
        InitializePieceCountsIfNeeded(clientId);
        Debug.Log("PlacePieces for client " + clientId);

        foreach (var pieceData in config.PiecesData)
        {
            while (pieceCountsByPlayer[clientId][pieceData.Name] > 0)
            {
                Tile randomTile = GetRandomEmptyTile(clientId);

                Debug.Log("Player ID: " + clientId + "; Tile: " + randomTile);

                if (randomTile != null)
                {
                    SpawnPieceInTileServerRpc(pieceData.Name, randomTile.IndexInMatrix.Value, clientId);
                    pieceCountsByPlayer[clientId][pieceData.Name] -= 1;
                    uiManager.UpdatePieceCount(pieceData.Name, pieceCountsByPlayer[clientId][pieceData.Name]);
                }
                else
                {
                    Debug.LogError("There are no free tiles");
                    break;
                }
            }
        }
    }

    private void InitializePieceCountsIfNeeded(ulong clientId)
    {
        if (!pieceCountsByPlayer.ContainsKey(clientId))
        {
            pieceCountsByPlayer[clientId] = new Dictionary<string, int>();
            foreach (var pieceData in config.PiecesData)
            {
                pieceCountsByPlayer[clientId][pieceData.Name] = pieceData.Count;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPieceInTileServerRpc(string pieceName, Vector2Int tileIndex, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(tileIndex.x, tileIndex.y);

        if (tile != null && !tile.IsOccupied.Value)
        {
            PieceData pieceData = config.GetPieceDataByName(pieceName);
            if (pieceData != null)
            {
                GameObject pieceObject = Instantiate(pieceData.Prefab, tile.transform.position, Quaternion.identity);
                NetworkObject networkObject = pieceObject.GetComponent<NetworkObject>();
                networkObject.Spawn(true);

                Piece placedPiece = pieceObject.GetComponent<Piece>();
                placedPiece.Initialize(tile, boardManager, pieceData, (int)clientId);
                tile.SetPiece(placedPiece);

                int newCount = pieceCountsByPlayer[clientId][pieceName];
                uiManager.UpdatePieceCount(pieceName, newCount);

                if (newCount == 0)
                {
                    uiManager.DeselectPiece();
                }
            }
        }
    }

    private Tile GetRandomEmptyTile(ulong clientId)
    {
        Tile[,] allTiles = boardManager.GetAllTiles();

        Tile[] emptyTiles = System.Array.FindAll(allTiles.Cast<Tile>().ToArray(), tile => !tile.IsOccupied.Value && IsTileInPlayerHalf(tile, clientId));

        if (emptyTiles.Length > 0)
        {
            return emptyTiles[Random.Range(0, emptyTiles.Length)];
        }
        Debug.LogWarning("There are no empty tiles in the first four rows");
        return null;
    }

    private bool IsTileInPlayerHalf(Tile tile, ulong clientId)
    {
        int maxRows = config.BoardRows;

        if (clientId == 0)
            if (tile.IndexInMatrix.Value.y < maxRows / 2 - 1)
                return true;
            else
                return false;
        else if (clientId == 1)
            if (tile.IndexInMatrix.Value.y > maxRows / 2)
                return true;
            else
                return false;
        else
        {
            Debug.LogError("Something wrong with clientId");
            return false;
        }
    }
}
