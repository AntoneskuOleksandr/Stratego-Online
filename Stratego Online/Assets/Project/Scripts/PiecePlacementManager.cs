using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PiecePlacementManager : MonoBehaviour
{
    private IBoardManager boardManager;
    private ConfigManager configManager;
    private Dictionary<string, int> pieceCounts = new Dictionary<string, int>();
    private UIManager uiManager;

    public void Initialize(IBoardManager boardManager, ConfigManager configManager, UIManager uiManager)
    {
        this.boardManager = boardManager;
        this.configManager = configManager;
        this.uiManager = uiManager;
        foreach (var pieceData in configManager.PiecesData)
        {
            pieceCounts[pieceData.Name] = pieceData.Count;
        }
    }

    public void PlacePiecesRandomly()
    {
        foreach (var pieceData in configManager.PiecesData)
        {
            for (int j = 0; j < pieceCounts[pieceData.Name];)
            {
                Tile randomTile = GetRandomEmptyTile();
                if (randomTile != null)
                {
                    SpawnPieceInTile(pieceData.Prefab, pieceData, randomTile, 0);
                    uiManager.UpdatePieceCount(pieceData.Name, pieceCounts[pieceData.Name]);
                }
                else
                {
                    Debug.LogError("There is no free tiles");
                }
            }
        }
    }

    public void MirrorPlacePiecesRandomly()
    {
        foreach (var pieceData in configManager.PiecesData)
        {
            for (int j = 0; j < pieceData.Count; j++)
            {
                Tile mirroredTile = GetMirroredTile();
                if (mirroredTile != null)
                {
                    SpawnPieceInMirroredTile(pieceData.Prefab, pieceData, mirroredTile, 1);
                }
                else
                {
                    Debug.LogError("There is no free tiles on the mirrored side");
                }
            }
        }
    }

    private void SpawnPieceInTile(GameObject piecePrefab, PieceData pieceData, Tile tile, ulong playerId)
    {
        GameObject pieceObject = Instantiate(piecePrefab, tile.transform.position, Quaternion.identity);
        Piece piece = pieceObject.GetComponent<Piece>();
        piece.Initialize(tile, boardManager, pieceData, playerId);
        piece.gameObject.GetComponent<NetworkObject>().Spawn(true);
        tile.PlacePiece(piece);
        pieceCounts[pieceData.Name]--;
    }


    private void SpawnPieceInMirroredTile(GameObject piecePrefab, PieceData pieceData, Tile tile, ulong playerId)
    {
        GameObject pieceObject = Instantiate(piecePrefab, tile.transform.position, Quaternion.Euler(0, 180, 0));
        Piece piece = pieceObject.GetComponent<Piece>();
        piece.Initialize(tile, boardManager, pieceData, playerId);
        piece.gameObject.GetComponent<NetworkObject>().Spawn(true);
        tile.PlacePiece(piece);
        pieceCounts[pieceData.Name]--;
    }

    private Tile GetRandomEmptyTile()
    {
        Tile[,] allTiles = boardManager.GetAllTiles();
        Tile[] emptyTiles = System.Array.FindAll(allTiles.Cast<Tile>().ToArray(), tile => !tile.IsOccupied && tile.IndexInMatrix.y < 4);
        if (emptyTiles.Length > 0)
        {
            return emptyTiles[Random.Range(0, emptyTiles.Length)];
        }
        Debug.LogWarning("There are no empty tiles in the first four rows");
        return null;
    }

    private Tile GetMirroredTile()
    {
        Tile[,] allTiles = boardManager.GetAllTiles();
        Tile[] mirroredTiles = System.Array.FindAll(allTiles.Cast<Tile>().ToArray(), tile => !tile.IsOccupied && tile.IndexInMatrix.y >= 6);
        if (mirroredTiles.Length > 0)
        {
            return mirroredTiles[Random.Range(0, mirroredTiles.Length)];
        }
        Debug.LogWarning("There are no free tiles on the mirrored side");
        return null;
    }
}
