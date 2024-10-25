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
    public Vector2Int IndexInMatrix;
    public bool IsLake;
    private Piece occupyingPiece;
    private IGameManager gameManager;
    private Material tileMaterial;
    private Color originalColor;
    private Color highlightedColor;

    public void Initialize(IGameManager gameManager, Color highlightedColor)
    {
        this.gameManager = gameManager;
        tileMaterial = GetComponent<Renderer>().material;
        originalColor = tileMaterial.color;
        this.highlightedColor = highlightedColor;
    }

    private void OnMouseDown()
    {
        if (IsOccupied)
        {
            if (gameManager.GetSelectedPiece() == occupyingPiece)
            {
                gameManager.DeselectPiece();
            }
            else
            {
                gameManager.SelectPiece(occupyingPiece);
            }
        }
        else
        {
            gameManager.TryToMoveSelectedPieceTo(this);
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

    public void Highlight()
    {
        tileMaterial.color = Color.Lerp(originalColor, highlightedColor, 0.5f);
    }

    public void Unhighlight()
    {
        tileMaterial.color = originalColor;
    }
}
