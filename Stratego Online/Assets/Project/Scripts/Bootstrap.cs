using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Button InitializeAllButton;
    [SerializeField] private Button GenerateBoardButton;

    private void Start()
    {
        if (IsHost)
        {
            InitializeAllButton.onClick.AddListener(() =>
            {
                InitializeClientRpc();
                InitializeAllButton.gameObject.SetActive(false);
            });

            GenerateBoardButton.onClick.AddListener(() =>
            {
                boardManager.InitializeBoardClientRpc();
                GenerateBoardButton.gameObject.SetActive(false);
            });

            uiManager.readyButton.onClick.AddListener(StartGameServerRpc);
        }
        else
            InitializeAllButton.gameObject.SetActive(false);
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

    [ServerRpc]
    private void StartGameServerRpc()
    {
        Debug.Log("StartGameServerRpc");
        gameManager.Initialize(boardManager, uiManager, piecePlacementManager);

        Tile[,] allTiles = boardManager.GetAllTiles();

        for (int y = 0; y < allTiles.GetLength(1); y++)
        {
            for (int x = 0; x < allTiles.GetLength(0); x++)
            {
                //StartTileGameClientRpc(allTiles[x, y].NetworkObjectId);
            }
        }

        Destroy(preGameManager.gameObject);

        gameManager.StartGame();
    }

    [ClientRpc]
    private void StartTileGameClientRpc(ulong tileNetworkObjectId)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tileNetworkObjectId];
        if (networkObject != null)
        {
            Tile tile = networkObject.GetComponent<Tile>();
            if (tile != null)
            {
                tile.StartGame();
            }
        }
    }
}
