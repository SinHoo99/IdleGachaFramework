using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDictionaryItem : MonoBehaviour
{
    public string UnitID { get; private set; } = string.Empty;
    public Image unitImage;
    public TextMeshProUGUI unitStatus;
    public Color uncollectedColor = Color.black;

    private Color _originalColor;

    private void Awake()
    {
        if (unitImage != null)
        {
            _originalColor = unitImage.color;
        }
    }
    public void Setup(string id, Sprite sprite)
    {
        UnitID = id;

        if (unitImage != null)
        {
            unitImage.sprite = sprite;
            unitImage.color = uncollectedColor;
        }

        if (unitStatus != null)
        {
            unitStatus.text = "???";

        }
    }

    public void UpdateUnitUI(bool collected)
    {
        if (unitImage != null)
        {
            unitImage.color = collected ? _originalColor : uncollectedColor;
        }

        var UnitData = DataManager.Instance?.GetUnitData(UnitID);
        if (unitStatus != null)
        {
            unitStatus.text = (collected && UnitData != null) ? $"{UnitData.Name}\n Damage : {UnitData.Damage} " : "???";
        }
    }
}
