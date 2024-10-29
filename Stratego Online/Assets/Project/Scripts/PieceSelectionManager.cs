using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PieceSelectionManager : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public List<PieceData> pieces;

    public void Initialize()
    {
        foreach (var piece in pieces)
        {
            GameObject button = Instantiate(buttonPrefab, buttonContainer);
            Image[] images = button.GetComponentsInChildren<Image>(); foreach (Image img in images)
            {
                if (img.gameObject != button)
                {
                    img.sprite = piece.icon; break;
                }
            }

            button.GetComponentInChildren<TMP_Text>().text = piece.pieceName;
            button.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                OnPieceButtonClick(piece);
            });

        }
    }

    private void OnPieceButtonClick(PieceData piece)
    {
        Debug.Log("Selected Piece: " + piece.pieceName);
    }
}
