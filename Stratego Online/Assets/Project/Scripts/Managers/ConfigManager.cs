using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConfigManager", menuName = "Managers/ConfigManager")]
public class ConfigManager : ScriptableObject
{
    public int BoardRows;
    public int BoardColumns;
    public float TileSize;
    public List<PieceData> PiecesData;

    public Material TileMaterialLake;
    public Material TileMaterialWhite;
    public Material TileMaterialBlack;
    public Color TileColorHighlighted;
    public Mesh hiddenPieceMesh;

    public PieceData GetPieceDataByName(string name)
    {
        PieceData pieceData = PiecesData.Find(piece => piece.Name == name);

        if (pieceData == null)
            Debug.LogError("There is no PieceData with name: " + name);

        return pieceData;
    }
}
