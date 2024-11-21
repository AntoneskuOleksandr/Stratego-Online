using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PiecePlacementManager : NetworkBehaviour
{
    private BoardManager boardManager;
    private ConfigManager config;

    public void Initialize(BoardManager boardManager, ConfigManager config)
    {
        this.boardManager = boardManager;
        this.config = config;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryToPlacePieceServerRpc(Vector2Int tileIndex, string pieceName, ulong clientId)
    {
        PieceData pieceData = config.GetPieceDataByName(pieceName);
        Tile tile = boardManager.GetTileAt(tileIndex);
        int availableCount = boardManager.pieceCountsByPlayer[clientId][pieceData.Name];

        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        if (tile != null && !tile.IsOccupied && IsTileInPlayerHalf(tile, clientId) && availableCount > 0)
        {
            InitializePieceClientRpc(tileIndex, pieceName, clientId);
            boardManager.UpdatePieceCountClientRpc(clientId, pieceName, availableCount - 1);
        }
        else
        {
            Debug.LogWarning("Something went wrong. You can't place piece here." +
                "\nTile: " + tile + "\nTile IsOccupied: " + tile.IsOccupied + "\nIsTileInPlayerHalf: " + IsTileInPlayerHalf(tile, clientId)
                + "\nPiece Count: " + availableCount + "\nPiece Name: " + pieceName);
        }
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

    [ServerRpc(RequireOwnership = false)]
    public void PlacePiecesRandomlyServerRpc(ulong clientId)
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
                TryToPlacePieceServerRpc(randomTile.IndexInMatrix, pieceData.Name, clientId);
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

        if (clientId == NetworkManager.Singleton.ConnectedClientsIds[0])
            return tile.IndexInMatrix.y < maxRows / 2 - 1;
        else if (clientId == NetworkManager.Singleton.ConnectedClientsIds[1])
            return tile.IndexInMatrix.y > maxRows / 2;
        else
        {
            Debug.LogError("Something wrong with clientId");
            return false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryToRemovePieceServerRpc(Vector2Int tileIndex, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(tileIndex);
        Piece piece = tile.GetPiece();
        if (!IsTileInPlayerHalf(tile, clientId) || piece == null)
        {
            return;
        }

        PieceData pieceData = piece.PieceData;
        int availableCount = boardManager.pieceCountsByPlayer[clientId][pieceData.Name];

        if (tile != null && tile.IsOccupied && IsTileInPlayerHalf(tile, clientId))
        {
            CmdRemovePieceClientRpc(tile.IndexInMatrix, clientId);
            boardManager.UpdatePieceCountClientRpc(clientId, pieceData.Name, availableCount + 1);
        }
        else
            Debug.Log("Failed attempt to remove Piece; Tile: " + tile +
                "; isOccupied: " + tile.IsOccupied + "; IsTileInPlayerHalf: " + IsTileInPlayerHalf(tile, clientId));
    }

    [ClientRpc]
    private void CmdRemovePieceClientRpc(Vector2Int pieceLocation, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(pieceLocation);
        Piece piece = tile.GetPiece();

        if (piece != null)
        {
            PieceData pieceData = piece.PieceData;
            Destroy(piece.gameObject);
            tile.RemovePiece();
        }
        else
        {
            Debug.LogError("Tile is occupied but tile.GetPiece is null");
        }
    }
}