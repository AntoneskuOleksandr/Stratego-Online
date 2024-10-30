using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private MonoBehaviour gameManagerBehaviour;
    [SerializeField] private MonoBehaviour preGameManagerBehaviour;
    [SerializeField] private MonoBehaviour boardManagerBehaviour;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private ConfigManager configManager;
    [SerializeField] private PiecePlacementManager piecePlacementManager;
    [SerializeField] private UIManager uiManager;

    private IGameManager gameManager;
    private IBoardManager boardManager;

    private void Awake()
    {
        gameManager = (IGameManager)preGameManagerBehaviour;
        boardManager = (IBoardManager)boardManagerBehaviour;

        boardManager.Initialize(boardGenerator, configManager);
        gameManager.Initialize(boardManager, uiManager, configManager);
        uiManager.Initialize(gameManager, configManager);
    }

    private void StartGame()
    {

    }
}
