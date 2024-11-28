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

    // Highlight the tile by blending the original color with the highlight color (green)
    public void Highlight()
    {
        if (tileRenderer == null)
            tileRenderer = GetComponent<Renderer>();

        // Use MaterialPropertyBlock to modify only this specific tile's material properties
        propertyBlock = new MaterialPropertyBlock();
        tileRenderer.GetPropertyBlock(propertyBlock);

        // Blend the original color with the highlight color
        Color newColor = Color.Lerp(originalColor, highlightedColor, 0.5f); // Adjust blend strength (0.5)
        propertyBlock.SetColor("_BaseColor", newColor); // Update _BaseColor for URP/Lit shader
        tileRenderer.SetPropertyBlock(propertyBlock);
    }

    // Reset the tile's color to its original state
    public void Unhighlight()
    {
        if (tileRenderer == null)
            tileRenderer = GetComponent<Renderer>();

        // Reset the color to the original color
        propertyBlock = new MaterialPropertyBlock();
        tileRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_BaseColor", originalColor);
        tileRenderer.SetPropertyBlock(propertyBlock);
    }

    // Set the material for the tile and store the original color
    public void SetMaterial(Material material)
    {
        tileRenderer = GetComponent<Renderer>();
        tileMaterial = material;
        tileRenderer.material = tileMaterial;

        // Store the original color from the material
        originalColor = tileMaterial.GetColor("_BaseColor");

        // Create a new MaterialPropertyBlock
        propertyBlock = new MaterialPropertyBlock();
        tileRenderer.GetPropertyBlock(propertyBlock);
    }

    // Start the game (set to true)
    public void StartGame()
    {
        isGameStarted = true;
    }
}
