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
    /// 데이터를 형식 T의 이름을 딴 JSON 파일로 저장합니다.
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
    /// JSON 파일에서 데이터를 로드하려고 시도합니다.
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
        string typeName = typeof(T).IsGenericType 
            ? $"{typeof(T).Name}_{typeof(T).GetGenericArguments()[0].Name}" 
            : typeof(T).Name;
            
        string directory = Application.persistentDataPath;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        return Path.Combine(directory, $"{typeName}.json");
    }
}
