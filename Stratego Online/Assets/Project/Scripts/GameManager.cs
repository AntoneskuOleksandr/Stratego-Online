using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, IGameManager
{
    private Piece selectedPiece;
    private IBoardManager boardManager;

    public void Initialize(IBoardManager boardManager, UIManager uiManager, ConfigManager config)
    {
        this.boardManager = boardManager;
    }

    public Piece GetSelectedPiece()
    {
        return selectedPiece;
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

    public void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            selectedPiece.Deselect();
            selectedPiece = null;
        }
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

    public void SelectPiece(PieceData pieceData)
    {
        throw new System.NotImplementedException();
    }
}
