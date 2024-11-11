using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PreGameManager : NetworkBehaviour
{
    [SerializeField] private Button GenerateBoardButton;
    public UnityEvent OnStartGame;
    private BoardManager boardManager;
    private UIManager uiManager;
    private PiecePlacementManager piecePlacementManager;
    private ulong clientId;

    private void Start()
    {
        if (!IsHost)
            GenerateBoardButton.gameObject.SetActive(false);

        clientId = NetworkManager.Singleton.LocalClientId;
    }

    public void Initialize(BoardManager boardManager, UIManager uiManager, PiecePlacementManager piecePlacementManager)
    {
        this.boardManager = boardManager;
        this.uiManager = uiManager;
        this.piecePlacementManager = piecePlacementManager;
        InitializePieceCounts();
    }

    private void InitializePieceCounts()
    {
        boardManager.InitializePieceCountsServerRpc();
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
        if (uiManager == null)
            return;

        PieceData selectedPiece = uiManager.GetSelectedPiece();

        if (selectedPiece != null)
        {
            Tile tile = GetTileUnderMouse();

            if (tile != null)
            {
                piecePlacementManager.PlacePieceServerRpc(tile.IndexInMatrix.Value, selectedPiece.Name, clientId);
            }
            else
                Debug.LogWarning("Tile = null");
        }
    }

    private void TryRemovePiece()
    {
        Tile tile = GetTileUnderMouse();
        piecePlacementManager.TryRemovePiece(tile, clientId);
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
        return null;
    }

    public void TryToMoveSelectedPieceTo(Tile tile)
    {
    }

    public void SelectPiece(PieceData pieceData)
    {
        // Handle piece selection as needed
    }
}
