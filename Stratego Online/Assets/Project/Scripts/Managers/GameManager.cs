using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private Piece selectedPiece;
    private BoardManager boardManager;

    public void Initialize(BoardManager boardManager, UIManager uiManager, PiecePlacementManager piecePlacementManager)
    {
        this.boardManager = boardManager;
    }

    public void StartGame()
    {
        Debug.Log("Game has started!");
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleTileActionServerRpc(Vector2Int tileIndex, ulong clientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        Debug.Log("HandleTileAction " + clientId);

        Tile tile = boardManager.GetTileAt(tileIndex);
        Piece occupyingPiece = tile.GetPiece();
        if (tile.IsOccupied && occupyingPiece.PlayerId == clientId)
        {
            if (GetSelectedPiece() == null)
            {
                SelectPieceClientRpc(tile.IndexInMatrix, clientRpcParams);
            }
            else if (GetSelectedPiece() == occupyingPiece)
            {
                DeselectPieceClientRpc(clientRpcParams);
            }
            else
            {
                TryToMoveSelectedPieceServerRpc(tileIndex);
            }
        }
        else
        {
            TryToMoveSelectedPieceServerRpc(tileIndex);
        }
    }

    [ServerRpc]
    public void TryToMoveSelectedPieceServerRpc(Vector2Int tileIndex)
    {
        Tile tile = boardManager.GetTileAt(tileIndex);
        Debug.Log("TryToMoveSelectedPieceTo " + tile);
        if (CanMove(tile))
        {
            if (tile.IsOccupied)
            {
                ResolveBattle(selectedPiece, selectedPiece.GetTile(), tile);
            }
            else
            {
                selectedPiece.MoveToTile(tile);
            }
        }
    }

    public Piece GetSelectedPiece()
    {
        return selectedPiece;
    }

    [ClientRpc]
    public void SelectPieceClientRpc(Vector2Int pieceLocation, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("SelectPieceClientRpc " + NetworkManager.Singleton.LocalClientId);
        if (selectedPiece != null)
        {
            DeselectPieceClientRpc();
        }
        selectedPiece = boardManager.GetTileAt(pieceLocation).GetPiece();
        selectedPiece.Select();
    }

    [ClientRpc]
    public void DeselectPieceClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (selectedPiece != null)
        {
            selectedPiece.Deselect();
            selectedPiece = null;
        }
    }

    public void ResolveBattle(Piece attacker, Tile attackerTile, Tile defenderTile)
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

        selectedPiece = null;
    }

    private bool CanMove(Tile tile)
    {
        if (selectedPiece != null)
        {
            List<Tile> possibleMoves = selectedPiece.GetPossibleMoves(boardManager.GetAllTiles());

            if (possibleMoves.Contains(tile))
            {
                return true;
            }
        }
        return false;
    }
}
