using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class DataManager : MonoBehaviour
{
    public readonly Dictionary<FruitsID, FruitsData> FruitDatas = new();
    public readonly Dictionary<BossID, BossData> BossDatas = new();

    public void Initialize()
    {
        LoadFruitsData();
        LoadBossData();
    }

    #region Fruit Data Loading
    private void LoadFruitsData()
    {
        FruitDatas.Clear();
        var fruitsCSV = CSVReader.Read(ResourcesPath.FruitsCSV);
        if (fruitsCSV == null) return;

        // Load SpriteAtlas once to improve performance
        var atlas = Resources.Load<SpriteAtlas>(ResourcesPath.CSVSprites);

        foreach (var row in fruitsCSV)
        {
            var fruitsData = new FruitsData
            {
                ID = (FruitsID)ParseInt(row[Data.ID]),
                Name = row[Data.Name],
                Price = ParseInt(row[Data.Price]),
                Type = (FruitsType)ParseInt(row[Data.Type]),
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

            if (!FruitDatas.ContainsKey(fruitsData.ID))
            {
                FruitDatas.Add(fruitsData.ID, fruitsData);
            }
        }
    }
    #endregion

    #region Boss Data Loading
    private void LoadBossData()
    {
        BossDatas.Clear();
        var bossCSV = CSVReader.Read(ResourcesPath.BossCSV);
        if (bossCSV == null) return;

        foreach (var row in bossCSV)
        {
            var bossID = (BossID)ParseInt(row[Data.ID]);
            var maxHealth = ParseInt(row[Data.MaxHealth]);
            var animationState = row[Data.AnimationState];
            var reward = ParseInt(row[Data.Reward]);

            var bossData = new BossData(bossID, maxHealth, animationState, reward);

            if (!BossDatas.ContainsKey(bossData.ID))
            {
                BossDatas.Add(bossData.ID, bossData);
            }
        }
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
