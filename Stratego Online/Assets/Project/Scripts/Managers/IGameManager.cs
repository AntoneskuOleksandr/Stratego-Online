using UnityEngine;

public interface IGameManager
{
    void Initialize(BoardManager boardManager, UIManager uiManager, PiecePlacementManager piecePlacementManager);
    void StartGame();
    void SelectPiece(Piece piece);
    void SelectPiece(PieceData pieceData);
    void TryToMoveSelectedPieceTo(Tile tile);
    Piece GetSelectedPiece();
    void DeselectPiece();
}
