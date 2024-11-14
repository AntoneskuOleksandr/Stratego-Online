using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private Dictionary<ulong, Piece> selectedPieces = new Dictionary<ulong, Piece>();
    private BoardManager boardManager;
    private ClientRpcParams clientRpcParams = new ClientRpcParams { };

    public void Initialize(BoardManager boardManager, UIManager uiManager, PiecePlacementManager piecePlacementManager)
    {
        this.boardManager = boardManager;
    }

    public void StartGame()
    {
        Debug.Log("Game has started!");
    }

    public Piece GetSelectedPiece(ulong clientId)
    {
        return selectedPieces.ContainsKey(clientId) ? selectedPieces[clientId] : null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleTileActionServerRpc(Vector2Int tileIndex, ulong clientId)
    {
        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        Tile tile = boardManager.GetTileAt(tileIndex);
        Piece occupyingPiece = tile.GetPiece();

        if (tile.IsOccupied && occupyingPiece.PlayerId == clientId)
        {
            Debug.Log(GetSelectedPiece(clientId));
            if (GetSelectedPiece(clientId) == null) //If selectedPiece is null
            {
                SelectPieceServerRpc(tileIndex, clientId); //Select piece at Client
            }
            else if (GetSelectedPiece(clientId) == occupyingPiece) //If pressed on selected piece
            {
                DeselectPieceServerRpc(clientId);
            }
            else if (occupyingPiece.PlayerId == clientId) //If pressed on another peace on same team
            {
                Debug.Log("Deselect old piece and Select new piece");
                DeselectPieceServerRpc(clientId);
                SelectPieceServerRpc(tileIndex, clientId);
            }
        }
        else if (GetSelectedPiece(clientId) != null)
        {
            TryToMoveSelectedPieceServerRpc(tileIndex, clientId);
        }
    }

    [ServerRpc]
    public void SelectPieceServerRpc(Vector2Int pieceLocation, ulong clientId)
    {
        selectedPieces[clientId] = boardManager.GetTileAt(pieceLocation).GetPiece();
        SelectPieceClientRpc(pieceLocation, clientId, clientRpcParams);
    }

    [ClientRpc]
    public void SelectPieceClientRpc(Vector2Int pieceLocation, ulong clientId, ClientRpcParams clientRpcParams)
    {
        Debug.Log("SelectPieceClientRpc " + clientId);
        Piece piece = boardManager.GetTileAt(pieceLocation).GetPiece();
        selectedPieces[clientId] = piece;
        piece.Select();
    }

    [ServerRpc]
    public void DeselectPieceServerRpc(ulong clientId)
    {
        selectedPieces[clientId] = null;
        DeselectPieceClientRpc(clientId, clientRpcParams);
    }

    [ClientRpc]
    public void DeselectPieceClientRpc(ulong clientId, ClientRpcParams clientRpcParams)
    {
        Debug.Log("DeselectPieceClientRpc " + clientId);
        if (GetSelectedPiece(clientId) != null)
        {
            Debug.Log(GetSelectedPiece(clientId));
            GetSelectedPiece(clientId).Deselect();
            selectedPieces[clientId] = null;
        }
    }

    [ServerRpc]
    public void TryToMoveSelectedPieceServerRpc(Vector2Int tileIndex, ulong clientId)
    {
        Debug.Log("TryToMoveSelectedPieceServerRpc");
        Tile tile = boardManager.GetTileAt(tileIndex);
        Piece selectedPiece = GetSelectedPiece(clientId);

        if (CanMove(selectedPiece, tile))
        {
            Debug.Log("CanMove " + selectedPiece + " " + tile.IndexInMatrix);
            if (tile.IsOccupied)
            {
                ResolveBattle(selectedPiece, selectedPiece.GetTile(), tile, clientId);
            }
            else
            {
                MovePieceClientRpc(selectedPiece.GetTile().IndexInMatrix, tileIndex);
                DeselectPieceServerRpc(clientId);
            }
        }
        else if (tile.IsOccupied && tile.GetPiece().PlayerId == clientId)
        {
            DeselectPieceServerRpc(clientId);
            SelectPieceServerRpc(tileIndex, clientId);
        }
    }

    [ClientRpc]
    public void MovePieceClientRpc(Vector2Int pieceLocation, Vector2Int targetTileTndex)
    {
        Debug.Log("MovePieceClientRpc " + NetworkManager.Singleton.LocalClientId);
        Piece piece = boardManager.GetTileAt(pieceLocation).GetPiece();
        piece.MoveToTile(boardManager.GetTileAt(targetTileTndex));
    }

    private bool CanMove(Piece piece, Tile tile)
    {
        List<Tile> possibleMoves = piece.GetPossibleMoves(boardManager.GetAllTiles());
        return possibleMoves.Contains(tile);
    }

    public void ResolveBattle(Piece attacker, Tile attackerTile, Tile defenderTile, ulong clientId)
    {
        Piece defender = defenderTile.GetPiece();
        if (defender != null && attacker.PlayerId != defender.PlayerId)
        {
            int attackerRank = attacker.GetRank();
            int defenderRank = defender.GetRank();
            string attackerType = attacker.GetPieceType();
            string defenderType = defender.GetPieceType();

            if (defenderType == "Bomb" && attackerType != "Miner")
            {
                Destroy(attacker.gameObject);
                attackerTile.RemovePiece();
            }
            else if (attackerType == "Spy" && defenderType == "Marshal")
            {
                Destroy(defender.gameObject);
                attacker.MoveToTile(defenderTile);
            }
            else if (attackerRank > defenderRank)
            {
                Destroy(defender.gameObject);
                attacker.MoveToTile(defenderTile);
            }
            else if (attackerRank < defenderRank)
            {
                Destroy(attacker.gameObject);
                attackerTile.RemovePiece();
            }
            else
            {
                Destroy(defender.gameObject);
                Destroy(attacker.gameObject);
                attackerTile.RemovePiece();
                defenderTile.RemovePiece();
            }
        }

        selectedPieces[clientId] = null;
    }

}
