using System.Collections.Generic;
using System.Linq;
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

    private void SpawnPieceInTile(GameObject piecePrefab, PieceData pieceData, Tile tile, int playerId)
    {
        GameObject pieceObject = Instantiate(piecePrefab, tile.transform.position, Quaternion.identity);
        Piece piece = pieceObject.GetComponent<Piece>();
        piece.Initialize(tile, boardManager, pieceData, playerId);
        tile.PlacePiece(piece);
        pieceCounts[pieceData.Name]--;
    }

    private Tile GetRandomEmptyTile()
    {
        Tile[,] allTiles = boardManager.GetAllTiles();
        Tile[] emptyTiles = System.Array.FindAll(allTiles.Cast<Tile>().ToArray(), tile => !tile.IsOccupied.Value && tile.IndexInMatrix.Value.y < 4);
        if (emptyTiles.Length > 0)
        {
            return emptyTiles[Random.Range(0, emptyTiles.Length)];
        }
        Debug.LogWarning("There are no empty tiles in the first four rows");
        return null;
    }
}
