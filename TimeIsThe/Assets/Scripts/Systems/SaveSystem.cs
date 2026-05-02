using System.IO;
using UnityEngine;

/// <summary>
/// Simple JSON-based save system. Extend SaveData with any fields you need to persist.
/// </summary>
public static class SaveSystem
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveSystem] Saved to {SavePath}");
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveSystem] No save file found, returning default data.");
            return new SaveData();
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool HasSave() => File.Exists(SavePath);

    public static void Delete()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}

[System.Serializable]
public class SaveData
{
    public int level = 1;
    public float playerHealth = 100f;
    // Add additional fields here as the game grows
}
