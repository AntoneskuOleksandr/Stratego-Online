using Unity.Netcode;
using UnityEngine;

public class Bootstrap : NetworkBehaviour
{
    [SerializeField] private PreGameManager preGameManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private ConfigManager configManager;
    [SerializeField] private PiecePlacementManager piecePlacementManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraController cameraController;
    private int clientsReadyToPlay = 0;
    private int connectedClient = 0;

    public override void OnNetworkSpawn()
    {
        OnClientConnectedServerRpc();

        uiManager.readyButton.onClick.AddListener(TryToStartGameServerRpc);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnClientConnectedServerRpc()
    {
        connectedClient++;

        Debug.Log("OnClientConnectedServerRpc");

        if (connectedClient == NetworkManager.ConnectedClients.Count)
            InitializeGameSetupServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializeGameSetupServerRpc()
    {
        InitializeClientRpc();
        GenerateBoardClientRpc();
    }

    [ClientRpc]
    private void InitializeClientRpc()
    {
        Debug.Log("InitializeBootstrap");

        cameraController.Initialize();
        boardManager.Initialize(boardGenerator, configManager, gameManager);
        uiManager.Initialize(preGameManager, configManager, piecePlacementManager);
        piecePlacementManager.Initialize(boardManager, uiManager, configManager);

        preGameManager.Initialize(boardManager, uiManager, piecePlacementManager);
    }

    [ClientRpc]
    private void GenerateBoardClientRpc()
    {
        Debug.Log("GenerateBoardClientRpc");

        boardManager.InitializeBoardClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryToStartGameServerRpc()
    {
        Debug.Log("TryToStartGame");
        clientsReadyToPlay++;

        if (clientsReadyToPlay == NetworkManager.ConnectedClients.Count)
            StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        Debug.Log("StartGameClientRpc");
        gameManager.Initialize(boardManager, uiManager, piecePlacementManager);

        Tile[,] allTiles = boardManager.GetAllTiles();

        for (int y = 0; y < allTiles.GetLength(1); y++)
        {
            for (int x = 0; x < allTiles.GetLength(0); x++)
            {
                allTiles[x, y].StartGame();
            }
        }

        Destroy(preGameManager.gameObject);

        gameManager.StartGame();
    }
}
