using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class DataManager : Singleton<DataManager>
{
    private readonly Dictionary<string, UnitData> _unitDatas = new();
    private readonly Dictionary<int, EnemyData> _enemyDatas = new();
    private readonly List<CardData> _allCardDatas = new();

    public IReadOnlyDictionary<string, UnitData> UnitDatas => _unitDatas;
    public IReadOnlyDictionary<int, EnemyData> EnemyDatas => _enemyDatas;
    public IReadOnlyList<CardData> AllCardDatas => _allCardDatas;

    public void Initialize()
    {
        LoadUnitData();
        LoadEnemyData();
        LoadCardData(); // 추가
    }

    private void LoadCardData()
    {
        _allCardDatas.Clear();
        var cards = Resources.LoadAll<CardData>("Cards");
        _allCardDatas.AddRange(cards);
        Debug.Log($"[DataManager] Loaded {_allCardDatas.Count} CardDatas.");
    }

    public UnitData GetUnitData(string id) => _unitDatas.TryGetValue(id, out var data) ? data : null;
    public EnemyData GetEnemyData(int stage) => _enemyDatas.TryGetValue(stage, out var data) ? data : null;

    private void LoadUnitData()
    {
        _unitDatas.Clear();
        var csv = CSVReader.Read(ResourcesPath.UnitCSV);
        if (csv == null) return;

        var atlas = Resources.Load<SpriteAtlas>(ResourcesPath.CSVSprites);

        foreach (var row in csv)
        {
            var data = UnitData.CreateFromCSV(row, atlas);
            if (!_unitDatas.ContainsKey(data.ID)) _unitDatas.Add(data.ID, data);
        }
        Debug.Log($"[DataManager] Loaded {_unitDatas.Count} UnitDatas.");
    }

    private void LoadEnemyData()
    {
        _enemyDatas.Clear();
        var csv = CSVReader.Read(ResourcesPath.EnemyCSV);
        if (csv == null) return;

        foreach (var row in csv)
        {
            var data = EnemyData.CreateFromCSV(row);
            if (!_enemyDatas.ContainsKey(data.Stage)) _enemyDatas.Add(data.Stage, data);
        }
        Debug.Log($"[DataManager] Loaded {_enemyDatas.Count} EnemyDatas.");
    }
}
