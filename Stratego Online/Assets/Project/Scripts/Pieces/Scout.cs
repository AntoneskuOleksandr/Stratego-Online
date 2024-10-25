using System.Collections.Generic;

public class Scout : Piece
{
    public override List<Tile> GetPossibleMoves(Tile[,] allTiles)
    {
        List<Tile> possibleMoves = new List<Tile>();
        int x = currentTile.IndexInMatrix.x;
        int y = currentTile.IndexInMatrix.y;

        if (x + 1 < allTiles.GetLength(0) && !allTiles[x + 1, y].IsOccupied)
        {
            possibleMoves.Add(allTiles[x + 1, y]);
        }
        if (x - 1 >= 0 && !allTiles[x - 1, y].IsOccupied)
        {
            possibleMoves.Add(allTiles[x - 1, y]);
        }
        if (y + 1 < allTiles.GetLength(1) && !allTiles[x, y + 1].IsOccupied)
        {
            possibleMoves.Add(allTiles[x, y + 1]);
        }
        if (y - 1 >= 0 && !allTiles[x, y - 1].IsOccupied)
        {
            possibleMoves.Add(allTiles[x, y - 1]);
        }

        return possibleMoves;
    }
}
