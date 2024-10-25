using UnityEngine;

public class Piece : MonoBehaviour
{
    private Tile currentTile;

    public void Initialize(Tile startTile)
    {
        currentTile = startTile;
        currentTile.PlacePiece(this);
        transform.position = startTile.Center;
    }

    public void Select()
    {
        Debug.Log("Piece selected");
    }

    public void MoveToTile(Tile newTile)
    {
        if (currentTile != null)
        {
            currentTile.RemovePiece();
        }

        currentTile = newTile;
        newTile.PlacePiece(this);
        transform.position = newTile.Center;
    }
}
