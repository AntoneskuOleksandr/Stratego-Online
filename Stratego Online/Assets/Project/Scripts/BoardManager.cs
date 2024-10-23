using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private BoardGenerator boardGenerator;
    private ConfigManager config;

    public void Initialize(BoardGenerator boardGenerator, ConfigManager config)
    {
        this.boardGenerator = boardGenerator;
        this.config = config;
        boardGenerator.Initialize(config);
    }

    public void GenerateBoard(GameManager gameManager)
    {
        GameObject[,] tiles = boardGenerator.GenerateBoard();
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                Tile tileComponent = tiles[x, y].GetComponent<Tile>();
                tileComponent.Initialize(gameManager);
            }
        }
    }
}
