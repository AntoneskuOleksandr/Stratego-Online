using Unity.Netcode;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsOccupied;
    public Vector2Int IndexInMatrix;
    public bool IsLake;

    private Piece occupyingPiece;
    private Material tileMaterial;
    private Color originalColor;
    private Color highlightedColor;
    private MaterialPropertyBlock propertyBlock;
    private Renderer tileRenderer;
    private bool isGameStarted;
    private GameManager gameManager;

    public Vector3 Center
    {
        get
        {
            return GetComponent<Renderer>().bounds.center;
        }
    }

    public void Initialize(Vector2Int index, GameManager gameManager, Color highlightedColor)
    {
        isGameStarted = false;
        this.gameManager = gameManager;
        IndexInMatrix = index;

        this.highlightedColor = highlightedColor;
    }

    private void OnMouseDown()
    {
        if (!isGameStarted)
            return;

        Debug.Log("OnMouseDown");
        gameManager.HandleTileActionServerRpc(IndexInMatrix, NetworkManager.Singleton.LocalClientId);
    }

    public void PlacePiece(Piece piece)
    {
        occupyingPiece = piece;
        IsOccupied = true;
    }

    public void SetPiece(Piece piece)
    {
        occupyingPiece = piece;
        IsOccupied = true;
    }

    public void RemovePiece()
    {
        occupyingPiece = null;
        IsOccupied = false;
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

    public void SetMaterial(Material material)
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
