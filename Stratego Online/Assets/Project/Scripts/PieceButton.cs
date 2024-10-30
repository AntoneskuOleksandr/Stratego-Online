using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PieceButton : MonoBehaviour
{
    public Image pieceIcon;
    public TMP_Text pieceNameText;
    public TMP_Text pieceRankText;
    public TMP_Text pieceCountText;
    public Button Button;

    public void SetPieceData(PieceData pieceData, int pieceCount)
    {
        pieceIcon.sprite = pieceData.Icon;
        pieceNameText.text = pieceData.Name;
        pieceRankText.text = pieceData.Rank.ToString();
        pieceCountText.text = pieceCount.ToString();
    }

    public void UpdatePieceCount(int count)
    {
        pieceCountText.text = count.ToString();
    }
}
