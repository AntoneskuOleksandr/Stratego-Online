using System.Collections.Generic;

public class Bomb : Piece
{
    public override List<Tile> GetPossibleMoves(Tile[,] allTiles)
    {
        return new List<Tile>();
    }
}