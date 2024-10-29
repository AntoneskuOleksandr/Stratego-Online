using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PieceButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button button;
    public Button Button
    {
        get
        {
            return button;
        }
        private set
        {

        }
    }

    public void SetPieceData(PieceData pieceData, int pieceCount)
    {
        icon.sprite = pieceData.Icon;
        nameText.text = pieceData.Name;
        rankText.text = pieceData.Rank.ToString();
        countText.text = pieceCount.ToString();
    }

    public void UpdatePieceCount(int count)
    {
        countText.text = $"{nameText.text}: {count}";
    }
}
