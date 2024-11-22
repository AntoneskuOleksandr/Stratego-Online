using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private Dictionary<ulong, Piece> selectedPieces = new Dictionary<ulong, Piece>();
    private BoardManager boardManager;
    private ClientRpcParams clientRpcParams = new ClientRpcParams { };
    private bool isHostTurn = true;

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
        if ((IsHostPlayer(clientId) && !isHostTurn) || (!IsHostPlayer(clientId) && isHostTurn))
        {
            return;
        }

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
            if (GetSelectedPiece(clientId) == null)
            {
                SelectPieceServerRpc(tileIndex, clientId);
            }
            else if (GetSelectedPiece(clientId) == occupyingPiece)
            {
                DeselectPieceServerRpc(clientId);
            }
            else if (occupyingPiece.PlayerId == clientId)
            {
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
        SelectPieceClientRpc(pieceLocation, clientId, clientRpcParams);
        selectedPieces[clientId] = boardManager.GetTileAt(pieceLocation).GetPiece();
    }

    [ClientRpc]
    public void SelectPieceClientRpc(Vector2Int pieceLocation, ulong clientId, ClientRpcParams clientRpcParams)
    {
        Piece piece = boardManager.GetTileAt(pieceLocation).GetPiece();
        selectedPieces[clientId] = piece;
        piece.Select();
    }

    [ServerRpc]
    public void DeselectPieceServerRpc(ulong clientId)
    {
        DeselectPieceClientRpc(clientId, clientRpcParams);
        selectedPieces[clientId] = null;
    }

    [ClientRpc]
    public void DeselectPieceClientRpc(ulong clientId, ClientRpcParams clientRpcParams)
    {
        if (GetSelectedPiece(clientId) != null)
        {
            GetSelectedPiece(clientId).Deselect();
            selectedPieces[clientId] = null;
        }
    }

    [ServerRpc]
    public void TryToMoveSelectedPieceServerRpc(Vector2Int tileIndex, ulong clientId)
    {
        Tile tile = boardManager.GetTileAt(tileIndex);
        Piece selectedPiece = GetSelectedPiece(clientId);

        if (CanMove(selectedPiece, tile))
        {
            if (tile.IsOccupied)
            {
                ResolveBattleClientRpc(selectedPiece.GetTile().IndexInMatrix, tileIndex, clientId);

            }
            else
            {
                MovePieceClientRpc(selectedPiece.GetTile().IndexInMatrix, tileIndex);
                DeselectPieceServerRpc(clientId);
                SwitchTurn();
            }
        }
        else if (tile.IsOccupied && tile.GetPiece().PlayerId == clientId)
        {
            DeselectPieceServerRpc(clientId);
            SelectPieceServerRpc(tileIndex, clientId);
        }
    }

    [ClientRpc]
    public void MovePieceClientRpc(Vector2Int pieceLocation, Vector2Int targetTileIndex)
    {
        Tile prevPieceTile = boardManager.GetTileAt(pieceLocation);
        Tile newPieceTile = boardManager.GetTileAt(targetTileIndex);
        Piece piece = prevPieceTile.GetPiece();
        piece.MoveToTile(newPieceTile);
        piece.ChangeTile(newPieceTile);
    }

    private bool CanMove(Piece piece, Tile tile)
    {
        List<Tile> possibleMoves = piece.GetPossibleMoves(boardManager.GetAllTiles());
        return possibleMoves.Contains(tile);
    }

    [ClientRpc]
    public void ResolveBattleClientRpc(Vector2Int attackerIndex, Vector2Int defenderIndex, ulong clientId)
    {
        Tile attackerTile = boardManager.GetTileAt(attackerIndex);
        Tile defenderTile = boardManager.GetTileAt(defenderIndex);

        Piece attacker = attackerTile.GetPiece();
        Piece defender = defenderTile.GetPiece();

        if (defender != null && attacker.PlayerId != defender.PlayerId)
        {
            RevealPieceClientRpc(attackerIndex);
            RevealPieceClientRpc(defenderIndex);

            attacker.MoveToTile(defenderTile);

            StartCoroutine(DelayBattle(attackerTile, defenderTile, clientId));
        }
    }

    private IEnumerator DelayBattle(Tile attackerTile, Tile defenderTile, ulong attackerClientId)
    {
        yield return new WaitForSeconds(2.0f);

        Piece attacker = attackerTile.GetPiece();
        Piece defender = defenderTile.GetPiece();

        if (attacker == null || defender == null)
            Debug.LogError("Attacker or defender is null");

        var attackerRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { attackerClientId }
            }
        };

        var defenderRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { defender.PlayerId }
            }
        };

        if (attacker != null && defender != null)
        {
            int attackerRank = attacker.GetRank();
            int defenderRank = defender.GetRank();
            string attackerType = attacker.GetPieceType();
            string defenderType = defender.GetPieceType();

            if (defenderType == "Bomb" && attackerType != "Miner")
            {
                Destroy(attacker.gameObject);
                attackerTile.RemovePiece();
                HidePieceClientRpc(defenderTile.IndexInMatrix, attackerRpcParams);
            }
            else if (attackerType == "Spy" && defenderType == "Marshal")
            {
                Destroy(defender.gameObject);
                attacker.ChangeTile(defenderTile);
                HidePieceClientRpc(attackerTile.IndexInMatrix, defenderRpcParams);
            }
            else if (attackerRank > defenderRank)
            {
                Destroy(defender.gameObject);
                attacker.ChangeTile(defenderTile);
                HidePieceClientRpc(attackerTile.IndexInMatrix, defenderRpcParams);
            }
            else if (attackerRank < defenderRank)
            {
                Destroy(attacker.gameObject);
                attackerTile.RemovePiece();
                HidePieceClientRpc(defenderTile.IndexInMatrix, attackerRpcParams);
            }
            else
            {
                Destroy(defender.gameObject);
                Destroy(attacker.gameObject);
                attackerTile.RemovePiece();
                defenderTile.RemovePiece();
            }
        }

        DeselectPieceClientRpc(attackerClientId, attackerRpcParams);
        SwitchTurn();
    }

    [ClientRpc]
    public void RevealPieceClientRpc(Vector2Int pieceLocation)
    {
        Tile tile = boardManager.GetTileAt(pieceLocation);
        Piece piece = tile.GetPiece();

        if (piece != null)
        {
            piece.SetRevealedState();
        }
    }

    [ClientRpc]
    public void HidePieceClientRpc(Vector2Int pieceLocation, ClientRpcParams clientRpcParams)
    {
        Tile tile = boardManager.GetTileAt(pieceLocation);
        Piece piece = tile.GetPiece();

        if (piece != null)
        {
            piece.SetHiddenState();
        }
    }


    private void SwitchTurn()
    {
        isHostTurn = !isHostTurn;
    }

    private bool IsHostPlayer(ulong clientId)
    {
        return NetworkManager.Singleton.LocalClientId == clientId;
    }
}
