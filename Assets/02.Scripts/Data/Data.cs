using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

#region Unit Data
[Serializable]
public class UnitData
{
    public string ID;               // 유닛 ID (문자열 기반)
    public string Name;               // 유닛 이름
    public UnitIType Type;           // 유닛 타입
    public Sprite Image;              // 유닛 스프라이트
    public string Description;        // 유닛 설명
    public int Price;                 // 판매 가격
    public float Probability;         // 생성 확률
    public float Damage;              // 데미지 값
    public PoolObject Prefab;         // 프리팹 참조
    public float AttackSpeed;         // 공격 속도 값

    public static UnitData CreateFromCSV(Dictionary<string, string> row, SpriteAtlas atlas)
    {
        var data = new UnitData
        {
            ID = row[Data.ID],
            Name = row[Data.Name],
            Price = ParseInt(row[Data.Price]),
            Type = (UnitIType)ParseInt(row[Data.Type]),
            Description = row[Data.Description],
            Probability = ParseFloat(row[Data.Probability]),
            Damage = ParseFloat(row[Data.Damage]),
            AttackSpeed = ParseFloat(row[Data.AttackSpeed])
        };

        if (atlas != null) data.Image = atlas.GetSprite(row[Data.Image]);
        data.Prefab = Resources.Load<PoolObject>(row[Data.Prefab]);

        return data;
    }

    private static int ParseInt(string value) => int.TryParse(value, out int result) ? result : 0;
    private static float ParseFloat(string value) => float.TryParse(value, out float result) ? result : 0f;
}
#endregion

#region Enemy Data
[Serializable]
public class EnemyData
{
    public int Stage;
    public EntityType Type;
    public string Name;
    public int MaxHealth;
    public int Count;
    public bool CanMove;
    public int Exp;
    public PoolObject Prefab; 

    public EnemyData(int stage, EntityType type, string name, int maxHealth, int count, bool canMove, int exp = 10)
    {
        Stage = stage;
        Type = type;
        Name = name;
        MaxHealth = maxHealth;
        Count = count;
        CanMove = canMove;
        Exp = exp;
    }

    public static EnemyData CreateFromCSV(Dictionary<string, string> row)
    {
        int stage = ParseInt(row[Data.Stage]);
        string typeStr = row[Data.Type];
        EntityType type = (typeStr == "Boss") ? EntityType.Boss : EntityType.Enemy;
        string name = row[Data.Name];
        int maxHealth = ParseInt(row[Data.MaxHealth]);
        int count = ParseInt(row[Data.Count]);
        bool canMove = ParseBool(row[Data.CanMove]);
        
        int exp = 10;
        if (row.TryGetValue(Data.Exp, out string expStr)) exp = ParseInt(expStr);

        var data = new EnemyData(stage, type, name, maxHealth, count, canMove, exp);
        data.Prefab = Resources.Load<PoolObject>($"Prefabs/Enemy/{name}");

        return data;
    }

    private static int ParseInt(string value) => int.TryParse(value, out int result) ? result : 0;
    private static bool ParseBool(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        string lower = value.ToLower();
        return lower == "true" || lower == "1" || lower == "yes";
    }
}
#endregion

#region Boss Data
[Serializable]
public class BossData
{
    public string Name;
    public int MaxHealth;
    public string AnimationState;
    public int Reward;

    public BossData(string name, int maxHealth, string animationState, int reward)
    {
        Name = name;
        MaxHealth = maxHealth;
        AnimationState = animationState;
        Reward = reward;
    }
}

[Serializable]
public class BossRuntimeData
{
    public string CurrentBossName;
    public float CurrentHealth;

    public BossRuntimeData(string name, float currentHealth)
    {
        CurrentBossName = name;
        CurrentHealth = currentHealth;
    }
}
#endregion

#region Serialization Helpers
[Serializable]
public class PrefabData
{
    public string prefabName;
    public SerializableVector3 position;
    public SerializableQuaternion rotation;

    public PrefabData(string prefabName, Vector3 position, Quaternion rotation)
    {
        this.prefabName = prefabName;
        this.position = new SerializableVector3(position);
        this.rotation = new SerializableQuaternion(rotation);
    }
}

[Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class SerializableQuaternion
{
    public float x, y, z, w;

    public SerializableQuaternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }

    public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
}
#endregion

#region Player Data
[Serializable]
public class PlayerData
{
    public Dictionary<string, CollectedUnitData> Inventory = new();
    public Dictionary<string, bool> DictionaryCollection = new();
    public DateTime LastCollectedTime;
    public int PlayerCoin = 1000;

    public int Level = 1;
    public float CurrentExp = 0;
    public float MaxExp = 100;

    public float Damage = 50f;
    public float AttackRange = 5f;
    public float MaxHP = 100f;
    public float AttackSpeed = 1.0f; // 기본 공격 속도 1.0

    private int _multiShotCount = 1;
    public int MultiShotCount 
    {
        get => _multiShotCount;
        set => _multiShotCount = Mathf.Clamp(value, 1, 3);
    }
}

[Serializable]
public class CollectedUnitData
{
    public string ID;     // 유닛 ID (문자열 기반)
    public int Amount;      // 수집된 수량
}
#endregion

#region Option Data
[Serializable]
public class OptionData
{
    public float BGMVolume;
    public float SFXVolume;
}
#endregion
