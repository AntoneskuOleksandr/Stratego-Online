using UnityEngine;

public interface IGameManager
{
    void Initialize(IBoardManager boardManager, UIManager uiManager, ConfigManager config);
    void StartGame();
    void SelectPiece(Piece piece);
    void SelectPiece(PieceData pieceData);
    void TryToMoveSelectedPieceTo(Tile tile);
    Piece GetSelectedPiece();
    void DeselectPiece();
}
