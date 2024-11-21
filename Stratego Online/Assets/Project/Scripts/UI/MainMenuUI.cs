using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject createSessionPanel;
    [SerializeField] private GameObject joinSessionPanel;

    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createSessionPanelButton;
    [SerializeField] private Button joinSessionPanelButton;
    [SerializeField] private Button startGameButton;

    [Header("Dependences")]
    [SerializeField] private MultiplayerManager multiplayerManager;

    private List<GameObject> allPanels = new List<GameObject>();

    private void Start()
    {
        allPanels.Add(mainMenuPanel);
        allPanels.Add(createSessionPanel);
        allPanels.Add(joinSessionPanel);

        startGameButton.interactable = false;

        mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        createSessionPanelButton.onClick.AddListener(OnCreateSessionButtonClick);
        joinSessionPanelButton.onClick.AddListener(OnJoinSessionButtonClick);
        startGameButton.onClick.AddListener(multiplayerManager.ChangeToGameScene);
        multiplayerManager.OnClientCountChange.AddListener(ChangeStartButtonState);
    }

    private void OnMainMenuButtonClick()
    {
        CloseAllPanels();
        mainMenuPanel.SetActive(true);
    }

    private void OnCreateSessionButtonClick()
    {
        CloseAllPanels();
        createSessionPanel.SetActive(true);
    }
    private void OnJoinSessionButtonClick()
    {
        CloseAllPanels();
        joinSessionPanel.SetActive(true);
    }

    private void CloseAllPanels()
    {
        foreach (GameObject panel in allPanels)
            panel.SetActive(false);
    }

    private void ChangeStartButtonState(int clientCount)
    {
        startGameButton.interactable = clientCount == 2;
    }
}
