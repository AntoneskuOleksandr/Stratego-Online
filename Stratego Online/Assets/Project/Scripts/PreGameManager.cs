using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

public class PreGameManager : NetworkBehaviour, IGameManager
{
    [SerializeField] private Button GenerateBoardButton;
    private IBoardManager boardManager;
    private UIManager uiManager;
    private PieceData selectedPiece;
    private Piece spawnedPiece;
    private ConfigManager config;
    private int currentPeaceCount = 1;
    public UnityEvent OnStartGame;

    private void Awake()
    {
        GenerateBoardButton.onClick.AddListener(() =>
        {
            boardManager.InitializeBoard(this);
            GenerateBoardButton.gameObject.SetActive(false);
        });
    }

    public void Initialize(IBoardManager boardManager, UIManager uiManager, ConfigManager config)
    {
        this.boardManager = boardManager;
        this.uiManager = uiManager;
        this.config = config;
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

        selectedPiece = uiManager.GetSelectedPiece();
        if (selectedPiece != null)
        {
            Tile tile = GetTileUnderMouse();
            currentPeaceCount = uiManager.GetPieceCurrentCount(selectedPiece.Name);
            if (tile != null && !tile.IsOccupied.Value && currentPeaceCount > 0)
            {
                PieceData pieceData = selectedPiece;
                ulong clientId = NetworkManager.Singleton.LocalClientId;
                CmdPlacePieceServerRpc(tile.IndexInMatrix.Value.x, tile.IndexInMatrix.Value.y, pieceData.Name, clientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdPlacePieceServerRpc(int x, int y, string pieceName, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(x, y);

        Debug.Log(tile.IsOccupied.Value);
        if (tile != null && !tile.IsOccupied.Value && IsTileInPlayerHalf(tile, clientId) && uiManager.GetPieceCurrentCount(pieceName) > 0)
        {
            PieceData pieceData = config.GetPieceDataByName(pieceName);
            Debug.Log(pieceData);
            if (pieceData != null)
            {
                GameObject pieceObject = Instantiate(pieceData.Prefab, tile.transform.position, Quaternion.identity);
                NetworkObject networkObject = pieceObject.GetComponent<NetworkObject>();
                networkObject.Spawn(true);

                Piece placedPiece = pieceObject.GetComponent<Piece>();
                placedPiece.Initialize(tile, boardManager, pieceData, 0);

                tile.SetPiece(placedPiece);

                int newCount = uiManager.GetPieceCurrentCount(pieceName) - 1;
                uiManager.UpdatePieceCount(pieceName, newCount);

                if (newCount == 0)
                {
                    uiManager.DeselectPiece();
                }
                Debug.Log(tile.IsOccupied.Value);
            }
        }
    }

    private bool IsTileInPlayerHalf(Tile tile, ulong clientId)
    {
        Debug.Log(clientId);
        int maxRows = config.BoardRows;

        if (clientId == 0)
            if (tile.IndexInMatrix.Value.y < maxRows / 2 - 1)
                return true;
            else
                return false;
        else if (clientId == 1)
            if (tile.IndexInMatrix.Value.y > maxRows / 2)
                return true;
            else
                return false;
        else
        {
            Debug.LogError("Something wrong with clientId");
            return false;
        }
    }

    private void TryRemovePiece()
    {
        Tile tile = GetTileUnderMouse();
        if (tile != null && tile.IsOccupied.Value)
        {
            CmdRemovePieceServerRpc(tile.IndexInMatrix.Value.x, tile.IndexInMatrix.Value.y);
        }
    }

    [ServerRpc]
    private void CmdRemovePieceServerRpc(int x, int y)
    {
        Tile tile = boardManager.GetTileAt(x, y);
        if (tile != null && tile.IsOccupied.Value)
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
    }

    public void SelectPiece(PieceData pieceData)
    {
        selectedPiece = pieceData;
    }
}
