using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager : MonoBehaviour
{
    // ----- VARIABLES ----- //
    [System.Serializable]
    public class VariableSelection
    {
        public string variableName;
        public bool isSelected;
        public object value;
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

    public event Action OnAddProfile;
    public event Action OnAddSave;

    public static SaveManager Instance;

    private string saveFolderPath = "";


    private void Awake()
    {
        if (Instance != null) { Destroy(this); }
        Instance = this;
    }
    // ----- FIELDS ----- //

    private void Start()
    {
        DebugSavedVariables();
    }

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

    
    private void DebugSavedVariables()
    {
        // Check each gameobject -> script -> variable if is selected
        foreach (var selection in scriptSelections)
        {
            if (selection.selectedScript != null)
            {
                foreach (var varSelection in selection.variableSelections)
                {
                    if (varSelection.isSelected)
                    {
                        GameObject gameObject = selection.targetGameObject as GameObject;
                        object value = GetFieldValue(selection.selectedScript, varSelection.variableName);
                        Debug.Log($"Save Variable: {varSelection.variableName}, Value: {value} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");
                    }
                }
            }
        }
    }
    

    public void NewSave()
    {
        if (CurrentProfile == "" || scriptSelections == null) return;
        Dictionary <string, object> data = new Dictionary<string, object>();

        string saveGUID = Guid.NewGuid().ToString();
        DateTime now = DateTime.Now;
        string currentDate = now.ToString("yyyy-MM-dd"); // Format : "2025-01-16"
        Debug.Log("Date actuelle : " + currentDate);
        string currentTime = now.ToString("HH:mm:ss"); // Format : "16:45:30"
        Debug.Log("Heure actuelle : " + currentTime);
        SaveInfos dataInfos = new SaveInfos(saveGUID, currentDate, currentTime);

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
                    Debug.Log($"Save Variable: {varSelection.variableName}, Value: {value}, Is Selected: {varSelection.isSelected} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");
                    varSelection.value = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
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
                            // D�finir la valeur charg�e dans la variable du script
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
            ProfilesNames.Add(profileName);
        }

        // D�finir le chemin du dossier de sauvegarde
        saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");

        // V�rifier et cr�er le dossier si n�cessaire
        CreateSaveFolder(profileName);

        OnAddProfile.Invoke();
    }

    private void CreateSaveFolder(string profileName)
    {
        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{profileName}");

        if (!Directory.Exists(saveFolderPathProfile))
        {
            Directory.CreateDirectory(saveFolderPathProfile);
            Debug.Log("Dossier de sauvegardes cr�� : " + saveFolderPathProfile);
        }
        else
        {
            Debug.Log("Dossier de sauvegardes d�j� existant : " + saveFolderPathProfile);
        }
    }

    public void GetProfilesDirectories()
    {
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        }

        // V�rifier si le dossier existe
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
            Debug.LogWarning("Le dossier sp�cifi� n'existe pas : " + saveFolderPath);
        }
    }

    public void GetCurrentProfileSaves()
    {
        if (CurrentProfile == "") return;

        if (string.IsNullOrEmpty(saveFolderPath))
        {
            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        }

        string saveFolderPathProfile = Path.Combine(saveFolderPath, $"{CurrentProfile}");

        // V�rifier si le dossier existe
        if (Directory.Exists(saveFolderPathProfile))
        {
            // Obtenir les noms de tous les sous-dossiers
            string[] files = Directory.GetFiles(saveFolderPathProfile, "*.save");
            Debug.Log("Fichiers dans " + saveFolderPath + ":");

            CurrentProfileSaves.Clear();

            foreach (string file in files)
            {
                Debug.Log(Path.GetFileName(file)); // Affiche uniquement le nom du dossier
                string filePath = Path.GetFullPath(file);

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream stream = new FileStream(filePath, FileMode.Open);

                SaveData data = binaryFormatter.Deserialize(stream) as SaveData;
                Debug.Log($"found data {data.SaveInfos.GUID} {data.SaveInfos.Time}");
                stream.Close();

                CurrentProfileSaves.Add(data);
            }
        }
        else
        {
            Debug.LogWarning("Le dossier sp�cifi� n'existe pas : " + saveFolderPath);
        }
    }
}
