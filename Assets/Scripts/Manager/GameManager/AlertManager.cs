using System.Collections;
using TMPro;
using UnityEngine;

public class AlertManager : MonoBehaviour
{
    [SerializeField] private GameObject alertObject;
    [SerializeField] private TextMeshProUGUI alertText;
    
    private Coroutine _alertCoroutine;

    /// <summary>
    /// Displays an alert message for a short duration.
    /// </summary>
    public void ShowAlert(string msg)
    {
        if (alertText == null || alertObject == null) return;

        alertText.text = msg;
        alertObject.SetActive(true);

        if (_alertCoroutine != null) StopCoroutine(_alertCoroutine);
        _alertCoroutine = StartCoroutine(AlertCo());
    }

    private IEnumerator AlertCo()
    {
        yield return new WaitForSecondsRealtime(2f);
        alertObject.SetActive(false);
    }
}
