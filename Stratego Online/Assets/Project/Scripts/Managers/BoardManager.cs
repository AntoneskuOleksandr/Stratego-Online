using System;
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

    public void InitializeBoard(IGameManager gameManager)
    {
        Debug.Log("InitializeBoard");
        this.gameManager = gameManager;

        if (IsServer)
        {
            tiles = boardGenerator.GenerateBoard();

            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    Tile tileComponent = tiles[x, y].GetComponent<Tile>();
                    tileComponent.ServerInitialize(gameManager, new Vector2Int(x, y), tileComponent.IsLake.Value);
                    InitializeTileClientRpc(tileComponent.NetworkObjectId, tileComponent.IsLake.Value, x, y);
                }
            }
        }
    }


    [ClientRpc]
    private void InitializeTileClientRpc(ulong networkObjectId, bool isLake, int x, int y)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (networkObject != null)
        {
            Tile tile = networkObject.GetComponent<Tile>();
            if (tile != null)
            {
                Material tileMaterial = isLake ? config.TileMaterialLake :
                    (x + y) % 2 == 0 ? config.TileMaterialWhite : config.TileMaterialBlack;
                tile.SetMaterial(tileMaterial);
                tile.ClientInitialize(gameManager, config.TileColorHighlighted, tileMaterial);
            }
        }
    }


    public Tile GetTileAt(int x, int y)
    {
        if (tiles == null || x < 0 || x >= tiles.GetLength(0) || y < 0 || y >= tiles.GetLength(1))
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
