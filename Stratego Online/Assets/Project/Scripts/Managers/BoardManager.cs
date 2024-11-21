using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BoardManager : NetworkBehaviour
{
    public Dictionary<ulong, Dictionary<string, int>> pieceCountsByPlayer;
    private BoardGenerator boardGenerator;
    private GameObject[,] tiles;
    private ConfigManager config;
    private GameManager gameManager;
    private UIManager uiManager;

    public void Initialize(BoardGenerator boardGenerator, ConfigManager config, GameManager gameManager, UIManager uiManager)
    {
        this.boardGenerator = boardGenerator;
        this.config = config;
        this.gameManager = gameManager;
        this.uiManager = uiManager;

        pieceCountsByPlayer = new Dictionary<ulong, Dictionary<string, int>>();
        boardGenerator.Initialize(config);
    }

    [ServerRpc]
    public void InitializePieceCountsServerRpc()
    {
        var connectedClients = NetworkManager.Singleton.ConnectedClientsIds;

        for (int i = 0; i < connectedClients.Count; i++)
        {
            ulong clientId = connectedClients[i];
            pieceCountsByPlayer[clientId] = new Dictionary<string, int>();

            foreach (var pieceData in config.PiecesData)
            {
                pieceCountsByPlayer[clientId][pieceData.Name] = pieceData.Count;
            }

            Debug.Log(pieceCountsByPlayer[clientId]);

            var pieceCountsList = new List<KeyValuePair<string, int>>(pieceCountsByPlayer[clientId]);

            foreach (var pieceCount in pieceCountsList)
            {
                UpdatePieceCountClientRpc(clientId, pieceCount.Key, pieceCount.Value);
            }
        }
    }

    [ClientRpc]
    public void UpdatePieceCountClientRpc(ulong clientId, string pieceName, int count)
    {
        if (!pieceCountsByPlayer.ContainsKey(clientId))
        {
            pieceCountsByPlayer[clientId] = new Dictionary<string, int>();
            Debug.Log("Created new player list");
        }

        pieceCountsByPlayer[clientId][pieceName] = count;

        if (NetworkManager.Singleton.LocalClientId == clientId)
            uiManager.UpdatePieceCount(pieceName, count);
    }
    
    [ClientRpc]
    public void InitializeBoardClientRpc()
    {
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