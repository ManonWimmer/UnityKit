using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static SaveManager;
using System.Collections.Generic;
using System.Runtime.Serialization;

public static class SaveSystem
{
    public static void Save(SaveData Data)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/player.test";
        FileStream stream = new FileStream(path, FileMode.Create);

        binaryFormatter.Serialize(stream, Data);
        stream.Close();
    }

    public static SaveData Load()
    {
        string path = Application.persistentDataPath + "/player.test";
        if (File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = binaryFormatter.Deserialize(stream) as SaveData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
