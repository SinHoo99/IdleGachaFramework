using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : PoolObject
{
    // Switch to TextMeshPro (World Space version)
    [SerializeField] private TextMeshPro _damageText;

    private void Awake()
    {
        if (_damageText == null) _damageText = GetComponent<TextMeshPro>();
        
        // Remove CanvasRenderer if it exists to avoid TMP warnings in World Space
        if (TryGetComponent<CanvasRenderer>(out var canvasRenderer))
        {
            Destroy(canvasRenderer);
        }
    }

    public override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        base.OnSpawn(position, rotation);
        // Setup will be called externally to pass the damage value
    }

    public void Setup(float damage)
    {
        if (_damageText == null) _damageText = GetComponent<TextMeshPro>();
        if (_damageText == null) return;

        _damageText.text = Mathf.FloorToInt(damage).ToString();

        // Reset state
        Color color = _damageText.color;
        color.a = 1f;
        _damageText.color = color;
        transform.localScale = Vector3.one;

        // Animation
        Sequence sequence = DOTween.Sequence();

        // Calculate a small random X and Y offset for a "floating" effect
        float randomX = Random.Range(-0.5f, 0.5f);
        float randomY = Random.Range(1.5f, 2.5f);
        Vector3 targetPosition = new Vector3(transform.position.x + randomX, transform.position.y + randomY, transform.position.z);

        // Move up and slightly sideways
        sequence.Join(transform.DOMove(targetPosition, 0.7f).SetEase(Ease.OutQuad));

        // Scale effect
        sequence.Join(transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutQuad));
        sequence.Append(transform.DOScale(1f, 0.15f).SetEase(Ease.InQuad));

        // Fade out
        sequence.Append(_damageText.DOFade(0f, 0.4f).SetDelay(0.2f));

        // Return to pool
        sequence.OnComplete(() => {
            ReturnToPool();
        });
    }

    private void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnObject(Tag.DamageText, this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
