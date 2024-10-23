using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private ConfigManager configManager;

    private void Awake()
    {
        boardManager.Initialize(boardGenerator, configManager);
        gameManager.Initialize(uiManager, boardManager);
    }
}
