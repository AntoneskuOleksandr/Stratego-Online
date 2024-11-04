using Unity.Netcode;
using UnityEngine;

public class Tile : NetworkBehaviour
{
    public NetworkVariable<bool> IsOccupied = new NetworkVariable<bool>(false);
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
        if (IsOwner)
        {
            if (IsOccupied.Value)
            {
                if (gameManager.GetSelectedPiece() == occupyingPiece)
                {
                    gameManager.DeselectPiece();
                }
                else if (gameManager.GetSelectedPiece() == null)
                {
                    gameManager.SelectPiece(occupyingPiece);
                }
                else
                {
                    gameManager.TryToMoveSelectedPieceTo(this);
                }
            }
            else
            {
                gameManager.TryToMoveSelectedPieceTo(this);
            }
        }
    }

    public void PlacePiece(Piece piece)
    {
        occupyingPiece = piece;
        IsOccupied.Value = true;
    }

    public void SetPiece(Piece piece)
    {
        if (IsServer)
        {
            occupyingPiece = piece;
            IsOccupied.Value = true;
        }
    }

    public void RemovePiece()
    {
        occupyingPiece = null;
        IsOccupied.Value = false;
    }

    public Piece GetPiece()
    {
        return occupyingPiece;
    }

    public void SetGameManager(IGameManager newGameManager)
    {
        this.gameManager = newGameManager;
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
