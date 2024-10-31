using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour, IGameManager
{
    private Piece selectedPiece;
    private IBoardManager boardManager;
    private UIManager uiManager;
    private ConfigManager configManager;

    public void Initialize(IBoardManager boardManager, UIManager uiManager, ConfigManager configManager)
    {
        this.boardManager = boardManager;
        this.uiManager = uiManager;
        this.configManager = configManager;
    }

    public void StartGame()
    {
        // Ћогика дл€ старта игры
        Debug.Log("Game has started!");
    }

    public Piece GetSelectedPiece()
    {
        return selectedPiece;
    }

    public void SelectPiece(Piece piece)
    {
        if (piece.PlayerId != NetworkManager.LocalClientId)
        {
            Debug.Log("Ёто не ваша фигура.");
            return;
        }

        if (selectedPiece != null)
        {
            selectedPiece.Deselect();
        }
        selectedPiece = piece;
        piece.Select();
    }

    public void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            selectedPiece.Deselect();
            selectedPiece = null;
        }
    }

    public void TryToMoveSelectedPieceTo(Tile tile)
    {
        if (selectedPiece != null && selectedPiece.PlayerId == NetworkManager.LocalClientId)
        {
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
            DeselectPiece();
        }
        else
        {
            Debug.Log("¬ы не можете перемещать чужую фигуру.");
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

    public void SelectPiece(PieceData pieceData)
    {
        throw new System.NotImplementedException();
    }
}
