using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BoardManager : NetworkBehaviour
{
    public Dictionary<ulong, Dictionary<string, int>> pieceCountsByPlayer = new Dictionary<ulong, Dictionary<string, int>>();
    private BoardGenerator boardGenerator;
    private GameObject[,] tiles;
    private ConfigManager config;
    private GameManager gameManager;

    public void Initialize(BoardGenerator boardGenerator, ConfigManager config, GameManager gameManager)
    {
        this.boardGenerator = boardGenerator;
        this.config = config;
        this.gameManager = gameManager;

        boardGenerator.Initialize(config);
    }

    [ServerRpc(RequireOwnership = false)]
    public void InitializePieceCountsServerRpc()
    {
        ulong clientsCount = 1;

        if (IsHost)
            clientsCount = 2;

        for (ulong i = 0; i < clientsCount; i++)
        {
            pieceCountsByPlayer[i] = new Dictionary<string, int>();
            foreach (var pieceData in config.PiecesData)
            {
                pieceCountsByPlayer[i][pieceData.Name] = pieceData.Count;
            }
            Debug.Log("Pieces count initialized for client with id: " + i);
            SendPieceCountsToClient(i);
        }
    }

    [ClientRpc]
    private void UpdateClientPieceCountClientRpc(ulong clientId, string pieceName, int count)
    {
        Debug.Log("Client: " + clientId + "; Piece name: " + pieceName + "; Count: " + count);
        if (pieceCountsByPlayer.ContainsKey(clientId))
        {
            Debug.Log(pieceCountsByPlayer[clientId][pieceName]);
            Debug.Log(count);
            pieceCountsByPlayer[clientId][pieceName] = count;
        }
    }

    public void SendPieceCountsToClient(ulong clientId)
    {
        foreach (var pieceCount in pieceCountsByPlayer[clientId])
        {
            if (!IsHost)
                UpdateClientPieceCountClientRpc(clientId, pieceCount.Key, pieceCount.Value);
        }
    }

    [ClientRpc]
    public void InitializeBoardClientRpc()
    {
        Debug.Log("InitializeBoardClientRpc");

        tiles = boardGenerator.GenerateBoard();

        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                Tile tileComponent = tiles[x, y].GetComponent<Tile>();
                tileComponent.Initialize(new Vector2Int(x, y), gameManager, config.TileColorHighlighted);
            }
        }
    }

    public Tile GetTileAt(Vector2Int index)
    {
        if (tiles == null || index.x < 0 || index.x >= tiles.GetLength(0) || index.y < 0 || index.y >= tiles.GetLength(1))
        {
            Debug.Log($"Tile on ({index.x};{index.y}) is null");
            return null;
        }
        return tiles[index.x, index.y].GetComponent<Tile>();
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
