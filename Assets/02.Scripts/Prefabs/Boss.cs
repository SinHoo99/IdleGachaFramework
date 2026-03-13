using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Boss : MonoBehaviour
{
    private GameManager GM => GameManager.Instance;
    private BossRuntimeData RuntimeData => BossDataManager.Instance?.BossRuntimeData;

    [SerializeField] private HealthStatusUI healthStatusUI;
    [SerializeField] private float sfxCooldown = 2f;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private BoxCollider2D _boxCollider;
    private HealthSystem _healthSystem;

    public event Action OnChangeBossHP;
    public static event Action<int> OnBossDefeated;

    private float _lastSFXTime = 0f;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _healthSystem = GetComponent<HealthSystem>();

        if (healthStatusUI != null)
        {
            healthStatusUI.HealthSystem = _healthSystem;
        }
    }

    private void Start()
    {
        InitializeBoss();
    }

    private void OnEnable()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath += OnDie;
            _healthSystem.OnChangeHP += HandleChangeHP;
        }

        Respawn();
    }

    private void OnDisable()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath -= OnDie;
            _healthSystem.OnChangeHP -= HandleChangeHP;
        }
    }

    public void InitializeBoss()
    {
        if (RuntimeData == null) return;

        var bossData = DataManager.Instance.GetBossData(RuntimeData.CurrentBossID);
        if (bossData == null) return;

        // Initialize Health
        if (RuntimeData.CurrentHealth <= 0)
            RuntimeData.CurrentHealth = bossData.MaxHealth;

        if (_healthSystem != null)
        {
            _healthSystem.InitHP(RuntimeData.CurrentHealth, bossData.MaxHealth);
        }

        // Initialize UI
        if (healthStatusUI != null)
        {
            healthStatusUI.UpdateHPStatus();
            healthStatusUI.ShowSlider();
        }

        UpdateBossAnimation();

        if (PlayerStatusUI.Instance != null) PlayerStatusUI.Instance.BossStatus();
    }

    private void HandleChangeHP()
    {
        OnChangeBossHP?.Invoke();
    }

    private void OnDie()
    {
        if (RuntimeData == null) return;

        int reward = DataManager.Instance.GetBossData(RuntimeData.CurrentBossID)?.Reward ?? 0;
        if (PlayerDataManager.Instance?.NowPlayerData != null)
            PlayerDataManager.Instance.NowPlayerData.PlayerCoin += reward;

        OnBossDefeated?.Invoke(reward);

        _spriteRenderer.enabled = false;
        _boxCollider.enabled = false;

        if (healthStatusUI != null) healthStatusUI.HideSlider();

        // Prepare next boss
        RuntimeData.CurrentBossID = GetNextBossID();

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(3f);
        Respawn();
    }

    private void Respawn()
    {
        InitializeBoss();
        _spriteRenderer.enabled = true;
        _boxCollider.enabled = true;
    }

    public void TakeDamage(float damage)
    {
        if (_healthSystem != null && !_healthSystem.IsDead)
        {
            _healthSystem.TakeDamage(damage);
            if (RuntimeData != null) RuntimeData.CurrentHealth = _healthSystem.CurHP;

            //Debug.Log($"[Boss] Took {damage} damage. Current HP: {_healthSystem.CurHP}");

            TakeDamageEffect();
        }
    }

    private BossID GetNextBossID()
    {
        if (RuntimeData == null) return BossID.A;
        return (RuntimeData.CurrentBossID < BossID.E) ? RuntimeData.CurrentBossID + 1 : BossID.A;
    }

    public void UpdateBossAnimation()
    {
        if (_animator != null && RuntimeData != null)
            _animator.SetTrigger(RuntimeData.CurrentBossID.ToString());
    }

    public void ResetBossData()
    {
        if (RuntimeData == null) return;

        var bossData = DataManager.Instance.GetBossData(RuntimeData.CurrentBossID);
        if (bossData != null)
        {
            RuntimeData.CurrentHealth = bossData.MaxHealth;
            InitializeBoss();
        }

        Debug.Log($"[Boss] Data reset for {RuntimeData.CurrentBossID}.");
    }

    #region Visual Effects
    private void TakeDamageEffect()
    {
        ColorEffect();
        ScaleEffect();
        PlayLimitedSFX();
    }

    private void ColorEffect()
    {
        _spriteRenderer.DOKill();
        _spriteRenderer.DOColor(Color.red, 0.1f).OnComplete(() => _spriteRenderer.DOColor(Color.white, 0.1f));
    }

    private void ScaleEffect()
    {
        transform.DOKill();
        transform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.1f).OnComplete(() => transform.DOScale(Vector3.one, 0.1f));
    }

    private void PlayLimitedSFX()
    {
        if (Time.time - _lastSFXTime < sfxCooldown) return;
        _lastSFXTime = Time.time;

        if (GM != null) GM.PlaySFX(SFX.TakeDamage);
    }
    #endregion
}
