using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static SaveManager;
using System.Collections.Generic;
using System.Runtime.Serialization;

public static class SaveSystem
{
    public static void NewSave(SaveData Data, string ProfileName)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        string saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{ProfileName}");
        string savePath = Path.Combine(saveFolderPathProfile + $"/{Data.SaveInfos.GUID}.save");

        FileStream stream = new FileStream(savePath, FileMode.Create);

        binaryFormatter.Serialize(stream, Data);
        stream.Close();
    }

    public static SaveData LoadSave(SaveInfos SaveInfos, string ProfileName)
    {
        string saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{ProfileName}");
        string savePath = Path.Combine(saveFolderPathProfile + $"/{SaveInfos.GUID}.save");

        if (File.Exists(savePath))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);

            SaveData data = binaryFormatter.Deserialize(stream) as SaveData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + savePath);
            return null;
        }
    }
}
