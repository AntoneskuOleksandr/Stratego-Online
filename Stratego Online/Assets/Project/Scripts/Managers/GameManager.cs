using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, IGameManager
{
    private Piece selectedPiece;
    private BoardManager boardManager;

    public void Initialize(BoardManager boardManager, UIManager uiManager, PiecePlacementManager piecePlacementManager)
    {
        this.boardManager = boardManager;
    }

    public void StartGame()
    {
        // Логика для старта игры
        Debug.Log("Game has started!");
    }

    public Piece GetSelectedPiece()
    {
        return selectedPiece;
    }

    public void SelectPiece(Piece piece)
    {
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
        if (CanMove(tile))
        {
            if (tile.IsOccupied.Value)
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
