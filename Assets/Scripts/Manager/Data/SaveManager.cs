using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveManager : Singleton<SaveManager>
{
    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.Indented
    };

    /// <summary>
    /// Saves data to a JSON file named after the type T.
    /// </summary>
    public void SaveData<T>(T data)
    {
        if (data == null) return;

        string path = GetSavePath<T>();
        try
        {
            string jsonData = JsonConvert.SerializeObject(data, JsonSettings);
            File.WriteAllText(path, jsonData);
            Debug.Log($"[SaveManager] Data saved to: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Failed to save data: {ex.Message}");
        }
    }

    /// <summary>
    /// Tries to load data from a JSON file.
    /// </summary>
    public bool TryLoadData<T>(out T data)
    {
        string path = GetSavePath<T>();
        if (File.Exists(path))
        {
            try
            {
                string jsonData = File.ReadAllText(path);
                data = JsonConvert.DeserializeObject<T>(jsonData, JsonSettings);
                Debug.Log($"[SaveManager] Data loaded from: {path}");
                return data != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to load data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[SaveManager] Save file not found: {path}");
        }

        data = default;
        return false;
    }

    private string GetSavePath<T>()
    {
        return Path.Combine(Application.persistentDataPath, $"{typeof(T).Name}.json");
    }
}
