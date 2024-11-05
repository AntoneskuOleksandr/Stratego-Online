using Unity.Netcode;
using UnityEngine;

public class Tile : NetworkBehaviour
{
    public NetworkVariable<bool> IsOccupied = new NetworkVariable<bool>(false);
    public NetworkVariable<Vector2Int> IndexInMatrix = new NetworkVariable<Vector2Int>();
    public NetworkVariable<bool> IsLake = new NetworkVariable<bool>(false);

    private Piece occupyingPiece;
    private IGameManager gameManager;
    private Material tileMaterial;
    private Color originalColor;
    private Color highlightedColor;

    public Vector3 Center
    {
        get
        {
            return GetComponent<Renderer>().bounds.center;
        }
    }

    public void ServerInitialize(IGameManager gameManager, Vector2Int index, bool isLake)
    {
        this.gameManager = gameManager;

        if (IsServer)
        {
            IndexInMatrix.Value = index;
            IsLake.Value = isLake;
        }
    }

    public void ClientInitialize(IGameManager gameManager, Color highlightedColor, Material material)
    {
        this.gameManager = gameManager;

        tileMaterial = material;
        GetComponent<MeshRenderer>().material = tileMaterial;

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

    public void Highlight()
    {
        tileMaterial.color = Color.Lerp(originalColor, highlightedColor, 0.5f);
    }

    public void Unhighlight()
    {
        tileMaterial.color = originalColor;
    }

    public void SetMaterial(Material material)
    {
        if (tileMaterial != material)
        {
            tileMaterial = material;
            GetComponent<MeshRenderer>().material = tileMaterial;
            originalColor = tileMaterial.color;
        }
    }

    public void SetGameManager(IGameManager gameManager)
    {
        this.gameManager = gameManager;
    }
}
