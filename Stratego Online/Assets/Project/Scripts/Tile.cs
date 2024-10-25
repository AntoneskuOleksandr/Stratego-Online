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
    private Piece occupyingPiece;
    private IGameManager gameManager;
    private Material originalMaterial;
    private Material highlightedMaterial;

    public void Initialize(IGameManager gameManager, Material highlightedMaterial)
    {
        this.gameManager = gameManager;
        this.highlightedMaterial = highlightedMaterial;
        originalMaterial = GetComponent<Renderer>().material;
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
        GetComponent<Renderer>().material = highlightedMaterial;
    }

    public void Unhighlight()
    {
        GetComponent<Renderer>().material = originalMaterial;
    }
}
