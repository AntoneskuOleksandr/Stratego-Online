using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine;

public class PreGameManager : NetworkBehaviour, IGameManager
{
    private IBoardManager boardManager;
    private UIManager uiManager;
    private PieceData selectedPiece;
    private Piece spawnedPiece;
    private ConfigManager config;
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
            if (tile != null && !tile.IsOccupied.Value && currentPeaceCount > 0)
            {
                PieceData pieceData = selectedPiece;
                ulong clientId = NetworkManager.Singleton.LocalClientId;
                CmdPlacePieceServerRpc(tile.IndexInMatrix.x, tile.IndexInMatrix.y, pieceData.Name, clientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdPlacePieceServerRpc(int x, int y, string pieceName, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(x, y);
        if (tile != null && !tile.IsOccupied.Value && IsTileInPlayerHalf(tile, clientId) && uiManager.GetPieceCurrentCount(pieceName) > 0)
        {
            PieceData pieceData = config.GetPieceDataByName(pieceName);
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
            }
        }
    }


    private bool IsTileInPlayerHalf(Tile tile, ulong clientId)
    {
        int maxRows = config.BoardRows;
        bool isClientOne = (clientId == 0); // например, если ID 0 - это первый игрок
        return isClientOne ? tile.IndexInMatrix.y < maxRows / 2 : tile.IndexInMatrix.y >= maxRows / 2;
    }


    private void TryRemovePiece()
    {
        Tile tile = GetTileUnderMouse();
        if (tile != null && tile.IsOccupied.Value)
        {
            CmdRemovePieceServerRpc(tile.IndexInMatrix.x, tile.IndexInMatrix.y);
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
