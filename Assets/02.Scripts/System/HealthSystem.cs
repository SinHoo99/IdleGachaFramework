using System;
using UnityEngine;

/// <summary>
/// Simple health management system for game objects.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    public float MaxHP;
    public float CurHP;

    public event Action OnChangeHP;
    public event Action OnDeath;

    public bool IsDead => CurHP <= 0;

    /// <summary>
    /// Initializes health values.
    /// </summary>
    public void InitHP(float curHP, float maxHP)
    {
        MaxHP = maxHP;
        CurHP = Mathf.Clamp(curHP, 0, maxHP);
    }

    /// <summary>
    /// Reduces current health and triggers events.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        CurHP -= damage;
        if (CurHP <= 0)
        {
            CurHP = 0;
            OnDeath?.Invoke();
        }

        OnChangeHP?.Invoke();
    }
}
