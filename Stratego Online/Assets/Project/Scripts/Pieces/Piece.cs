using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class Piece : MonoBehaviour
{
    public PieceData PieceData { get; private set; }
    public ulong PlayerId { get; private set; }
    protected Tile currentTile;
    private float originalYPosition;
    private float selectedYPosition;
    private BoardManager boardManager;
    private List<Tile> highlightedTiles = new List<Tile>();

    private void Awake()
    {
        originalYPosition = transform.position.y;
        selectedYPosition = originalYPosition + 0.5f;
    }

    public void Initialize(Tile startTile, BoardManager boardManager, PieceData pieceData, ulong playerId)
    {
        this.PieceData = pieceData;
        this.boardManager = boardManager;
        this.PlayerId = playerId;
        currentTile = startTile;
        transform.position = currentTile.Center;
    }

    public int GetRank()
    {
        return PieceData.Rank;
    }

    public string GetPieceType()
    {
        return PieceData.Name;
    }

    public Tile GetTile()
    {
        return currentTile;
    }

    public bool IsSpecialPiece()
    {
        return PieceData.Name == "Miner" || PieceData.Name == "Spy";
    }

    public void Select()
    {
        HighlightMoves(GetPossibleMoves(boardManager.GetAllTiles()));
        RaisePiece();
    }

    public void Deselect()
    {
        UnhighlightMoves();
        LowerPiece();
    }

    private void HighlightMoves(List<Tile> availableTiles)
    {
        highlightedTiles.Clear();

        foreach (Tile tile in availableTiles)
        {
            if (tile != null)
            {
                tile.Highlight();
                highlightedTiles.Add(tile);
            }
            else
                Debug.Log("Tile = null");
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

    private void OnDestroy()
    {
        UnhighlightMoves();
    }
}
