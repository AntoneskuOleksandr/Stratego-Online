using UnityEngine;

[CreateAssetMenu(fileName = "New Piece", menuName = "Stratego/Piece")]
public class PieceData : ScriptableObject
{
    public string Name;
    public int Rank;
    public int Count;
    public Sprite Icon;
}