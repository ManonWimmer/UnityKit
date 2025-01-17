using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static UnityEngine.Rendering.DebugUI;

public class SaveManager : MonoBehaviour
{
    // ----- VARIABLES ----- //
    [System.Serializable]
    public class VariableSelection
    {
        public string variableName;
        public bool isSelected;
        public object value;
        public object defaultValue;
    }
    // ----- VARIABLES ----- //

    // ----- GAME OBJECT, SCRIPT & VARIABLES ----- //
    [System.Serializable]
    public class ScriptSelection
    {
        public GameObject targetGameObject;
        public MonoBehaviour selectedScript;
        public List<VariableSelection> variableSelections;
        public string guid = "";
    }
    // ----- GAME OBJECT, SCRIPT & VARIABLES ----- //

    // ----- FIELDS ----- //
    public List<ScriptSelection> scriptSelections = new List<ScriptSelection>();

    public List<string> ProfilesNames = new List<string>();
    public List<SaveData> CurrentProfileSaves = new List<SaveData>();
    public string CurrentProfile = "";

    public Dictionary<string, List<SaveData>> DictProfileSaveDatas = new Dictionary<string, List<SaveData>>();
    public Dictionary<string, int> DictProfileNbrSaves = new Dictionary<string, int>();

    public event Action OnAddProfile;
    public event Action OnAddSave;
    public event Action OnAllProfilesData;
    public event Action OnRemoveProfile;

    public static SaveManager Instance;

    private string saveFolderPath = "";


    private void Awake()
    {
        if (Instance != null) { Destroy(this); }
        Instance = this;

        GetDefaultSavedVariables();
    }
    // ----- FIELDS ----- //


    private object GetFieldValue(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (field != null)
        {
            return field.GetValue(obj);
        }

        var property = obj.GetType().GetProperty(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (property != null)
        {
            return property.GetValue(obj);
        }

        Debug.LogWarning($"Field or Property '{fieldName}' not found on {obj.GetType().Name}");
        return null;
    }

    
    private void GetDefaultSavedVariables()
    {
        Debug.Log("get default saved variables");
        foreach (var selection in scriptSelections)
        {
            if (selection.selectedScript != null)
            {
                foreach (var varSelection in selection.variableSelections)
                {
                    object value = GetFieldValue(selection.selectedScript, varSelection.variableName);
                    varSelection.defaultValue = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                    Debug.Log("default " + varSelection.defaultValue);
                }
            }
        }
    }



    public void NewSave(bool isDefault, string profileName)
    {
        Debug.Log($"SAVE {isDefault} {profileName}");
        CurrentProfile = profileName;
        if (profileName == "" || scriptSelections == null) return;
        Dictionary <string, object> data = new Dictionary<string, object>();

        string saveGUID = Guid.NewGuid().ToString();

        DateTime now = DateTime.Now;
        string currentDate = now.ToString("yyyy-MM-dd"); // Format : "2025-01-16"
        Debug.Log("Date actuelle : " + currentDate);
        string currentTime = now.ToString("HH:mm:ss"); // Format : "16:45:30"
        Debug.Log("Heure actuelle : " + currentTime);

        int saveNbr = DictProfileNbrSaves[CurrentProfile] + 1;
        Debug.Log($"SAVE NBR {saveNbr}");

        SaveInfos dataInfos = new SaveInfos(saveGUID, currentDate, currentTime, saveNbr);

        foreach (var selection in scriptSelections)
        {
            if (selection.targetGameObject != null && selection.selectedScript != null && selection.variableSelections.Count > 0)
            {
                data.Add(selection.guid, selection.variableSelections); // GUID - List variables
                Debug.Log($"add to dict data, guid : {selection.guid}, game object : {selection.targetGameObject}, script : {selection.selectedScript}, list variables :");
                foreach (var varSelection in selection.variableSelections) 
                {
                    GameObject gameObject = selection.targetGameObject as GameObject;
                    if (isDefault)
                    {
                        object value = varSelection.defaultValue;
                        varSelection.value = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                        Debug.Log($"Save Default Variable: {varSelection.variableName}, Value: {value}, Is Selected: {varSelection.isSelected} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");

                    }
                    else
                    {
                        object value = GetFieldValue(selection.selectedScript, varSelection.variableName);
                        varSelection.value = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                        Debug.Log($"Save Variable: {varSelection.variableName}, Value: {value}, Is Selected: {varSelection.isSelected} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");

                    }
                    Debug.Log("default value " + varSelection.defaultValue);
                }
            }
        }

        Debug.Log($"Save Infos: GUID {dataInfos.GUID}, Date {dataInfos.Date}, Time {dataInfos.Time}");

        SaveData saveData = new SaveData(data, dataInfos);

        SaveSystem.NewSave(saveData, profileName);

        OnAddSave.Invoke();
    }

    public void OverrideSave(SaveData lastSave)
    {
        SaveData overrideSave = lastSave;
        Debug.Log($"OVERRIDE SAVE {overrideSave.SaveInfos.GUID} {CurrentProfile}");

        if (CurrentProfile == "" || scriptSelections == null) return;
        Dictionary<string, object> data = new Dictionary<string, object>();

        // Keep GUID but new date & time
        string saveGUID = overrideSave.SaveInfos.GUID;

        DateTime now = DateTime.Now;
        string currentDate = now.ToString("yyyy-MM-dd"); // Format : "2025-01-16"
        string currentTime = now.ToString("HH:mm:ss"); // Format : "16:45:30"

        int saveNbr = DictProfileNbrSaves[CurrentProfile] + 1;
        Debug.Log($"SAVE NBR {saveNbr}");

        SaveInfos dataInfos = new SaveInfos(saveGUID, currentDate, currentTime, saveNbr);

        foreach (var selection in scriptSelections)
        {
            if (selection.targetGameObject != null && selection.selectedScript != null && selection.variableSelections.Count > 0)
            {
                data.Add(selection.guid, selection.variableSelections); // GUID - List variables
                Debug.Log($"add to dict data, guid : {selection.guid}, game object : {selection.targetGameObject}, script : {selection.selectedScript}, list variables :");
                foreach (var varSelection in selection.variableSelections)
                {
                    GameObject gameObject = selection.targetGameObject as GameObject;
                    object value = GetFieldValue(selection.selectedScript, varSelection.variableName);
                    varSelection.value = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                    Debug.Log($"Save Variable: {varSelection.variableName}, Value: {value}, Is Selected: {varSelection.isSelected} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");

                }
            }
        }

        Debug.Log($"Save Infos: GUID {dataInfos.GUID}, Date {dataInfos.Date}, Time {dataInfos.Time}");

        SaveData saveData = new SaveData(data, dataInfos);

        SaveSystem.NewSave(saveData, CurrentProfile);

        OnAddSave.Invoke();
    }


    public void LoadSave(SaveInfos SaveInfos)
    {
        Debug.Log("ici");
        if (CurrentProfile == "" || SaveInfos == null) return;

        SaveData data = SaveSystem.LoadSave(SaveInfos, CurrentProfile);

        foreach (var entry in data.DictVariables)
        {
            string guid = entry.Key;
            List<SaveManager.VariableSelection> variables = entry.Value as List<SaveManager.VariableSelection>;

            SaveManager.ScriptSelection selection = GetScriptSelectionFromGUID(guid);

            if (selection != null)
            {
                GameObject gameObject = selection.targetGameObject as GameObject;
                Component component = selection.selectedScript as Component;

                if (gameObject != null && component != null)
                {
                    foreach (var varSelection in variables)
                    {
                        if (varSelection.isSelected)
                        {
                            // Définir la valeur chargée dans la variable du script
                            SetFieldValue(component, varSelection.variableName, varSelection.value);
                            Debug.Log($"Loaded Variable: {varSelection.variableName}, Set Value: {varSelection.value} in GameObject: {gameObject.name}, Script: {component.ToString()}");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Invalid GameObject or Component for GUID: {guid}");
                }
            }
            else
            {
                Debug.LogWarning($"Script selection with GUID {guid} not found.");
            }
        }
    }

    public void DeleteSave(SaveInfos saveInfos)
    {
        SaveSystem.DeleteSave(saveInfos, CurrentProfile);

        OnAddSave.Invoke();
    }

    public void DeleteProfile(string profileName)
    {
        Debug.Log("delete profile");
        SaveSystem.DeleteProfile(profileName);
        ProfilesNames.Remove(profileName);
        DictProfileSaveDatas.Remove(profileName);
        DictProfileNbrSaves.Remove(profileName);

        OnRemoveProfile.Invoke();
    }

    private void SetFieldValue(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(obj, value);
            return;
        }

        var property = obj.GetType().GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
            return;
        }

        Debug.LogWarning($"Field or Property '{fieldName}' not found or not writable on {obj.GetType().Name}");
    }

    private ScriptSelection GetScriptSelectionFromGUID(string guid)
    {
        foreach(var selection in scriptSelections) {
            if (guid == selection.guid)
                return selection;
        }

        return null;
    }

    public void CreateProfile(string profileName)
    {
        profileName = profileName.ToUpper();

        if (!ProfilesNames.Contains(profileName))
        {
            Debug.Log("create profile");
            ProfilesNames.Add(profileName);
            DictProfileNbrSaves[profileName] = 0;

            // Définir le chemin du dossier de sauvegarde
            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");

            // Vérifier et créer le dossier si nécessaire
            CreateSaveFolder(profileName);

            NewSave(true, profileName);

            //OnAddProfile.Invoke();
        }
        
    }

    private void CreateSaveFolder(string profileName)
    {
        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{profileName}");

        if (!Directory.Exists(saveFolderPathProfile))
        {
            Directory.CreateDirectory(saveFolderPathProfile);
            Debug.Log("Dossier de sauvegardes créé : " + saveFolderPathProfile);
        }
        else
        {
            Debug.Log("Dossier de sauvegardes déjà existant : " + saveFolderPathProfile);
        }
    }

    public void GetProfilesDirectories()
    {
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        }

        // Vérifier si le dossier existe
        if (Directory.Exists(saveFolderPath))
        {
            // Obtenir les noms de tous les sous-dossiers
            string[] folders = Directory.GetDirectories(saveFolderPath);
            Debug.Log("Sous-dossiers dans " + saveFolderPath + ":");

            foreach (string folder in folders)
            {
                Debug.Log(Path.GetFileName(folder)); // Affiche uniquement le nom du dossier
                string folderName = Path.GetFileName(folder);
                if (!ProfilesNames.Contains((string)folderName)) ProfilesNames.Add((string)folderName);
            }
        }
        else
        {
            Debug.LogWarning("Le dossier spécifié n'existe pas : " + saveFolderPath);
        }
    }

    public void GetCurrentProfileSaves()
    {
        CurrentProfileSaves = GetProfileSaves(CurrentProfile);
        int saveNbr = 0;
        foreach(SaveData saveData in CurrentProfileSaves)
        {
            if (saveData.SaveInfos.SaveNbr > saveNbr) saveNbr = saveData.SaveInfos.SaveNbr;
        }
        Debug.Log($"MAX SAVE NBR {saveNbr}");
        DictProfileNbrSaves[CurrentProfile] = saveNbr;
    }

    public void GetAllProfilesSaves()
    {
        if (ProfilesNames.Count == 0) return;

        if (string.IsNullOrEmpty(saveFolderPath))
        {
            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        }

        DictProfileSaveDatas.Clear();

        foreach (string profileName in ProfilesNames)
        {
            List<SaveData> ProfileSaves = new List<SaveData>();
            ProfileSaves = GetProfileSaves(profileName);
            DictProfileSaveDatas.Add(profileName, ProfileSaves);
        }

        Debug.Log("end profiles saves");
        OnAllProfilesData.Invoke();
    }

    public List<SaveData> GetProfileSaves(string profileName)
    {
        if (profileName == "") return null;

        if (string.IsNullOrEmpty(saveFolderPath))
        {
            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        }

        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{profileName}");

        // Vérifier si le dossier existe
        if (Directory.Exists(saveFolderPathProfile))
        {
            // Obtenir les noms de tous les sous-dossiers
            string[] files = Directory.GetFiles(saveFolderPathProfile, "*.save");
            Debug.Log("Fichiers dans " + saveFolderPath + ":");


            List<SaveData> ProfileSaves = new List<SaveData>();

            /*
            if (files.Length == 0)
            {
                Debug.Log("Create default save");
                NewSave(true, profileName);
            }*/

            foreach (string file in files)
            {
                Debug.Log(Path.GetFileName(file)); // Affiche uniquement le nom du dossier
                string filePath = Path.GetFullPath(file);

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream stream = new FileStream(filePath, FileMode.Open);

                SaveData data = binaryFormatter.Deserialize(stream) as SaveData;
                Debug.Log($"found data {data.SaveInfos.GUID} {data.SaveInfos.Time}");
                stream.Close();

                if (!ProfileSaves.Contains(data)) ProfileSaves.Add(data);
            }

            // trier les saves du plus récent au plus ancien
            ProfileSaves = ProfileSaves.OrderByDescending(save =>
            DateTime.Parse(save.SaveInfos.Date + " " + save.SaveInfos.Time)).ToList();

            return ProfileSaves;
        }
        else
        {
            Debug.LogWarning("Le dossier spécifié n'existe pas : " + saveFolderPath);
            return null;
        }
    }
}
