using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Bootstrap : NetworkBehaviour
{
    [SerializeField] private MonoBehaviour preGameManagerBehaviour;
    [SerializeField] private MonoBehaviour gameManagerBehaviour;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private ConfigManager configManager;
    [SerializeField] private PiecePlacementManager piecePlacementManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Button InitializeAllButton;

    private IGameManager preGameManager;
    private IGameManager gameManager;

    private void Start()
    {
        if (!IsHost)
            InitializeAllButton.gameObject.SetActive(false);

        InitializeAllButton.onClick.AddListener(() =>
        {
            InitializeClientRpc();
            InitializeAllButton.gameObject.SetActive(false);
        });
    }

    [ClientRpc]
    private void InitializeClientRpc()
    {
        Debug.Log("InitializeBootstrap");
        preGameManager = (IGameManager)preGameManagerBehaviour;
        gameManager = (IGameManager)gameManagerBehaviour;

        cameraController.Initialize();
        boardManager.Initialize(boardGenerator, configManager);
        uiManager.Initialize(preGameManager, configManager, piecePlacementManager);
        piecePlacementManager.Initialize(boardManager, configManager, uiManager);

        var preGameManagerScript = preGameManagerBehaviour as PreGameManager;
        if (preGameManagerScript != null)
        {
            preGameManagerScript.OnStartGame.AddListener(StartGame);
        }

        preGameManager.Initialize(boardManager, uiManager, configManager);
    }

    private void StartGame()
    {
        Tile[,] allTiles = boardManager.GetAllTiles();
        for (int y = 0; y < allTiles.GetLength(1); y++)
        {
            for (int x = 0; x < allTiles.GetLength(0); x++)
            {
                allTiles[x, y].SetGameManager(gameManager);
            }
        }
        Destroy(((Component)preGameManager).gameObject);
        gameManager.Initialize(boardManager, uiManager, configManager);
        gameManager.StartGame();
    }
}
