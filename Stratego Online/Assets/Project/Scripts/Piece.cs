using UnityEngine;
using DG.Tweening;

public class Piece : MonoBehaviour
{
    private Tile currentTile;
    private float originalYPosition;
    private float selectedYPosition;

    private void Awake()
    {
        originalYPosition = transform.position.y;
        selectedYPosition = originalYPosition + 0.5f;
    }

    public void Initialize(Tile startTile)
    {
        currentTile = startTile;
        currentTile.PlacePiece(this);
        transform.position = startTile.Center;
    }

    public void Select()
    {
        Debug.Log("Piece selected");
        RaisePiece();
    }

    public void Deselect()
    {
        Debug.Log("Piece deselected");
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
}
