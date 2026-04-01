using UnityEngine;

/// <summary>
/// Base class for all enemy/boss attack behaviors.
/// Created as a ScriptableObject to allow reusable patterns across different enemies.
/// </summary>
public abstract class EnemyAttackPattern : ScriptableObject
{
    [Header("Pattern Base Settings")]
    public float Cooldown = 2f;
    public string AnimationTrigger = "Attack"; // Optional animation trigger to play

    /// <summary>
    /// Executes the attack logic.
    /// </summary>
    /// <param name="owner">The Enemy instance executing this pattern.</param>
    public abstract void Execute(Enemy owner);
}
