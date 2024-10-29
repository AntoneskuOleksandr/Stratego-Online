public interface IGameManager
{
    void Initialize(IBoardManager boardManager);
    void SelectPiece(Piece piece);
    void TryToMoveSelectedPieceTo(Tile tile);
    Piece GetSelectedPiece();
    void DeselectPiece();
}
