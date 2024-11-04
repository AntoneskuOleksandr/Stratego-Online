using Unity.Netcode;
using UnityEngine;

public class BoardManager : NetworkBehaviour, IBoardManager
{
    private BoardGenerator boardGenerator;
    private GameObject[,] tiles;
    private ConfigManager config;
    private IGameManager gameManager;

    public void Initialize(BoardGenerator boardGenerator, ConfigManager config)
    {
        this.boardGenerator = boardGenerator;
        this.config = config;

        boardGenerator.Initialize(config);
    }

    public void GenerateBoard(IGameManager gameManager)
    {
        this.gameManager = gameManager;
        tiles = boardGenerator.GenerateBoard();

        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                Tile tileComponent = tiles[x, y].GetComponent<Tile>();
                tileComponent.Initialize(gameManager, config.TileColorHighlighted);
            }
        }

        GenerateBoardClientRpc();
    }

    [ServerRpc]
    public void GenerateBoardServerRpc()
    {
        tiles = boardGenerator.GenerateBoard();

        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                Tile tileComponent = tiles[x, y].GetComponent<Tile>();
                tileComponent.Initialize(gameManager, config.TileColorHighlighted);
            }
        }

        GenerateBoardClientRpc();
    }

    [ClientRpc]
    private void GenerateBoardClientRpc()
    {
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                Tile tileComponent = tiles[x, y].GetComponent<Tile>();
                tileComponent.Initialize(gameManager, config.TileColorHighlighted);
            }
        }
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x < 0 || x >= tiles.GetLength(0) || y < 0 || y >= tiles.GetLength(1))
        {
            return null;
        }
        return tiles[x, y].GetComponent<Tile>();
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
