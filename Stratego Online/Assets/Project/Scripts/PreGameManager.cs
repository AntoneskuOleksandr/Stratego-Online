using UnityEngine;
using UnityEngine.Events;

public class PreGameManager : MonoBehaviour, IGameManager
{
    private IBoardManager boardManager;
    private UIManager uiManager;
    private PieceData selectedPiece;
    private Piece spawnedPiece;
    private ConfigManager config;
    private bool isPlayerOne = true;
    private int currentPeaceCount = 1;
    public UnityEvent OnStartGame;

    public void Initialize(IBoardManager boardManager, UIManager uiManager, ConfigManager config)
    {
        this.boardManager = boardManager;
        this.uiManager = uiManager;
        this.config = config;
        boardManager.GenerateBoard(this);
    }

    public void StartGame()
    {
        OnStartGame.Invoke();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPlacePiece();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            TryRemovePiece();
        }
    }

    private void TryPlacePiece()
    {
        selectedPiece = uiManager.GetSelectedPiece();
        if (selectedPiece != null)
        {
            Tile tile = GetTileUnderMouse();
            currentPeaceCount = uiManager.GetPieceCurrentCount(selectedPiece.Name);
            if (tile != null && !tile.IsOccupied && IsTileInPlayerHalf(tile) && currentPeaceCount > 0)
            {
                GameObject pieceObject = Instantiate(selectedPiece.Prefab, tile.transform.position, Quaternion.identity);
                Piece placedPiece = pieceObject.GetComponent<Piece>();
                placedPiece.Initialize(tile, boardManager, selectedPiece, 0);
                tile.PlacePiece(placedPiece);

                int newCount = uiManager.GetPieceCurrentCount(selectedPiece.Name) - 1;
                uiManager.UpdatePieceCount(selectedPiece.Name, newCount);

                if (newCount == 0)
                {
                    uiManager.DeselectPiece();
                }
            }
        }
    }

    private bool IsTileInPlayerHalf(Tile tile)
    {
        int maxRows = config.BoardRows;
        return isPlayerOne ? tile.IndexInMatrix.y < maxRows / 2 - 1 : tile.IndexInMatrix.y >= maxRows / 2 + 1;
    }

    private void TryRemovePiece()
    {
        Tile tile = GetTileUnderMouse();
        if (tile != null && tile.IsOccupied)
        {
            Piece piece = tile.GetPiece();
            if (piece != null)
            {
                PieceData pieceData = piece.PieceData;
                Destroy(piece.gameObject);
                tile.RemovePiece();

                if (pieceData != null)
                {
                    int newCount = uiManager.GetPieceCurrentCount(pieceData.Name) + 1;
                    uiManager.UpdatePieceCount(pieceData.Name, newCount);
                }
            }
        }
    }

    private Tile GetTileUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }

    public void SelectPiece(Piece piece)
    {
    }

    public void DeselectPiece()
    {
    }

    public Piece GetSelectedPiece()
    {
        return spawnedPiece;
    }

    public void TryToMoveSelectedPieceTo(Tile tile)
    {
        // ����� �������� ���������������, ���� ��� �� ����� � PreGameManager
    }

    public void SelectPiece(PieceData pieceData)
    {
        selectedPiece = pieceData;
    }
}
