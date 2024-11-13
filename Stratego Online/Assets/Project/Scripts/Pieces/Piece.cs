using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public abstract class Piece : NetworkBehaviour
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
        Debug.Log(playerId);
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

    public void Select(ulong clientId)
    {
        RequestHighlightMovesServerRpc(NetworkObjectId, clientId);
    }

    public void Deselect(ulong clientId)
    {
        RequestUnhighlightMovesServerRpc(NetworkObjectId, clientId);
        LowerPiece();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestHighlightMovesServerRpc(ulong pieceId, ulong clientId)
    {
        Piece piece = NetworkManager.Singleton.SpawnManager.SpawnedObjects[pieceId].GetComponent<Piece>();
        if (piece != null)
        {
            List<Vector2Int> movePositions = piece.GetFilteredMovePositions();
            HighlightMovesClientRpc(movePositions.ToArray(), clientId);
        }
    }

    [ClientRpc]
    private void HighlightMovesClientRpc(Vector2Int[] movePositions, ulong clientId)
    {
        Debug.Log("HighlightMovesClientRpc");
        Debug.Log(NetworkManager.Singleton.LocalClientId);
        Debug.Log(clientId);
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log(movePositions.Length);
            highlightedTiles.Clear();
            foreach (Vector2Int pos in movePositions)
            {
                Debug.Log(pos);
                Tile tile = boardManager.GetTileAt(pos.x, pos.y);
                Debug.Log(boardManager);
                if (tile != null)
                {
                    tile.Highlight();
                    highlightedTiles.Add(tile);
                }
            }
            RaisePiece();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestUnhighlightMovesServerRpc(ulong pieceId, ulong clientId)
    {
        UnhighlightMovesClientRpc(clientId);
    }

    [ClientRpc]
    private void UnhighlightMovesClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            foreach (Tile tile in highlightedTiles)
            {
                tile.Unhighlight();
            }
            highlightedTiles.Clear();
        }
    }

    private List<Vector2Int> GetFilteredMovePositions()
    {
        List<Tile> possibleMoves = GetFilteredMoves(boardManager.GetAllTiles());
        List<Vector2Int> movePositions = new List<Vector2Int>();
        foreach (Tile tile in possibleMoves)
        {
            movePositions.Add(tile.IndexInMatrix);
        }
        return movePositions;
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
        Deselect(PlayerId);
    }

    private void RaisePiece()
    {
        Debug.Log("RaisePiece");
        transform.DOMoveY(selectedYPosition, 0.3f);
        Debug.Log("selectedYPosition: " + selectedYPosition);
    }

    private void LowerPiece()
    {
        Debug.Log("LowerPiece");
        transform.DOMoveY(originalYPosition, 0.3f);
    }

    public abstract List<Tile> GetPossibleMoves(Tile[,] allTiles);

    protected List<Tile> GetFilteredMoves(Tile[,] allTiles)
    {
        List<Tile> possibleMoves = GetPossibleMoves(allTiles);
        return possibleMoves.FindAll(tile => !tile.IsLake);
    }

    private new void OnDestroy()
    {
        RequestUnhighlightMovesServerRpc(NetworkObjectId, PlayerId);
    }
}
