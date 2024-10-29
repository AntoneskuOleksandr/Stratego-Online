using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] Transform buttonContainer;
    [SerializeField] List<PieceData> pieces;
    private Dictionary<string, PieceButton> pieceButtons = new Dictionary<string, PieceButton>();

    public void Initialize()
    {
        GenerateButtons();
    }

    private void GenerateButtons()
    {
        foreach (var piece in pieces)
        {
            GameObject buttonObject = Instantiate(buttonPrefab, buttonContainer);
            buttonObject.name = piece.Name;
            PieceButton pieceButton = buttonObject.GetComponent<PieceButton>();
            pieceButton.SetPieceData(piece, piece.Count);
            pieceButtons[piece.Name] = pieceButton;

            pieceButton.Button.onClick.AddListener(() =>
            {
                OnPieceButtonClick(piece);
            });
        }
    }

    public void UpdatePieceCount(string pieceName, int count)
    {
        if (pieceButtons.ContainsKey(pieceName))
        {
            pieceButtons[pieceName].UpdatePieceCount(count);
        }
    }

    private void OnPieceButtonClick(PieceData piece)
    {
        Debug.Log("Selected Piece: " + piece.Name);
        //UpdatePieceCount(piece.Name, newCount);
    }
}
