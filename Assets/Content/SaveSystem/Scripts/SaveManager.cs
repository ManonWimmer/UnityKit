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
using System.Globalization;
using static UnityEditor.Rendering.InspectorCurveEditor;

public class SaveManager : MonoBehaviour
{
    // ----- VARIABLES ----- //
    [System.Serializable]
    public class VariableSelection
    {
        public string VariableName;
        public bool IsSaved;
        public object SavedValue;
        public object DefaultValue;
    }
    // ----- VARIABLES ----- //

    // ----- TRANSFORM ----- //
    [System.Serializable]
    public class TransformSelection
    {
        public object Transform;

        // Position
        public bool positionSelected;
        public float[] positionValue = new float[3];
        public float[] defaultPositionValue = new float[3];

        // Rotation
        public bool rotationSelected;
        public float[] rotationValue = new float[4];
        public float[] defaultRotationValue = new float[4];

        // Scale
        public bool scaleSelected;
        public float[] scaleValue = new float[3];
        public float[] defaultScaleValue = new float[3];

        public TransformSelection(bool positionSelected = false, bool rotationSelected = false, bool scaleSelected = false)
        {
            this.positionSelected = positionSelected;
            this.rotationSelected = rotationSelected;
            this.scaleSelected = scaleSelected;
        }
    }
    // ----- TRANSFORM ----- //

    // ----- GAME OBJECT, SCRIPT & VARIABLES ----- //
    [System.Serializable]
    public class SavedEntry
    {
        public GameObject SelectedGameObject;
        public List<ScriptData> ScriptDatas;
        public TransformSelection TransformSelection;
        public Transform Transform;
        public string SelectionGUID = "";
    }

    [System.Serializable]
    public class ScriptData
    {
        public MonoBehaviour SelectedScript;
        public List<VariableSelection> VariableSelections;
    }
    // ----- GAME OBJECT, SCRIPT & VARIABLES ----- //

    // ----- FIELDS ----- //
    public List<SavedEntry> SavedEntries = new List<SavedEntry>();

    public List<string> ProfilesNames = new List<string>();
    public List<SaveData> CurrentProfileSaves = new List<SaveData>();
    public string CurrentProfile = "";

    public Dictionary<string, List<SaveData>> DictProfileSaveDatas = new Dictionary<string, List<SaveData>>();
    public Dictionary<string, int> DictProfileNbrSaves = new Dictionary<string, int>();

    public event Action OnUpdateSave;
    public event Action OnAllProfilesData;
    public event Action OnRemoveProfile;

    public static SaveManager Instance;

    private string saveFolderPath = "";
    // ----- FIELDS ----- //

    private void Awake()
    {
        if (Instance != null) { Destroy(this); }
        Instance = this;

        GetDefaultSavedVariables();
        GetDefaultTransformValues();
    }

    #region Get & Set Values - Variables
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

        //Debug.LogWarning($"Field or Property '{fieldName}' not found or not writable on {obj.GetType().Name}");
    }

    private void GetDefaultSavedVariables()
    {
        foreach (var savedEntry in SavedEntries)
        {
            if (savedEntry.ScriptDatas.Count > 0)
            {
                foreach (var scriptData in savedEntry.ScriptDatas) 
                {
                    if (scriptData.SelectedScript != null)
                    {
                        foreach (var varSelection in scriptData.VariableSelections)
                        {
                            object value = GetFieldValue(scriptData.SelectedScript, varSelection.VariableName);
                            varSelection.DefaultValue = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                            //Debug.Log("default " + varSelection.defaultValue);
                        }
                    }
                }
            }
            
        }
    }
    #endregion

    #region Set & Get Values - Transform
    private void SetTransformValues(SavedEntry savedEntry)
    {
        Transform selectedTransform = savedEntry.Transform;

        // -- Position -- //
        float[] positionValue = new float[3];
        positionValue[0] = selectedTransform.position.x;
        positionValue[1] = selectedTransform.position.y;
        positionValue[2] = selectedTransform.position.z;
        
        savedEntry.TransformSelection.positionValue = positionValue;
        Debug.Log($"Set Transform position : {positionValue[0]}, {positionValue[1]}, {positionValue[2]}");
        // -- Position -- //

        // -- Rotation -- //
        float[] rotationValue = new float[4];
        rotationValue[0] = selectedTransform.localRotation.w;
        rotationValue[1] = selectedTransform.localRotation.x;
        rotationValue[2] = selectedTransform.localRotation.y;
        rotationValue[3] = selectedTransform.localRotation.z;

        savedEntry.TransformSelection.rotationValue = rotationValue;
        // -- Rotation -- //

        // -- Scale -- //
        float[] scaleValue = new float[3];
        scaleValue[0] = selectedTransform.localScale.x;
        scaleValue[1] = selectedTransform.localScale.y;
        scaleValue[2] = selectedTransform.localScale.z;

        savedEntry.TransformSelection.scaleValue = scaleValue;
        // -- Scale -- //
    }

    private void GetDefaultTransformValues()
    {
        foreach (var savedEntry in SavedEntries)
        {
            // Position
            float[] positionValue = new float[3];
            positionValue[0] = savedEntry.Transform.position[0];
            positionValue[1] = savedEntry.Transform.position[1];
            positionValue[2] = savedEntry.Transform.position[2];

            savedEntry.TransformSelection.defaultPositionValue = positionValue;
            Debug.Log($"Get Transform Default position : {savedEntry.TransformSelection.defaultPositionValue[0]}, {savedEntry.TransformSelection.defaultPositionValue[1]}, {savedEntry.TransformSelection.defaultPositionValue[2]}");

            // Rotation
            float[] rotationValue = new float[4];
            rotationValue[0] = savedEntry.Transform.localRotation.w;
            rotationValue[1] = savedEntry.Transform.localRotation.x;
            rotationValue[2] = savedEntry.Transform.localRotation.y;
            rotationValue[3] = savedEntry.Transform.localRotation.z;

            savedEntry.TransformSelection.defaultRotationValue = rotationValue;

            // Scale
            float[] scaleValue = new float[3];
            scaleValue[0] = savedEntry.Transform.localScale.x;
            scaleValue[1] = savedEntry.Transform.localScale.y;
            scaleValue[2] = savedEntry.Transform.localScale.z;

            savedEntry.TransformSelection.defaultScaleValue = scaleValue;
        }
    }

    private void SetTransformDefaultValues(SavedEntry savedEntry)
    {
        TransformSelection transformSelection = savedEntry.TransformSelection;

        // Position
        float[] positionValue = new float[3];
        positionValue[0] = transformSelection.defaultPositionValue[0];
        positionValue[1] = transformSelection.defaultPositionValue[1];
        positionValue[2] = transformSelection.defaultPositionValue[2];

        savedEntry.TransformSelection.positionValue = positionValue;
        Debug.Log($"Set Transform Default position : {savedEntry.TransformSelection.positionValue[0]}, {savedEntry.TransformSelection.positionValue[1]}, {savedEntry.TransformSelection.positionValue[2]}");

        // Rotation
        float[] rotationValue = new float[4];
        rotationValue[0] = transformSelection.defaultRotationValue[0];
        rotationValue[1] = transformSelection.defaultRotationValue[1];
        rotationValue[2] = transformSelection.defaultRotationValue[2];
        rotationValue[3] = transformSelection.defaultRotationValue[3];

        savedEntry.TransformSelection.rotationValue = rotationValue;

        // Scale
        float[] scaleValue = new float[3];
        scaleValue[0] = transformSelection.defaultScaleValue[0];
        scaleValue[1] = transformSelection.defaultScaleValue[1];
        scaleValue[2] = transformSelection.defaultScaleValue[2];

        savedEntry.TransformSelection.scaleValue = scaleValue;
    }
    #endregion

    #region Create & Override Save
    public void NewSave(bool isDefault, string profileName)
    {
        Debug.Log($"SAVE {isDefault} {profileName}");
        CurrentProfile = profileName;

        if (profileName == "" || SavedEntries == null) return;

        List<(string, object)> ListTupleSelectionGUIDListVariables = new List<(string, object)>();
        Dictionary<string, TransformSelection> DictSelectionGUIDTransformSelection = new Dictionary<string, TransformSelection>();

        string saveGUID = Guid.NewGuid().ToString();

        // Get Data & Time
        DateTime now = DateTime.Now;
        string currentDate = now.ToString("yyyy-MM-dd"); 
        string currentTime = now.ToString("HH:mm:ss");

        int saveNbr = DictProfileNbrSaves[CurrentProfile] + 1;

        SaveInfos dataInfos = new SaveInfos(saveGUID, currentDate, currentTime, saveNbr);

        foreach (var savedEntry in SavedEntries)
        {
            if (savedEntry.ScriptDatas.Count > 0)
            {
                foreach (var scriptData in savedEntry.ScriptDatas)
                {
                    Debug.Log("script data : " + scriptData.SelectedScript.ToString());
                    if (savedEntry.SelectedGameObject != null && scriptData.SelectedScript != null && scriptData.VariableSelections.Count > 0)
                    {
                        ListTupleSelectionGUIDListVariables.Add((savedEntry.SelectionGUID, scriptData.VariableSelections));

                        // -- SAVE VARIABLES -- //
                        foreach (var varSelection in scriptData.VariableSelections)
                        {
                            if (isDefault)
                            {
                                object value = varSelection.DefaultValue;
                                varSelection.SavedValue = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                                //Debug.Log($"Save Default Variable: {varSelection.variableName}, Value: {value}, Is Selected: {varSelection.isSelected} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");
                            }
                            else
                            {
                                object value = GetFieldValue(scriptData.SelectedScript, varSelection.VariableName);
                                varSelection.SavedValue = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                                //Debug.Log($"Save Variable: {varSelection.variableName}, Value: {value}, Is Selected: {varSelection.isSelected} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");
                            }
                        }
                        // -- SAVE VARIABLES -- //
                    }
                }
            }

            // -- SAVE TRANSFORM -- //
            Transform selectedTransform = savedEntry.Transform;
            Debug.Log($"Saved transform {selectedTransform}");

            if (isDefault)
            {
                SetTransformDefaultValues(savedEntry);
            }
            else
            {
                SetTransformValues(savedEntry);
            }

            Debug.Log($"Saved position : {savedEntry.TransformSelection.positionValue[0]}, {savedEntry.TransformSelection.positionValue[1]}, {savedEntry.TransformSelection.positionValue[2]} Save N° {dataInfos.SaveNbr}");

            DictSelectionGUIDTransformSelection.Add(savedEntry.SelectionGUID, savedEntry.TransformSelection);
            // -- SAVE TRANSFORM -- //
        }

        SaveData saveData = new SaveData(ListTupleSelectionGUIDListVariables, DictSelectionGUIDTransformSelection, dataInfos);

        SaveSystem.NewSave(saveData, profileName);

        OnUpdateSave.Invoke();
    }

    public void OverrideSave(SaveData lastSave)
    {
        SaveData overrideSave = lastSave;
        Debug.Log($"OVERRIDE SAVE {overrideSave.SaveInfos.SaveGUID} {CurrentProfile}");

        if (CurrentProfile == "" || SavedEntries == null) return;

        List<(string, object)> ListTupleSelectionGUIDListVariables = new List<(string, object)>();
        Dictionary<string, TransformSelection> DictSelectionGUIDTransformSelection = new Dictionary<string, TransformSelection>();

        // Keep GUID
        string saveGUID = overrideSave.SaveInfos.SaveGUID;

        // New Date & Time
        DateTime now = DateTime.Now;
        string currentDate = now.ToString("yyyy-MM-dd"); 
        string currentTime = now.ToString("HH:mm:ss"); 

        int saveNbr = lastSave.SaveInfos.SaveNbr;

        SaveInfos dataInfos = new SaveInfos(saveGUID, currentDate, currentTime, saveNbr);

        foreach (var savedEntry in SavedEntries)
        {
            if (savedEntry.ScriptDatas.Count > 0)
            {
                foreach (var scriptData in savedEntry.ScriptDatas)
                {
                    if (savedEntry.SelectedGameObject != null && scriptData.SelectedScript != null && scriptData.VariableSelections.Count > 0)
                    {
                        ListTupleSelectionGUIDListVariables.Add((savedEntry.SelectionGUID, scriptData.VariableSelections)); // GUID - List variables

                        // -- SAVE VARIABLES -- //
                        foreach (var varSelection in scriptData.VariableSelections)
                        {
                            object value = GetFieldValue(scriptData.SelectedScript, varSelection.VariableName);
                            varSelection.SavedValue = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                        }
                        // -- SAVE VARIABLES -- //  
                    }
                }
            }

            // -- SAVE TRANSFORM -- //
            Transform selectedTransform = savedEntry.Transform;
            Debug.Log($"Saved transform {selectedTransform}");

            SetTransformValues(savedEntry);

            Debug.Log($"Saved position : {savedEntry.TransformSelection.positionValue[0]}, {savedEntry.TransformSelection.positionValue[1]}, {savedEntry.TransformSelection.positionValue[2]} Save N° {dataInfos.SaveNbr}");

            DictSelectionGUIDTransformSelection.Add(savedEntry.SelectionGUID, savedEntry.TransformSelection);
            // -- SAVE TRANSFORM -- //
        }

        SaveData saveData = new SaveData(ListTupleSelectionGUIDListVariables, DictSelectionGUIDTransformSelection, dataInfos);

        SaveSystem.NewSave(saveData, CurrentProfile);

        OnUpdateSave.Invoke();
    }
    #endregion

    #region Load Save
    public void LoadSave(SaveInfos SaveInfos)
    {
        if (CurrentProfile == "" || SaveInfos == null) return;

        SaveData data = SaveSystem.LoadSave(SaveInfos, CurrentProfile);

        foreach (var (selectionGUID, variablesObject) in data.ListTupleSelectionGUIDListVariables)
        {
            List<VariableSelection> ListVariableSelections = variablesObject as List<VariableSelection>;
            TransformSelection transformSelection = data.DictSelectionGUIDTransformSelection[selectionGUID];

            SaveManager.SavedEntry savedEntry = GetScriptSelectionFromGUID(selectionGUID);

            if (savedEntry != null)
            {
                GameObject gameObject = savedEntry.SelectedGameObject;

                if (savedEntry.ScriptDatas.Count > 0)
                {
                    foreach (var scriptData in savedEntry.ScriptDatas)
                    {
                        Component component = scriptData.SelectedScript;

                        if (gameObject != null && component != null)
                        {
                            // -- LOAD VARIABLES -- //
                            foreach (var varSelection in ListVariableSelections)
                            {
                                if (varSelection.IsSaved)
                                {
                                    SetFieldValue(component, varSelection.VariableName, varSelection.SavedValue);
                                    //Debug.Log($"Loaded Variable: {varSelection.variableName}, Set Value: {varSelection.value} in GameObject: {gameObject.name}, Script: {component.ToString()}");
                                }
                            }
                            // -- LOAD VARIABLES -- // 
                        }
                    }
                }

                // -- LOAD TRANSFORMS -- //
                Transform selectedTransform = savedEntry.Transform;
                if (selectedTransform != null)
                {
                    if (transformSelection != null)
                    {
                        // -- Position -- //
                        if (transformSelection.positionSelected)
                        {
                            Vector3 position = new Vector3(transformSelection.positionValue[0], transformSelection.positionValue[1], transformSelection.positionValue[2]);
                            selectedTransform.position = position;

                            Debug.Log($"LOAD POSITION : {position} guid {data.SaveInfos.SaveGUID} Save N° {data.SaveInfos.SaveNbr}");
                        }
                        // -- Position -- //

                        // -- Rotation -- //
                        if (transformSelection.rotationSelected)
                        {
                            Quaternion rotation = new Quaternion(transformSelection.rotationValue[0], transformSelection.rotationValue[1], transformSelection.rotationValue[2], transformSelection.rotationValue[3]);
                            selectedTransform.rotation = rotation;
                        }
                        // -- Rotation -- //

                        // -- Scale -- //
                        if (transformSelection.scaleSelected)
                        {
                            Vector3 scale = new Vector3(transformSelection.scaleValue[0], transformSelection.scaleValue[1], transformSelection.scaleValue[2]);
                            selectedTransform.localScale = scale;
                        }
                        // -- Scale -- //
                    }
                }
                else
                {
                    Debug.LogError($"Invalid GameObject or Component for GUID: {selectionGUID}");
                }
                // -- LOAD TRANSFORMS -- //
            }

        }
        
    }
    #endregion

    #region Delete Save & Profile
    public void DeleteSave(SaveInfos saveInfos)
    {
        SaveSystem.DeleteSave(saveInfos, CurrentProfile);

        OnUpdateSave.Invoke();
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
    #endregion

    #region Get Selection From GUID
    private SavedEntry GetScriptSelectionFromGUID(string SelectionGUID)
    {
        foreach(var selection in SavedEntries) {
            if (SelectionGUID == selection.SelectionGUID)
                return selection;
        }

        return null;
    }
    #endregion

    #region Create Profile & Save Directory
    public void CreateProfile(string profileName)
    {
        profileName = profileName.ToUpper();

        if (!ProfilesNames.Contains(profileName))
        {
            Debug.Log("create profile");
            ProfilesNames.Add(profileName);
            DictProfileNbrSaves[profileName] = 0;

            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");

            CreateSaveFolder(profileName);

            NewSave(true, profileName);
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
    #endregion

    #region Get Profiles Directories & Saves
    public void GetProfilesDirectories()
    {
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        }

        if (Directory.Exists(saveFolderPath))
        {
            string[] folders = Directory.GetDirectories(saveFolderPath);

            foreach (string folder in folders)
            {
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

        if (Directory.Exists(saveFolderPathProfile))
        {
            string[] files = Directory.GetFiles(saveFolderPathProfile, "*.save");

            List<SaveData> ProfileSaves = new List<SaveData>();

            foreach (string file in files)
            {
                //Debug.Log(Path.GetFileName(file)); 
                string filePath = Path.GetFullPath(file);

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream stream = new FileStream(filePath, FileMode.Open);

                SaveData data = binaryFormatter.Deserialize(stream) as SaveData;
                //Debug.Log($"found data {data.SaveInfos.GUID} {data.SaveInfos.Time}");
                stream.Close();

                if (!ProfileSaves.Contains(data)) ProfileSaves.Add(data);
            }

            // Order from most recent
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
    #endregion 
}
