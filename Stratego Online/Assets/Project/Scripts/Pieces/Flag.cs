using System.Collections.Generic;

public class Flag : Piece
{
    public override List<Tile> GetPossibleMoves(Tile[,] allTiles)
    {
        return new List<Tile>();
    }
}