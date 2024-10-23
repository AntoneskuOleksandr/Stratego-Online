using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Piece selectedPiece;
    private UIManager uiManager;
    private BoardManager boardManager;

    public void Initialize(UIManager uiManager, BoardManager boardManager)
    {
        this.uiManager = uiManager;
        this.boardManager = boardManager;
    }

    private void Start()
    {
        boardManager.GenerateBoard(this);
    }

    public void SelectPiece(Piece piece)
    {
        selectedPiece = piece;
    }

    public void MoveSelectedPieceTo(Tile tile)
    {
        if (selectedPiece != null)
        {
            selectedPiece.MoveToTile(tile);
            selectedPiece = null;
        }
    }
}
