using UnityEngine;

[CreateAssetMenu(fileName = "ConfigManager", menuName = "Managers/ConfigManager")]
public class ConfigManager : ScriptableObject
{
    public int boardRows;
    public int boardColumns;
    public float tileSize;

    public Material tileMaterialLake;
    public Material tileMaterialWhite;
    public Material tileMaterialBlack;
    public Material tileMaterialHighlighted;

    public GameObject[] piecePrefabs; 
    public int[] pieceCounts; 
}
