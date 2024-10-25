using System.Linq;
using UnityEngine;

public class PiecePlacementManager : MonoBehaviour
{
    private IBoardManager boardManager;
    private ConfigManager configManager;

    public void Initialize(IBoardManager boardManager, ConfigManager configManager)
    {
        this.boardManager = boardManager;
        this.configManager = configManager;
    }

    public void PlacePiecesRandomly()
    {
        for (int i = 0; i < configManager.piecePrefabs.Length; i++)
        {
            GameObject piecePrefab = configManager.piecePrefabs[i];
            int pieceCount = configManager.pieceCounts[i];

            for (int j = 0; j < pieceCount; j++)
            {
                Tile randomTile = GetRandomEmptyTile();
                if (randomTile != null)
                {
                    GameObject pieceObject = Instantiate(piecePrefab, randomTile.transform.position, Quaternion.identity);
                    Piece piece = pieceObject.GetComponent<Piece>();
                    piece.Initialize(randomTile);
                }
            }
        }
    }

    private Tile GetRandomEmptyTile()
    {
        Tile[,] allTiles = boardManager.GetAllTiles();
        Tile[] emptyTiles = System.Array.FindAll(allTiles.Cast<Tile>().ToArray(), tile => !tile.IsOccupied);

        if (emptyTiles.Length > 0)
        {
            return emptyTiles[Random.Range(0, emptyTiles.Length)];
        }

        Debug.LogWarning("There are no empty tiles");
        return null;
    }
}
