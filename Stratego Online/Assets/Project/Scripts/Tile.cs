using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isOccupied;
    public Vector2Int IndexInMatrix;
    private Piece occupyingPiece;
    private GameManager gameManager;

    public void Initialize(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    void OnMouseDown()
    {
        if (isOccupied)
        {
            occupyingPiece.Select();
        }
        else
        {
            gameManager.MoveSelectedPieceTo(this);
        }
    }

    public void PlacePiece(Piece piece)
    {
        occupyingPiece = piece;
        isOccupied = true;
    }

    public void RemovePiece()
    {
        occupyingPiece = null;
        isOccupied = false;
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }
}
