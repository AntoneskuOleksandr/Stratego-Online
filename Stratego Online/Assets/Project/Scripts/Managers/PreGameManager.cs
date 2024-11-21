using Unity.Netcode;
using UnityEngine;

public class PreGameManager : NetworkBehaviour
{
    private BoardManager boardManager;
    private UIManager uiManager;
    private PiecePlacementManager piecePlacementManager;
    private PieceData selectedPiece;
    private ulong clientId;

    public override void OnNetworkSpawn()
    {
        clientId = NetworkManager.Singleton.LocalClientId;
    }

    public void Initialize(BoardManager boardManager, UIManager uiManager, PiecePlacementManager piecePlacementManager)
    {
        this.boardManager = boardManager;
        this.uiManager = uiManager;
        this.piecePlacementManager = piecePlacementManager;

        Debug.Log("PreGameManager: Initialize");

        if (IsHost)
            boardManager.InitializePieceCountsServerRpc();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GetTileUnderMouse() != null)
                TryPlacePiece();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (GetTileUnderMouse() != null)
                piecePlacementManager.TryToRemovePieceServerRpc(GetTileUnderMouse().IndexInMatrix, clientId);
        }
    }

    public void SelectPiece(PieceData pieceData)
    {
        selectedPiece = pieceData;
    }

    public void DeselectPiece()
    {
        selectedPiece = null;
    }

    private void TryPlacePiece()
    {
        if (selectedPiece != null)
        {
            Tile tile = GetTileUnderMouse();
            piecePlacementManager.TryToPlacePieceServerRpc(tile.IndexInMatrix, selectedPiece.Name, clientId);
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

    public Piece GetSelectedPiece()
    {
        return null;
    }
}
