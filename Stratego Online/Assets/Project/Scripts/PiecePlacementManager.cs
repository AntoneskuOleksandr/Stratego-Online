using System.Linq;
using UnityEngine;

public class PiecePlacementManager : MonoBehaviour
{
    [SerializeField] private GameObject[] pieces;
    private IBoardManager boardManager; 

    public void Initialize(IBoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    public void PlacePiecesRandomly()
    {
        foreach (GameObject piecePrefab in pieces)
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

    private Tile GetRandomEmptyTile()
    {
        Tile[,] allTiles = boardManager.GetAllTiles();
        Tile[] emptyTiles = System.Array.FindAll(allTiles.Cast<Tile>().ToArray(), tile => !tile.IsOccupied); 

        if (emptyTiles.Length > 0)
        {
            return emptyTiles[Random.Range(0, emptyTiles.Length)];
        }

        return null;
    }
}
