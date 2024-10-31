using System.Collections.Generic;

public abstract class SingleStepPiece : Piece
{
    public override List<Tile> GetPossibleMoves(Tile[,] allTiles)
    {
        List<Tile> possibleMoves = new List<Tile>();
        int x = currentTile.IndexInMatrix.x;
        int y = currentTile.IndexInMatrix.y;

        TryAddTile(allTiles, x + 1, y, possibleMoves);
        TryAddTile(allTiles, x - 1, y, possibleMoves);
        TryAddTile(allTiles, x, y + 1, possibleMoves);
        TryAddTile(allTiles, x, y - 1, possibleMoves);

        return possibleMoves;
    }

    private void TryAddTile(Tile[,] allTiles, int x, int y, List<Tile> possibleMoves)
    {
        if (x >= 0 && y >= 0 && x < allTiles.GetLength(0) && y < allTiles.GetLength(1))
        {
            Tile tile = allTiles[x, y];
            if (!tile.IsLake)
            {
                if (!tile.IsOccupied || (tile.IsOccupied && tile.GetPiece().PlayerId != PlayerId))
                {
                    possibleMoves.Add(tile);
                }
            }
        }
    }
}
