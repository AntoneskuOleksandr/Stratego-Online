public interface IBoardManager
{
    void Initialize(BoardGenerator boardGenerator, ConfigManager config);
    void GenerateBoard(IGameManager gameManager);
    Tile[,] GetAllTiles();
}
