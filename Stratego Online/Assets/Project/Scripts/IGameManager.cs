public interface IGameManager
{
    void Initialize(IBoardManager boardManager, PiecePlacementManager piecePlacementManager);
    void SelectPiece(Piece piece);
    void MoveSelectedPieceTo(Tile tile);
    Piece GetSelectedPiece();
    void DeselectPiece();
}
