using System.Collections.Generic;
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

    public Piece GetSelectedPiece()
    {
        return selectedPiece;
    }

    public void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            selectedPiece.Deselect();
            selectedPiece = null;
        }
    }

    public void SelectPiece(Piece piece)
    {
        if (selectedPiece != null)
        {
            selectedPiece.Deselect();
        }
        selectedPiece = piece;
        piece.Select();
    }

    public void TryToMoveSelectedPieceTo(Tile tile)
    {
        if (CanMove(tile))
        {
            selectedPiece.MoveToTile(tile);
            selectedPiece = null;
        }
        else
            DeselectPiece();
    }

    private bool CanMove(Tile tile)
    {
        if (selectedPiece != null)
        {
            List<Tile> possibleMoves = selectedPiece.GetPossibleMoves(boardManager.GetAllTiles());
            if (possibleMoves.Contains(tile))
            {
                return true;
            }
        }
        return false;
    }
}
