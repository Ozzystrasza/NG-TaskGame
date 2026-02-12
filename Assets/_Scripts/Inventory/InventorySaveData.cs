using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class InventorySlotSaveData
{
    public int slotIndex;
    public string itemId;
}

[Serializable]
public class InventorySaveData
{
    public List<InventorySlotSaveData> consumableSlots = new List<InventorySlotSaveData>();
    public List<InventorySlotSaveData> equipmentSlots = new List<InventorySlotSaveData>();
    public string equippedWeaponId;
    public string equippedArmorId;
}

public static class InventorySaveSystem
{
    private const string FileName = "inventory.json";

    public static string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }

    public static void SaveToFile(InventorySaveData data)
    {
        if (data == null) return;

        try
        {
            string path = GetSaveFilePath();
            string json = JsonUtility.ToJson(data, false);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save inventory: " + ex.Message);
        }
    }

    public static InventorySaveData LoadFromFile()
    {
        string path = GetSaveFilePath();
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<InventorySaveData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to load inventory: " + ex.Message);
            return null;
        }
    }
}
