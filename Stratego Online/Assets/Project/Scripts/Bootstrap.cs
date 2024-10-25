using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private MonoBehaviour gameManagerBehaviour;
    [SerializeField] private MonoBehaviour boardManagerBehaviour;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private ConfigManager configManager;
    [SerializeField] private PiecePlacementManager piecePlacementManager;

    private IGameManager gameManager;
    private IBoardManager boardManager;

    private void Awake()
    {
        gameManager = (IGameManager)gameManagerBehaviour;
        boardManager = (IBoardManager)boardManagerBehaviour;

        boardManager.Initialize(boardGenerator, configManager);
        gameManager.Initialize(boardManager, piecePlacementManager);
        piecePlacementManager.Initialize(boardManager, configManager);
    }
}
