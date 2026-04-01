using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dedicated UI component for the AutoPlay Toggle.
/// Requires an AutoPlayController on the same GameObject or assigned via Inspector.
/// </summary>
[RequireComponent(typeof(AutoPlayController))]
public class UI_AutoPlayToggle : MonoBehaviour
{
    [SerializeField] private Toggle _autoPlayToggle;
    [SerializeField] private GameObject _autoEffect; // Visual effect when auto-play is ON
    private AutoPlayController _controller;

    private void Awake()
    {
        _controller = GetComponent<AutoPlayController>();
    }

    private void Start()
    {
        if (_autoPlayToggle != null && _controller != null)
        {
            // Initial sync with controller
            _autoPlayToggle.isOn = _controller.IsAutoPlay;
            UpdateEffectState(_controller.IsAutoPlay);

            // Add listener for user interaction
            _autoPlayToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool value)
    {
        if (_controller != null)
        {
            _controller.IsAutoPlay = value;
            UpdateEffectState(value);
        }
    }

    private void OnEnable()
    {
        // Sync UI and effect state whenever this object is enabled
        if (_autoPlayToggle != null && _controller != null)
        {
            bool currentState = _controller.IsAutoPlay;
            _autoPlayToggle.SetIsOnWithoutNotify(currentState);
            UpdateEffectState(currentState);
        }
    }

    private void UpdateEffectState(bool isActive)
    {
        if (_autoEffect != null)
        {
            _autoEffect.SetActive(isActive);
        }
    }
}

