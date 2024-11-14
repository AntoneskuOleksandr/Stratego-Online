using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PiecePlacementManager : NetworkBehaviour
{
    private BoardManager boardManager;
    private UIManager uiManager;
    private ConfigManager config;

    public void Initialize(BoardManager boardManager, UIManager uiManager, ConfigManager config)
    {
        this.boardManager = boardManager;
        this.uiManager = uiManager;
        this.config = config;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlacePieceServerRpc(Vector2Int tileIndex, string pieceName, ulong clientId)
    {
        PieceData pieceData = config.GetPieceDataByName(pieceName);
        Debug.Log(pieceData);

        Tile tile = boardManager.GetTileAt(tileIndex);

        if (tile != null && !tile.IsOccupied && IsTileInPlayerHalf(tile, clientId) && boardManager.pieceCountsByPlayer[clientId][pieceData.Name] > 0)
        {
            if (pieceData != null)
            {
                InitializePieceClientRpc(tileIndex, pieceData.Name, clientId);

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
    private void InitializePieceClientRpc(Vector2Int tileIndex, string PieceName, ulong clientId)
    {
        PieceData pieceData = config.GetPieceDataByName(PieceName);
        Tile tile = boardManager.GetTileAt(tileIndex);
        GameObject pieceGO = Instantiate(pieceData.Prefab, tile.transform.position, clientId == 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0));
        Piece piece = pieceGO.GetComponent<Piece>();
        tile.SetPiece(piece);
        piece.Initialize(tile, boardManager, pieceData, clientId);
    }

    public void RequestPlacePiecesRandomly(ulong clientId)
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
            CmdRemovePieceServerRpc(tile.IndexInMatrix, clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdRemovePieceServerRpc(Vector2Int pieceLocation, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(pieceLocation);
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
