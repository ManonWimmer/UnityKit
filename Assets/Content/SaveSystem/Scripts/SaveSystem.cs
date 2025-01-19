using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static SaveManager;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

public static class SaveSystem
{
    public static void NewSave(SaveData Data, string ProfileName)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        string saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{ProfileName}");
        string savePath = Path.Combine(saveFolderPathProfile + $"/{Data.SaveInfos.SaveGUID}.save");

        FileStream stream = new FileStream(savePath, FileMode.Create);

        binaryFormatter.Serialize(stream, Data);
        stream.Close();
    }

    public static void DeleteSave(SaveInfos SaveInfos, string ProfileName)
    {
        if (SaveInfos == null || string.IsNullOrEmpty(ProfileName))
        {
            Debug.LogError("Invalid SaveInfos or ProfileName.");
            return;
        }

        string saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{ProfileName}");
        string savePath = Path.Combine(saveFolderPathProfile + $"/{SaveInfos.SaveGUID}.save");

        if (File.Exists(savePath))
        {
            try
            {
                File.Delete(savePath);
                Debug.Log($"Delete file at {savePath} successfully deleted.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Delete deleting save file: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Delete file at {savePath} does not exist.");
        }
    }

    public static void DeleteProfile(string ProfileName)
    {
        if (string.IsNullOrEmpty(ProfileName))
        {
            Debug.LogError("Invalid ProfileName.");
            return;
        }

        string saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        string profileFolderPath = Path.Combine(saveFolderPath, $"{ProfileName}");

        if (Directory.Exists(profileFolderPath))
        {
            try
            {
                // Supprimer tous les fichiers dans le répertoire
                string[] files = Directory.GetFiles(profileFolderPath);
                foreach (string file in files)
                {
                    File.Delete(file);
                    Debug.Log($"Deleted file: {file}");
                }

                // Supprimer tous les sous-dossiers dans le répertoire
                string[] subDirs = Directory.GetDirectories(profileFolderPath);
                foreach (string subDir in subDirs)
                {
                    // Appel récursif pour supprimer tout dans les sous-dossiers
                    Directory.Delete(subDir, true);
                    Debug.Log($"Deleted subdirectory: {subDir}");
                }

                // Après avoir supprimé tout le contenu, supprimer le dossier
                Directory.Delete(profileFolderPath);
                Debug.Log($"Profile folder at {profileFolderPath} successfully deleted.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deleting profile folder: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Profile folder at {profileFolderPath} does not exist.");
        }
    }




    public static SaveData LoadSave(SaveInfos SaveInfos, string ProfileName)
    {
        Debug.Log("load save system");
        string saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{ProfileName}");
        string savePath = Path.Combine(saveFolderPathProfile + $"/{SaveInfos.SaveGUID}.save");

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
