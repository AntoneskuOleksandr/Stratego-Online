using System.Collections.Generic;
using UnityEngine;

public class Scout : Piece
{
    public override List<Tile> GetPossibleMoves(Tile[,] allTiles)
    {
        List<Tile> possibleMoves = new List<Tile>();
        int x = currentTile.IndexInMatrix.x;
        int y = currentTile.IndexInMatrix.y;

        for (int i = y + 1; i < allTiles.GetLength(1); i++)
        {
            if (!AddTileIfValid(allTiles[x, i], possibleMoves))
                break;
        }

        for (int i = y - 1; i >= 0; i--)
        {
            if (!AddTileIfValid(allTiles[x, i], possibleMoves))
                break;
        }

        for (int i = x + 1; i < allTiles.GetLength(0); i++)
        {
            if (!AddTileIfValid(allTiles[i, y], possibleMoves))
                break;
        }

        for (int i = x - 1; i >= 0; i--)
        {
            if (!AddTileIfValid(allTiles[i, y], possibleMoves))
                break;
        }

        return possibleMoves;
    }

    private bool AddTileIfValid(Tile tile, List<Tile> possibleMoves)
    {
        if (tile.IsLake)
        {
            return false;
        }

        if (tile.IsOccupied && tile.GetPiece().PlayerId != PlayerId)
        {
            possibleMoves.Add(tile);
            return false;
        }

        if (!tile.IsOccupied)
        {
            possibleMoves.Add(tile);
        }

        return true;
    }

}
