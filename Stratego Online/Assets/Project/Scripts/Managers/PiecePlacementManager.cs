using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PiecePlacementManager : NetworkBehaviour
{
    private BoardManager boardManager;
    private UIManager uiManager;
    private ConfigManager config;
    private ulong clientId;

    public void Initialize(BoardManager boardManager, UIManager uiManager, ConfigManager config)
    {
        this.boardManager = boardManager;
        this.uiManager = uiManager;
        this.config = config;
        clientId = NetworkManager.Singleton.LocalClientId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlacePieceServerRpc(Vector2Int tileIndex, string pieceName, ulong clientId)
    {
        PieceData pieceData = config.GetPieceDataByName(pieceName);
        Tile tile = boardManager.GetTileAt(tileIndex.x, tileIndex.y);
        if (tile != null && !tile.IsOccupied && IsTileInPlayerHalf(tile, clientId) && boardManager.pieceCountsByPlayer[clientId][pieceData.Name] > 0)
        {
            if (pieceData != null)
            {
                GameObject pieceObject = Instantiate(pieceData.Prefab, tile.transform.position, clientId == 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0));
                NetworkObject networkObject = pieceObject.GetComponent<NetworkObject>();

                Piece placedPiece = pieceObject.GetComponent<Piece>();
                networkObject.Spawn(true);

                InitializePieceClientRpc(networkObject.NetworkObjectId, tile.GetComponent<NetworkObject>().NetworkObjectId,
                    pieceName, clientId);
                tile.SetPiece(placedPiece);

                boardManager.pieceCountsByPlayer[clientId][pieceData.Name] -= 1;
                boardManager.SendPieceCountsToClient(clientId);

                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                UpdatePieceCountClientRpc(pieceData.Name, boardManager.pieceCountsByPlayer[clientId][pieceData.Name], clientRpcParams);

                if (boardManager.pieceCountsByPlayer[clientId][pieceData.Name] == 0)
                {
                    uiManager.DeselectPiece();
                }
            }
        }
        else
        {
            Debug.LogWarning("Something went wrong. You can't place piece here." +
                "\nTile: " + tile + "\nTile IsOccupied: " + tile.IsOccupied + "\nIsTileInPlayerHalf: " + IsTileInPlayerHalf(tile, clientId)
                + "\nPiece Count: " + boardManager.pieceCountsByPlayer[clientId][pieceName] + "\nPiece Name: " + pieceName);
        }
    }

    [ClientRpc]
    private void UpdatePieceCountClientRpc(string pieceName, int count, ClientRpcParams clientRpcParams = default)
    {
        uiManager.UpdatePieceCount(pieceName, count);
    }

    [ClientRpc]
    private void InitializePieceClientRpc(ulong pieceNOId, ulong tileNOId, string pieceDataName, ulong clientId)
    {
        NetworkObject pieceNO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[pieceNOId];
        Piece piece = pieceNO.GetComponent<Piece>();
        NetworkObject tileNO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tileNOId];
        Tile tile = tileNO.GetComponent<Tile>();

        piece.ClientInitialize(tile, boardManager, config.GetPieceDataByName(pieceDataName), clientId);
    }

    public void RequestPlacePiecesRandomly()
    {
        PlacePiecesRandomlyServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlacePiecesRandomlyServerRpc(ulong clientId)
    {
        List<Tile> availableTiles = GetAvailableTiles(clientId);

        foreach (PieceData pieceData in config.PiecesData)
        {
            int pieceCount = boardManager.pieceCountsByPlayer[clientId][pieceData.Name];
            for (int i = 0; i < pieceCount; i++)
            {
                if (availableTiles.Count == 0) break;
                Tile randomTile = availableTiles[Random.Range(0, availableTiles.Count)];
                availableTiles.Remove(randomTile);
                PlacePieceServerRpc(randomTile.IndexInMatrix, pieceData.Name, clientId);
            }
        }
    }

    private List<Tile> GetAvailableTiles(ulong clientId)
    {
        List<Tile> availableTiles = new List<Tile>();
        foreach (Tile tile in boardManager.GetAllTiles())
        {
            if (!tile.IsOccupied && IsTileInPlayerHalf(tile, clientId))
            {
                availableTiles.Add(tile);
            }
        }
        return availableTiles;
    }

    private bool IsTileInPlayerHalf(Tile tile, ulong clientId)
    {
        int maxRows = config.BoardRows;

        if (clientId == 0)
            return tile.IndexInMatrix.y < maxRows / 2 - 1;
        else if (clientId == 1)
            return tile.IndexInMatrix.y > maxRows / 2;
        else
        {
            Debug.LogError("Something wrong with clientId");
            return false;
        }
    }

    public void TryRemovePiece(Tile tile, ulong clientId)
    {
        if (tile != null && tile.IsOccupied && IsTileInPlayerHalf(tile, clientId))
        {
            CmdRemovePieceServerRpc(tile.IndexInMatrix.x, tile.IndexInMatrix.y, clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdRemovePieceServerRpc(int x, int y, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(x, y);
        Piece piece = tile.GetPiece();
        if (piece != null)
        {
            PieceData pieceData = piece.PieceData;
            Destroy(piece.gameObject);
            tile.RemovePiece();

            if (pieceData != null)
            {
                boardManager.pieceCountsByPlayer[clientId][pieceData.Name] += 1;
                boardManager.SendPieceCountsToClient(clientId);

                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                UpdatePieceCountClientRpc(pieceData.Name, boardManager.pieceCountsByPlayer[clientId][pieceData.Name], clientRpcParams);
            }
        }
        else
        {
            Debug.LogError("Tile is occupied but tile.GetPiece is null");
        }
    }
}
