using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public Vector3 Center
    {
        get
        {
            return GetComponent<Renderer>().bounds.center;
        }
        private set { }
    }
    private Piece occupyingPiece;
    private IGameManager gameManager;

    public void Initialize(IGameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    private void OnMouseDown()
    {
        if (IsOccupied)
        {
            gameManager.SelectPiece(occupyingPiece);
        }
        else
        {
            gameManager.MoveSelectedPieceTo(this);
        }
    }

    public void PlacePiece(Piece piece)
    {
        occupyingPiece = piece;
        IsOccupied = true;
    }

    public void RemovePiece()
    {
        occupyingPiece = null;
        IsOccupied = false;
    }

    public bool IsTileOccupied()
    {
        return IsOccupied;
    }
}
