
public interface IBoardManager
{
    void Initialize(BoardGenerator boardGenerator, ConfigManager config);
    void InitializeBoard(IGameManager gameManager);
    Tile[,] GetAllTiles();
    Tile GetTileAt(int x, int y);
}
