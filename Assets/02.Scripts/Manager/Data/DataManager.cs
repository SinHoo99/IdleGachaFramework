using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class DataManager : Singleton<DataManager>
{
    private readonly Dictionary<string, UnitData> _unitDatas = new();
    private readonly Dictionary<BossID, BossData> _bossDatas = new();

    public IReadOnlyDictionary<string, UnitData> UnitDatas => _unitDatas;
    public IReadOnlyDictionary<BossID, BossData> BossDatas => _bossDatas;

    public void Initialize()
    {
        LoadUnitData();
        LoadBossData();
    }

    public UnitData GetUnitData(string id)
    {
        return _unitDatas.TryGetValue(id, out var data) ? data : null;
    }

    public BossData GetBossData(BossID id)
    {
        return _bossDatas.TryGetValue(id, out var data) ? data : null;
    }

    #region Unit Data Loading
    private void LoadUnitData()
    {
        _unitDatas.Clear();
        var fruitsCSV = CSVReader.Read(ResourcesPath.UnitCSV);
        if (fruitsCSV == null || fruitsCSV.Count == 0)
        {
            Debug.LogError($"[DataManager] Failed to load UnitData CSV from {ResourcesPath.UnitCSV} or it is empty.");
            return;
        }

        // Load SpriteAtlas once to improve performance
        var atlas = Resources.Load<SpriteAtlas>(ResourcesPath.CSVSprites);
        if (atlas == null)
        {
            Debug.LogWarning($"[DataManager] SpriteAtlas not found at: {ResourcesPath.CSVSprites}");
        }

        foreach (var row in fruitsCSV)
        {
            var fruitsData = new UnitData
            {
                ID = row[Data.ID], // Directly use string ID from CSV
                Name = row[Data.Name],
                Price = ParseInt(row[Data.Price]),
                Type = (UnitIType)ParseInt(row[Data.Type]),
                Description = row[Data.Description],
                Probability = ParseFloat(row[Data.Probability]),
                Damage = ParseFloat(row[Data.Damage]),
                AttackSpeed = ParseFloat(row[Data.AttackSpeed])
            };

            // Load Sprite from Atlas
            if (atlas != null)
            {
                fruitsData.Image = atlas.GetSprite(row[Data.Image]);
            }

            // Load Prefab
            fruitsData.Prefab = Resources.Load<PoolObject>(row[Data.Prefab]);

            if (fruitsData.Prefab == null)
            {
                Debug.LogWarning($"[DataManager] Prefab not found for Unit {fruitsData.ID} at path: {row[Data.Prefab]}");
            }

            if (!_unitDatas.ContainsKey(fruitsData.ID))
            {
                _unitDatas.Add(fruitsData.ID, fruitsData);
            }
        }
        Debug.Log($"[DataManager] Successfully loaded {_unitDatas.Count} UnitDatas.");
    }
    #endregion

    #region Boss Data Loading
    private void LoadBossData()
    {
        _bossDatas.Clear();
        var bossCSV = CSVReader.Read(ResourcesPath.BossCSV);
        if (bossCSV == null || bossCSV.Count == 0)
        {
            Debug.LogError($"[DataManager] Failed to load BossData CSV from {ResourcesPath.BossCSV} or it is empty.");
            return;
        }

        foreach (var row in bossCSV)
        {
            var bossID = (BossID)ParseInt(row[Data.ID]);
            var maxHealth = ParseInt(row[Data.MaxHealth]);
            var animationState = row[Data.AnimationState];
            var reward = ParseInt(row[Data.Reward]);

            var bossData = new BossData(bossID, maxHealth, animationState, reward);

            if (!_bossDatas.ContainsKey(bossData.ID))
            {
                _bossDatas.Add(bossData.ID, bossData);
            }
        }
        Debug.Log($"[DataManager] Successfully loaded {_bossDatas.Count} BossDatas.");
    }
    #endregion

    #region Helper Methods
    private int ParseInt(string value)
    {
        return int.TryParse(value, out int result) ? result : 0;
    }

    private float ParseFloat(string value)
    {
        return float.TryParse(value, out float result) ? result : 0f;
    }
    #endregion
}
