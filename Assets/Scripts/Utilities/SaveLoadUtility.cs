using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveLoadUtility
{

    public static SaveData SaveData { get { return GetSaveData(); } }

    private static SaveData playerData;

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
}