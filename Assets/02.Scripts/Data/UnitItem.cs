using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitItem : MonoBehaviour
{
    public Button UnitButton;
    public TextMeshProUGUI UnitText;
    private string unitID;

    /// <summary>
    /// Updates the fruit item UI and stores the fruitID.
    /// </summary>
    public void UpdateUnit(string id, int count, Sprite icon)
    {
        unitID = id;

        // Update button image
        if (UnitButton != null && UnitButton.TryGetComponent<Image>(out var buttonImage))
        {
            buttonImage.sprite = icon;
        }

        // Update unit text info with Level instead of Count
        var data = DataManager.Instance?.GetUnitData(id);
        if (data != null)
        {
            UnitText.text = $"{data.Name} Lv.{count} \n Price : {data.Price}";
        }
    }

    /// <summary>
    /// Event triggered when the fruit button is clicked.
    /// </summary>
    public void OnUnitButtonClicked()
    {
        if (ObjectPool.Instance == null || PlayerDataManager.Instance == null) return;

        if (PlayerDataManager.Instance.TrySellUnit(unitID, 1))
        {
            Debug.Log($"{unitID} sold.");
            
            // Check if amount became 0, if so remove from field
            if (PlayerDataManager.Instance.NowPlayerData.Inventory.TryGetValue(unitID, out var collectedData) && collectedData.Amount <= 0)
            {
                if (SpawnManager.Instance != null)
                {
                    SpawnManager.Instance.RemoveUnitFromField(unitID);
                    Debug.Log($"{unitID} amount is 0, removed from field.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"{unitID} could not be sold.");
        }
    }
}
