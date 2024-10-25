using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class Piece : MonoBehaviour
{
    protected Tile currentTile;
    private float originalYPosition;
    private float selectedYPosition;
    protected IBoardManager boardManager;
    private List<Tile> highlightedTiles = new List<Tile>();

    private void Awake()
    {
        originalYPosition = transform.position.y;
        selectedYPosition = originalYPosition + 0.5f;
    }

    public void Initialize(Tile startTile, IBoardManager boardManager)
    {
        this.boardManager = boardManager;
        currentTile = startTile;
        currentTile.PlacePiece(this);
        transform.position = startTile.Center;
    }

    public void Select()
    {
        HighlightMoves();
        RaisePiece();
    }

    public void Deselect()
    {
        UnhighlightMoves();
        LowerPiece();
    }

    public void MoveToTile(Tile newTile)
    {
        if (currentTile != null)
        {
            currentTile.RemovePiece();
        }
        currentTile = newTile;
        newTile.PlacePiece(this);
        transform.DOMove(newTile.Center, 0.3f);
        Deselect();
    }

    private void RaisePiece()
    {
        transform.DOMoveY(selectedYPosition, 0.3f);
    }

    private void LowerPiece()
    {
        transform.DOMoveY(originalYPosition, 0.3f);
    }

    public abstract List<Tile> GetPossibleMoves(Tile[,] allTiles);

    protected List<Tile> GetFilteredMoves(Tile[,] allTiles)
    {
        List<Tile> possibleMoves = GetPossibleMoves(allTiles);
        return possibleMoves.FindAll(tile => !tile.IsLake); // ��������� ����� �� ��������� �����
    }

    private void HighlightMoves()
    {
        List<Tile> possibleMoves = GetFilteredMoves(boardManager.GetAllTiles());
        highlightedTiles = possibleMoves;
        foreach (Tile tile in possibleMoves)
        {
            tile.Highlight();
        }
    }

    private void UnhighlightMoves()
    {
        foreach (Tile tile in highlightedTiles)
        {
            tile.Unhighlight();
        }
        highlightedTiles.Clear();
    }
}
