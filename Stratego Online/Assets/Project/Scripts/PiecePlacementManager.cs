using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PiecePlacementManager : MonoBehaviour
{
    private IBoardManager boardManager;
    private ConfigManager configManager;
    private Dictionary<string, int> pieceCounts;

    public void Initialize(IBoardManager boardManager, ConfigManager configManager)
    {
        this.boardManager = boardManager;
        this.configManager = configManager;

        pieceCounts = configManager.PiecesData.ToDictionary(pieceData => pieceData.Name, pieceData => pieceData.Count);
    }

    public void PlacePiecesRandomly()
    {
        foreach (var pieceData in configManager.PiecesData)
        {
            int pieceCount = pieceCounts[pieceData.Name];

            for (int j = 0; j < pieceCount; j++)
            {
                Tile randomTile = GetRandomEmptyTile();
                if (randomTile != null)
                {
                    SpawnPieceInTile(pieceData.Prefab, pieceData, randomTile);
                }
                else
                {
                    Debug.LogError("There is no free tiles");
                }
            }
        }
    }

    private void SpawnPieceInTile(GameObject piecePrefab, PieceData pieceData, Tile tile)
    {
        GameObject pieceObject = Instantiate(piecePrefab, tile.transform.position, Quaternion.identity);
        Piece piece = pieceObject.GetComponent<Piece>();
        piece.Initialize(tile, boardManager, pieceData);
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
}
