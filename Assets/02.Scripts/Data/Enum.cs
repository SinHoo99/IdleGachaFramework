public enum BossID
{
    A = 1,
    B,
    C,
    D,
    E
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
    OnEnemyDefeated
}

public enum GameState
{
    Ready,
    Playing,
    Win,
    Lose,
    Paused
}