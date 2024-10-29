using UnityEngine;

[CreateAssetMenu(fileName = "New Piece", menuName = "Stratego/Piece")]
public class PieceData : ScriptableObject
{
    public string pieceName;
    public int rank;
    public Sprite icon;
}
