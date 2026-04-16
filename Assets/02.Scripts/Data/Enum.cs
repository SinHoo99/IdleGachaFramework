public enum EntityType
{
    Enemy,
    Boss
}

public enum UnitIType
{
    Object,
}

public enum BGM
{
    BGM
}
public enum SFX
{
    Shoot,
    Click,
    TakeDamage,
    Upgrade
}

public enum GameEventType
{
    OnDataReset,
    OnInventoryUpdate,
    OnDictionaryUpdate,
    OnBossDefeated,
    OnGameStart,
    OnStageStart,
    OnStageClear,
    OnStageFail,
    OnEnemyDefeated,
    OnRewardEarned // 추가
}

public enum PlayerState
{
    Idle,
    Attack,
    Parrying,
    Hit,
    Die
}

public enum GameState
{
    Ready,
    Playing,
    Win,
    Lose,
    Paused
}

public enum CardEffectType
{
    Damage,
    MaxHP,
    AttackRange,
    AttackSpeed,
    MultiShot
}