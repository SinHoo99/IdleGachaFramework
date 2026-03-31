using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class DataManager : Singleton<DataManager>
{
    private readonly Dictionary<string, UnitData> _unitDatas = new();
    private readonly Dictionary<int, EnemyData> _enemyDatas = new();

    public IReadOnlyDictionary<string, UnitData> UnitDatas => _unitDatas;
    public IReadOnlyDictionary<int, EnemyData> EnemyDatas => _enemyDatas;

    public void Initialize()
    {
        LoadUnitData();
        LoadEnemyData();
    }

    public UnitData GetUnitData(string id)
    {
        return _unitDatas.TryGetValue(id, out var data) ? data : null;
    }

    public EnemyData GetEnemyData(int stage)
    {
        return _enemyDatas.TryGetValue(stage, out var data) ? data : null;
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

    #region Enemy Data Loading
    private void LoadEnemyData()
    {
        _enemyDatas.Clear();
        var enemyCSV = CSVReader.Read(ResourcesPath.EnemyCSV);
        if (enemyCSV == null || enemyCSV.Count == 0)
        {
            Debug.LogError($"[DataManager] Failed to load EnemyData CSV from {ResourcesPath.EnemyCSV} or it is empty.");
            return;
        }

        foreach (var row in enemyCSV)
        {
            int stage = ParseInt(row[Data.Stage]);
            string typeStr = row[Data.Type];
            EntityType type = (typeStr == "Boss") ? EntityType.Boss : EntityType.Enemy;
            string name = row[Data.Name];
            int maxHealth = ParseInt(row[Data.MaxHealth]);
            int count = ParseInt(row[Data.Count]);
            bool canMove = ParseBool(row[Data.CanMove]);

            var enemyData = new EnemyData(stage, type, name, maxHealth, count, canMove);

            // Load Prefab for this specific enemy (Path: Resources/Prefabs/Enemy/Name)
            string prefabPath = $"Prefabs/Enemy/{name}";
            enemyData.Prefab = Resources.Load<PoolObject>(prefabPath);

            if (enemyData.Prefab == null)
            {
                Debug.LogWarning($"[DataManager] Prefab not found for Enemy '{name}' at Resources/{prefabPath}");
            }

            if (!_enemyDatas.ContainsKey(stage))
            {
                _enemyDatas.Add(stage, enemyData);
            }
        }
        Debug.Log($"[DataManager] Successfully loaded {_enemyDatas.Count} EnemyDatas.");
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

    private bool ParseBool(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        string lower = value.ToLower();
        return lower == "true" || lower == "1" || lower == "yes";
    }
    #endregion
}
