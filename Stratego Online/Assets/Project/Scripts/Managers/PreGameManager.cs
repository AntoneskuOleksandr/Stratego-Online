using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PreGameManager : NetworkBehaviour, IGameManager
{
    [SerializeField] private Button GenerateBoardButton;
    private BoardManager boardManager;
    private UIManager uiManager;
    private PieceData selectedPiece;
    private Piece spawnedPiece;
    private ConfigManager config;
    public UnityEvent OnStartGame;
    private ulong clientId;

    private void Start()
    {
        if (!IsHost)
            GenerateBoardButton.gameObject.SetActive(false);

        GenerateBoardButton.onClick.AddListener(() =>
        {
            boardManager.InitializeBoard(this);
            GenerateBoardButton.gameObject.SetActive(false);
        });

        clientId = NetworkManager.Singleton.LocalClientId;
    }

    public void Initialize(BoardManager boardManager, UIManager uiManager, ConfigManager config)
    {
        this.boardManager = boardManager;
        this.uiManager = uiManager;
        this.config = config;
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

        selectedPiece = uiManager.GetSelectedPiece();

        if (selectedPiece != null)
        {
            Tile tile = GetTileUnderMouse();

            if (tile != null)
            {
                PieceData pieceData = selectedPiece;
                CmdPlacePieceServerRpc(tile.IndexInMatrix.Value, pieceData.Name, clientId);
            }
            else
                Debug.LogWarning("Tile = null");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdPlacePieceServerRpc(Vector2Int tileIndex, string pieceName, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(tileIndex.x, tileIndex.y);

        if (tile != null && !tile.IsOccupied.Value && IsTileInPlayerHalf(tile, clientId) && boardManager.pieceCountsByPlayer[clientId][pieceName] > 0)
        {
            PieceData pieceData = config.GetPieceDataByName(pieceName);
            if (pieceData != null)
            {
                GameObject pieceObject = Instantiate(pieceData.Prefab, tile.transform.position, clientId == 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0));
                NetworkObject networkObject = pieceObject.GetComponent<NetworkObject>();
                networkObject.Spawn(true);

                Piece placedPiece = pieceObject.GetComponent<Piece>();
                placedPiece.Initialize(tile, boardManager, pieceData, 0);
                tile.SetPiece(placedPiece);

                boardManager.pieceCountsByPlayer[clientId][pieceData.Name] -= 1;
                boardManager.SendPieceCountsToClient(clientId);

                UpdatePieceCountClientRpc(pieceData.Name, boardManager.pieceCountsByPlayer[clientId][pieceData.Name], clientId);

                if (boardManager.pieceCountsByPlayer[clientId][pieceData.Name] == 0)
                {
                    uiManager.DeselectPiece();
                }
            }
        }
        else
        {
            Debug.LogWarning("Something went wrong. You can't place piece here." +
                "\nTile: " + tile + "\nTile IsOccupied: " + tile.IsOccupied.Value + "\nIsTileInPlayerHalf: " + IsTileInPlayerHalf(tile, clientId)
                + "\nPiece Count: " + boardManager.pieceCountsByPlayer[clientId][pieceName]);
        }
    }

    [ClientRpc]
    private void UpdatePieceCountClientRpc(string pieceName, int count, ulong clientId)
    {
        if (clientId == this.clientId)
            uiManager.UpdatePieceCount(pieceName, count);
    }

    private bool IsTileInPlayerHalf(Tile tile, ulong clientId)
    {
        int maxRows = config.BoardRows;

        if (clientId == 0)
            return tile.IndexInMatrix.Value.y < maxRows / 2 - 1;
        else if (clientId == 1)
            return tile.IndexInMatrix.Value.y > maxRows / 2;
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
                    boardManager.pieceCountsByPlayer[clientId][pieceData.Name] += 1;
                    boardManager.SendPieceCountsToClient(clientId);
                    UpdatePieceCountClientRpc(pieceData.Name, boardManager.pieceCountsByPlayer[clientId][pieceData.Name], clientId);
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
