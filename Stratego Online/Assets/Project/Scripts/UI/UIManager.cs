using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Button randomPlacementButton;
    [SerializeField] private Transform buttonContainer;
    public Button readyButton;
    private Dictionary<string, int> pieceCounts = new Dictionary<string, int>();
    private List<PieceData> originalPiecesData;
    private Dictionary<string, PieceButton> pieceButtons = new Dictionary<string, PieceButton>();
    private PieceData selectedPiece;
    private PreGameManager preGameManager;
    private PiecePlacementManager piecePlacementManager;

    public void Initialize(PreGameManager preGameManager, ConfigManager config, PiecePlacementManager piecePlacementManager)
    {
        this.preGameManager = preGameManager;
        this.piecePlacementManager = piecePlacementManager;
        originalPiecesData = config.PiecesData;
        GenerateButtons();
        readyButton.onClick.AddListener(() =>
        {
            OnReadyButtonClick();
        });
        randomPlacementButton.onClick.AddListener(() =>
        {
            piecePlacementManager.PlacePiecesRandomlyServerRpc(NetworkManager.Singleton.LocalClientId);
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
            preGameManager.SelectPiece(selectedPiece);
        }
        else
        {
            Debug.Log("No more pieces of type: " + pieceData.Name);
        }
    }

    private void OnReadyButtonClick()
    {
        HideUI();
        Destroy(preGameManager.gameObject);
    }

    private void OnRandomPlacementButtonClick()
    {
        piecePlacementManager.PlacePiecesRandomlyServerRpc(NetworkManager.Singleton.LocalClientId);
        randomPlacementButton.gameObject.SetActive(false);
    }

    public void UpdatePieceCount(string pieceName, int count)
    {
        if (pieceButtons.ContainsKey(pieceName))
        {
            pieceCounts[pieceName] = count;
            pieceButtons[pieceName].UpdatePieceCount(count);
            CheckReadyButtonStatus();

            if (count == 0)
                preGameManager.DeselectPiece();
        }
    }

    public int GetPieceCurrentCount(string name)
    {
        return pieceCounts[name];
    }

    private void HideUI()
    {
        gameObject.SetActive(false);
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