using Unity.Netcode;
using UnityEngine;

public class Tile : NetworkBehaviour
{
    public NetworkVariable<bool> IsOccupied = new NetworkVariable<bool>(false);
    public NetworkVariable<Vector2Int> IndexInMatrix = new NetworkVariable<Vector2Int>();
    public NetworkVariable<bool> IsLake = new NetworkVariable<bool>(false);

    private Piece occupyingPiece;
    private GameManager gameManager;
    private Material tileMaterial;
    private Color originalColor;
    private Color highlightedColor;
    private MaterialPropertyBlock propertyBlock;
    private Renderer tileRenderer;
    private bool isGameStarted;

    public Vector3 Center
    {
        get
        {
            return GetComponent<Renderer>().bounds.center;
        }
    }

    public void ServerInitialize(Vector2Int index, bool isLake, GameManager gameManager)
    {
        if (IsServer)
        {
            this.gameManager = gameManager;
            IndexInMatrix.Value = index;
            IsLake.Value = isLake;
        }
    }

    public void ClientInitialize(Color highlightedColor, Material material)
    {
        isGameStarted = false;
        this.highlightedColor = highlightedColor;

        SetMaterial(material);
    }

    private void OnMouseDown()
    {
        Debug.Log("GameManager: " + gameManager);
        Debug.Log("Owner: " + OwnerClientId);

        if (!isGameStarted)
            return;

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
        propertyBlock.SetColor("_Color", Color.Lerp(originalColor, highlightedColor, 0.5f));
        tileRenderer.SetPropertyBlock(propertyBlock);
    }

    public void Unhighlight()
    {
        propertyBlock.SetColor("_Color", originalColor);
        tileRenderer.SetPropertyBlock(propertyBlock);
    }

    private void SetMaterial(Material material)
    {
        tileRenderer = GetComponent<Renderer>();
        tileMaterial = material;
        tileRenderer.material = tileMaterial;

        originalColor = tileMaterial.color;

        propertyBlock = new MaterialPropertyBlock();
        tileRenderer.GetPropertyBlock(propertyBlock);
    }

    public void StartGame()
    {
        isGameStarted = true;
    }
}
