using UnityEngine;

public class Piece : MonoBehaviour
{
    private Tile currentTile;

    public void Select()
    {
        // Логика выбора фигуры
    }

    public void MoveToTile(Tile newTile)
    {
        if (currentTile != null)
        {
            currentTile.RemovePiece();
        }
        currentTile = newTile;
        newTile.PlacePiece(this);
        // Обновление позиции в пространстве
        transform.position = newTile.transform.position;
    }

    public void Initialize(Tile startTile)
    {
        currentTile = startTile;
        currentTile.PlacePiece(this);
        transform.position = startTile.transform.position;
    }
}
