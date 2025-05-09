using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveLoadUtility
{

    public static SaveData SaveData { get { return GetSaveData(); } }

#pragma warning disable UDR0001 // Domain Reload Analyzer
    private static SaveData playerData;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    private static SaveData GetSaveData() {
        if (playerData == null) {
            playerData = LoadSaveData();
        }

        return playerData;
    }

    public static SaveData LoadSaveData() {
        string filePath = Application.persistentDataPath + "/SaveData.json";

        if (File.Exists(filePath) == true) {
            string loadedJson = File.ReadAllText(filePath);
            SaveData loadedData = SaveData.FromJSON(loadedJson);
            return loadedData;
        }

        return new SaveData();
    }

    public static void SavePlayerData() {
        string filePath = Application.persistentDataPath + "/SaveData.json";
        string jsonString = SaveData.ToJSON();
        File.WriteAllText(filePath, jsonString);
    }

    public static void ResetSaveData() {
        string filePath = Application.persistentDataPath + "/SaveData.json";

        if (File.Exists(filePath) == true) {
            File.Delete(filePath);
        }
    }
}
