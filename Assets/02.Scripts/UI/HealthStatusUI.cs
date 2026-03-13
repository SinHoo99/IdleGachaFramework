using UnityEngine;
using UnityEngine.UI;

public class HealthStatusUI : MonoBehaviour
{
    public HealthSystem HealthSystem;

    [Header("HP UI")]
    [SerializeField] private Slider HPBar;

    private void OnEnable()
    {
        if (HealthSystem != null)
        {
            HealthSystem.OnChangeHP += UpdateHPStatus;
            UpdateHPStatus();
        }
    }

    private void OnDisable()
    {
        if (HealthSystem != null)
        {
            HealthSystem.OnChangeHP -= UpdateHPStatus;
        }
    }

    #region HP Update
    public void UpdateHPStatus()
    {
        if (HealthSystem == null || HPBar == null) return;
        
        // Ensure slider scale matches health values
        HPBar.maxValue = HealthSystem.MaxHP;
        HPBar.value = HealthSystem.CurHP;
        
        Debug.Log($"[HealthStatusUI] HP Synchronized: {HealthSystem.CurHP} / {HealthSystem.MaxHP}");
    }
    #endregion

    public void ShowSlider()
    {
        this.gameObject.SetActive(true);
    }

    public void HideSlider() 
    {
        this.gameObject.SetActive(false);
    }
}
