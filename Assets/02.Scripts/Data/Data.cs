using System;
using System.Collections.Generic;
using UnityEngine;

#region Unit Data
[Serializable]
public class UnitData
{
    public string ID;               // Unit ID (string-based)
    public string Name;               // Unit Name
    public UnitIType Type;           // Unit Type
    public Sprite Image;              // Unit Sprite
    public string Description;        // Unit Description
    public int Price;                 // Selling Price
    public float Probability;         // Spawning Probability
    public float Damage;              // Damage value
    public PoolObject Prefab;         // Prefab reference
    public float AttackSpeed;         // Attack speed value
}
#endregion

#region Boss Data
[Serializable]
public class BossData
{
    public BossID ID;
    public int MaxHealth;
    public string AnimationState;
    public int Reward;

    public BossData(BossID id, int maxHealth, string animationState, int reward)
    {
        ID = id;
        MaxHealth = maxHealth;
        AnimationState = animationState;
        Reward = reward;
    }
}

[Serializable]
public class BossRuntimeData
{
    public BossID CurrentBossID;
    public float CurrentHealth;

    public BossRuntimeData(BossID id, float currentHealth)
    {
        CurrentBossID = id;
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
}

[Serializable]
public class CollectedUnitData
{
    public string ID;     // Unit ID (string-based)
    public int Amount;      // Collected quantity
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
