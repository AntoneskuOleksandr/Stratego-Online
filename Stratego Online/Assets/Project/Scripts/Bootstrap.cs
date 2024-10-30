using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private MonoBehaviour preGameManagerBehaviour;
    [SerializeField] private MonoBehaviour gameManagerBehaviour;
    [SerializeField] private MonoBehaviour boardManagerBehaviour;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private ConfigManager configManager;
    [SerializeField] private UIManager uiManager;

    private IGameManager preGameManager;
    private IGameManager gameManager;
    private IBoardManager boardManager;

    private void Awake()
    {
        preGameManager = (IGameManager)preGameManagerBehaviour;
        gameManager = (IGameManager)gameManagerBehaviour;
        boardManager = (IBoardManager)boardManagerBehaviour;

        boardManager.Initialize(boardGenerator, configManager);
        uiManager.Initialize(preGameManager, configManager);

        // Подписываемся на событие OnStartGame
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

        gameManager.Initialize(boardManager, uiManager, configManager);
        gameManager.StartGame();
    }
}
