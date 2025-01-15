using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static SaveManager;
using System.Collections.Generic;
using System.Runtime.Serialization;

public static class SaveSystem
{
    /*
    public static void Save(List<ScriptSelection> ScriptSelections)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/player.test";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(ScriptSelections);
        //Dictionary<string, object> gameData = new Dictionary<string, object>();

        binaryFormatter.Serialize(stream, data);
        stream.Close();
    }
    */

    public static void Save(Dictionary<string, object> Data)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/player.test";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(Data);

        binaryFormatter.Serialize(stream, data);
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
