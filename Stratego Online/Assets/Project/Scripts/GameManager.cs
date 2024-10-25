using UnityEngine;

public class GameManager : MonoBehaviour, IGameManager
{
    private Piece selectedPiece;
    private IBoardManager boardManager;
    private PiecePlacementManager piecePlacementManager;

    public void Initialize(IBoardManager boardManager, PiecePlacementManager piecePlacementManager)
    {
        this.boardManager = boardManager;
        this.piecePlacementManager = piecePlacementManager;
    }

    private void Start()
    {
        boardManager.GenerateBoard(this);
        piecePlacementManager.PlacePiecesRandomly();
    }

    public void SelectPiece(Piece piece)
    {
        selectedPiece = piece;
        piece.Select();
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
