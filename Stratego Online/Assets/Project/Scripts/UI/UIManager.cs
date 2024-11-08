using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button randomPlacementButton;
    [SerializeField] private Transform buttonContainer;
    private Dictionary<string, int> pieceCounts = new Dictionary<string, int>();
    private List<PieceData> originalPiecesData;
    private Dictionary<string, PieceButton> pieceButtons = new Dictionary<string, PieceButton>();
    private PieceData selectedPiece;
    private IGameManager gameManager;

    public void Initialize(IGameManager gameManager, ConfigManager config, PiecePlacementManager piecePlacementManager)
    {
        this.gameManager = gameManager;
        originalPiecesData = config.PiecesData;
        GenerateButtons();
        readyButton.onClick.AddListener(() =>
        {
            gameManager.StartGame();
            HideUI();
        });
        randomPlacementButton.onClick.AddListener(() =>
        {
            piecePlacementManager.PlacePiecesRandomly();
            randomPlacementButton.gameObject.SetActive(false);
        });
        readyButton.interactable = false;
        CheckReadyButtonStatus();
    }

    private void GenerateButtons()
    {
        foreach (var pieceData in originalPiecesData)
        {
            GameObject buttonObject = Instantiate(buttonPrefab, buttonContainer);
            buttonObject.name = pieceData.Name;
            PieceButton pieceButton = buttonObject.GetComponent<PieceButton>();
            pieceButton.SetPieceData(pieceData, pieceData.Count);
            pieceButtons[pieceData.Name] = pieceButton;
            pieceCounts[pieceData.Name] = pieceData.Count;
            pieceButton.Button.onClick.AddListener(() =>
            {
                OnPieceButtonClick(pieceData);
            });
        }
    }

    private void OnPieceButtonClick(PieceData pieceData)
    {
        if (pieceCounts[pieceData.Name] > 0)
        {
            selectedPiece = pieceData;
            gameManager.SelectPiece(selectedPiece);
        }
        else
        {
            Debug.Log("No more pieces of type: " + pieceData.Name);
        }
    }

    public void UpdatePieceCount(string pieceName, int count)
    {
        Debug.Log("UpdatePieceCount; Client: " + NetworkManager.Singleton.LocalClientId);
        if (pieceButtons.ContainsKey(pieceName))
        {
            pieceCounts[pieceName] = count;
            pieceButtons[pieceName].UpdatePieceCount(count);
            CheckReadyButtonStatus();
        }
    }

    public PieceData GetSelectedPiece()
    {
        return selectedPiece;
    }

    public void DeselectPiece()
    {
        selectedPiece = null;
    }

    public int GetPieceCurrentCount(string name)
    {
        return pieceCounts[name];
    }

    private void HideUI()
    {
        this.gameObject.SetActive(false);
    }

    private void CheckReadyButtonStatus()
    {
        bool allPiecesPlaced = true;

        foreach (var count in pieceCounts.Values)
        {
            if (count > 0)
            {
                allPiecesPlaced = false;
                break;
            }
        }

        readyButton.interactable = allPiecesPlaced;
    }
}
