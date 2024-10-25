using UnityEngine;

public class BoardManager : MonoBehaviour, IBoardManager
{
    private BoardGenerator boardGenerator;
    private GameObject[,] tiles;
    private ConfigManager config;

    public void Initialize(BoardGenerator boardGenerator, ConfigManager config)
    {
        this.boardGenerator = boardGenerator;
        this.config = config;

        boardGenerator.Initialize(config);
    }

    public void GenerateBoard(IGameManager gameManager)
    {
        tiles = boardGenerator.GenerateBoard();

        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                Tile tileComponent = tiles[x, y].GetComponent<Tile>();
                tileComponent.Initialize(gameManager, config.tileColorHighlighted);
            }
        }
    }

    public Tile[,] GetAllTiles()
    {
        Tile[,] tileComponents = new Tile[tiles.GetLength(0), tiles.GetLength(1)];
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                tileComponents[x, y] = tiles[x, y].GetComponent<Tile>();
            }
        }
        return tileComponents;
    }
}
