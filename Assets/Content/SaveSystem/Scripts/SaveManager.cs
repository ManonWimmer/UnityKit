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
        public string variableName;
        public bool isSelected;
        public object value;
        public object defaultValue;
    }
    // ----- VARIABLES ----- //

    // ----- TRANSFORM ----- //
    [System.Serializable]
    public class TransformSelection
    {
        public object transform;

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

        /*
        public TransformSelection(Transform transform, bool positionSelected = false, bool rotationSelected = false, bool scaleSelected = false)
        {
            Debug.Log($"SET TRANSFORM VALUES {transform}");
            this.transform = transform ;

            // Position
            positionValue[0] = transform.position.x;
            positionValue[1] = transform.position.y;
            positionValue[2] = transform.position.z;
            this.positionSelected = positionSelected;

            // Rotation
            rotationValue[0] = transform.localRotation.w;
            rotationValue[1] = transform.localRotation.x;
            rotationValue[2] = transform.localRotation.y;
            rotationValue[3] = transform.localRotation.z;
            this.rotationSelected = rotationSelected;

            // Scale
            scaleValue[0] = transform.localScale.x;
            scaleValue[1] = transform.localScale.y;
            scaleValue[2] = transform.localScale.z;
            this.scaleSelected = scaleSelected;
        }
        */

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
    public class ScriptSelection
    {
        public GameObject targetGameObject;
        public MonoBehaviour selectedScript;
        public List<VariableSelection> variableSelections;
        public TransformSelection transformSelection;
        public Transform transform;
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
                // VARIABLES 
                foreach (var varSelection in selection.variableSelections)
                {
                    object value = GetFieldValue(selection.selectedScript, varSelection.variableName);
                    varSelection.defaultValue = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                    //Debug.Log("default " + varSelection.defaultValue);
                }

                SetTransformValues(selection);

                // TRANSFORM
                float[] positionValue = new float[3];
                positionValue[0] = selection.transform.position.x;
                positionValue[1] = selection.transform.position.y;
                positionValue[2] = selection.transform.position.z;
                selection.transformSelection.defaultPositionValue = (float[])positionValue.Clone();
                //Debug.Log($"Transform position : {positionValue[0]}, {positionValue[1]}, {positionValue[2]}");
                Debug.Log($"Default position : {selection.transformSelection.defaultPositionValue[0]}, {selection.transformSelection.defaultPositionValue[1]}, {selection.transformSelection.defaultPositionValue[2]}");

                float[] rotationValue = new float[4];
                rotationValue[0] = selection.transform.localRotation.w;
                rotationValue[1] = selection.transform.localRotation.x;
                rotationValue[2] = selection.transform.localRotation.y;
                rotationValue[3] = selection.transform.localRotation.z;
                selection.transformSelection.defaultRotationValue = (float[])rotationValue.Clone();

                float[] scaleValue = new float[3];
                scaleValue[0] = selection.transform.localScale.x;
                scaleValue[1] = selection.transform.localScale.y;
                scaleValue[2] = selection.transform.localScale.z;
                selection.transformSelection.defaultScaleValue = (float[])scaleValue.Clone();
            }
        }
    }

    private void SetTransformValues(ScriptSelection selection)
    {
        Transform transformS = selection.transform;
        // Position
        float[] positionValue = new float[3];
        positionValue[0] = transformS.position.x;
        positionValue[1] = transformS.position.y;
        positionValue[2] = transformS.position.z;
        
        selection.transformSelection.positionValue = positionValue;
        Debug.Log($"Set Transform position : {positionValue[0]}, {positionValue[1]}, {positionValue[2]}");

        // Rotation
        float[] rotationValue = new float[4];
        rotationValue[0] = transformS.localRotation.w;
        rotationValue[1] = transformS.localRotation.x;
        rotationValue[2] = transformS.localRotation.y;
        rotationValue[3] = transformS.localRotation.z;

        selection.transformSelection.rotationValue = rotationValue;

        // Scale
        float[] scaleValue = new float[3];
        scaleValue[0] = transformS.localScale.x;
        scaleValue[1] = transformS.localScale.y;
        scaleValue[2] = transformS.localScale.z;

        selection.transformSelection.scaleValue = scaleValue;
        //this.scaleSelected = scaleSelected;
    }

    private void SetTransformDefaultValues(ScriptSelection selection)
    {
        TransformSelection transformSelection = selection.transformSelection;

        // Position
        float[] positionValue = new float[3];
        positionValue[0] = transformSelection.defaultPositionValue[0];
        positionValue[1] = transformSelection.defaultPositionValue[1];
        positionValue[2] = transformSelection.defaultPositionValue[2];

        selection.transformSelection.positionValue = positionValue;
        Debug.Log($"Set Transform Default position : {selection.transformSelection.positionValue[0]}, {selection.transformSelection.positionValue[1]}, {selection.transformSelection.positionValue[2]}");

        // Rotation
        float[] rotationValue = new float[4];
        rotationValue[0] = selection.transform.localRotation.w;
        rotationValue[1] = selection.transform.localRotation.x;
        rotationValue[2] = selection.transform.localRotation.y;
        rotationValue[3] = selection.transform.localRotation.z;

        selection.transformSelection.rotationValue = rotationValue;

        // Scale
        float[] scaleValue = new float[3];
        scaleValue[0] = selection.transform.localScale.x;
        scaleValue[1] = selection.transform.localScale.y;
        scaleValue[2] = selection.transform.localScale.z;

        selection.transformSelection.scaleValue = scaleValue;
        //this.scaleSelected = scaleSelected;
    }


    public void NewSave(bool isDefault, string profileName)
    {
        Debug.Log($"SAVE {isDefault} {profileName}");
        CurrentProfile = profileName;
        if (profileName == "" || scriptSelections == null) return;
        Dictionary <string, object> data = new Dictionary<string, object>();
        Dictionary<string, TransformSelection> transforms = new Dictionary<string, TransformSelection>();

        string saveGUID = Guid.NewGuid().ToString();

        DateTime now = DateTime.Now;
        string currentDate = now.ToString("yyyy-MM-dd"); // Format : "2025-01-16"
        //Debug.Log("Date actuelle : " + currentDate);
        string currentTime = now.ToString("HH:mm:ss"); // Format : "16:45:30"
        //Debug.Log("Heure actuelle : " + currentTime);

        int saveNbr = DictProfileNbrSaves[CurrentProfile] + 1;
        //Debug.Log($"SAVE NBR {saveNbr}");

        SaveInfos dataInfos = new SaveInfos(saveGUID, currentDate, currentTime, saveNbr);

        foreach (var selection in scriptSelections)
        {
            if (selection.targetGameObject != null && selection.selectedScript != null && selection.variableSelections.Count > 0)
            {
                data.Add(selection.guid, selection.variableSelections); // GUID - List variables
                //Debug.Log($"add to dict data, guid : {selection.guid}, game object : {selection.targetGameObject}, script : {selection.selectedScript}, list variables :");

                // SAVE VARIABLES 
                foreach (var varSelection in selection.variableSelections) 
                {
                    GameObject gameObject = selection.targetGameObject as GameObject;
                    if (isDefault)
                    {
                        object value = varSelection.defaultValue;
                        varSelection.value = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                        //Debug.Log($"Save Default Variable: {varSelection.variableName}, Value: {value}, Is Selected: {varSelection.isSelected} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");
                    }
                    else
                    {
                        object value = GetFieldValue(selection.selectedScript, varSelection.variableName);
                        varSelection.value = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                        //Debug.Log($"Save Variable: {varSelection.variableName}, Value: {value}, Is Selected: {varSelection.isSelected} | in GameObject: {gameObject.name} & Script : {selection.selectedScript}");
                    }
                    //Debug.Log("default value " + varSelection.defaultValue);
                }

                // SAVE TRANSFORM
                Transform transformS = selection.transform;
                Debug.Log($"Saved transform {transformS}");

                

                if (isDefault)
                {
                    SetTransformDefaultValues(selection);
                }
                else
                {
                    SetTransformValues(selection);
                }

                Debug.Log($"Saved position : {selection.transformSelection.positionValue[0]}, {selection.transformSelection.positionValue[1]}, {selection.transformSelection.positionValue[2]} Save N° {dataInfos.SaveNbr}");

                transforms.Add(selection.guid, selection.transformSelection);
            }
        }

        //Debug.Log($"Save Infos: GUID {dataInfos.GUID}");

        SaveData saveData = new SaveData(data, transforms, dataInfos);

        SaveSystem.NewSave(saveData, profileName);

        OnAddSave.Invoke();
    }

    public void OverrideSave(SaveData lastSave)
    {
        SaveData overrideSave = lastSave;
        Debug.Log($"OVERRIDE SAVE {overrideSave.SaveInfos.GUID} {CurrentProfile}");

        if (CurrentProfile == "" || scriptSelections == null) return;
        Dictionary<string, object> data = new Dictionary<string, object>();
        Dictionary<string, TransformSelection> transforms = new Dictionary<string, TransformSelection>();

        // Keep GUID but new date & time
        string saveGUID = overrideSave.SaveInfos.GUID;

        DateTime now = DateTime.Now;
        string currentDate = now.ToString("yyyy-MM-dd"); // Format : "2025-01-16"
        string currentTime = now.ToString("HH:mm:ss"); // Format : "16:45:30"

        int saveNbr = lastSave.SaveInfos.SaveNbr;
        //Debug.Log($"SAVE NBR {saveNbr}");

        SaveInfos dataInfos = new SaveInfos(saveGUID, currentDate, currentTime, saveNbr);

        foreach (var selection in scriptSelections)
        {
            if (selection.targetGameObject != null && selection.selectedScript != null && selection.variableSelections.Count > 0)
            {
                data.Add(selection.guid, selection.variableSelections); // GUID - List variables
                //Debug.Log($"add to dict data, guid : {selection.guid}, game object : {selection.targetGameObject}, script : {selection.selectedScript}, list variables :");

                // SAVE VARIABLES 
                foreach (var varSelection in selection.variableSelections)
                {
                    GameObject gameObject = selection.targetGameObject as GameObject;
                    object value = GetFieldValue(selection.selectedScript, varSelection.variableName);
                    varSelection.value = (value is ICloneable) ? ((ICloneable)value).Clone() : value;
                }

                // SAVE TRANSFORM
                Transform transformS = selection.transform;
                Debug.Log($"Saved transform {transformS}");

                SetTransformValues(selection);

                Debug.Log($"Saved position : {selection.transformSelection.positionValue[0]}, {selection.transformSelection.positionValue[1]}, {selection.transformSelection.positionValue[2]} Save N° {dataInfos.SaveNbr}");

                transforms.Add(selection.guid, selection.transformSelection);
            }
        }

        //Debug.Log($"Save Infos: GUID {dataInfos.GUID}");

        SaveData saveData = new SaveData(data, transforms, dataInfos);

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
            TransformSelection transformSelection = data.DictTransforms[guid] as TransformSelection;

            SaveManager.ScriptSelection selection = GetScriptSelectionFromGUID(guid);

            if (selection != null)
            {
                GameObject gameObject = selection.targetGameObject as GameObject;
                Component component = selection.selectedScript as Component;

                if (gameObject != null && component != null)
                {
                    // LOAD VARIABLES
                    foreach (var varSelection in variables)
                    {
                        if (varSelection.isSelected)
                        {
                            // Définir la valeur chargée dans la variable du script
                            SetFieldValue(component, varSelection.variableName, varSelection.value);
                            //Debug.Log($"Loaded Variable: {varSelection.variableName}, Set Value: {varSelection.value} in GameObject: {gameObject.name}, Script: {component.ToString()}");
                        }
                    }

                    Transform transformS = selection.transform;
                    if (transformS != null)
                    {
                        if (transformSelection != null)
                        {
                            // Position
                            if (transformSelection.positionSelected)
                            {
                                Debug.Log($"AVANT LOAD POSITION : {transformS.position}");
                                Vector3 position = new Vector3(transformSelection.positionValue[0], transformSelection.positionValue[1], transformSelection.positionValue[2]);
                                transformS.position = position;
                                Debug.Log($"LOAD POSITION : {position} guid {data.SaveInfos.GUID} Save N° {data.SaveInfos.SaveNbr}");
                            }

                            // Rotation
                            if (transformSelection.rotationSelected)
                            {
                                Quaternion rotation = new Quaternion(transformSelection.rotationValue[0], transformSelection.rotationValue[1], transformSelection.rotationValue[2], transformSelection.rotationValue[3]);
                                transformS.rotation = rotation;
                            }

                            // Scale
                            if (transformSelection.scaleSelected)
                            {
                                Vector3 scale = new Vector3(transformSelection.scaleValue[0], transformSelection.scaleValue[1], transformSelection.scaleValue[2]);
                                transformS.localScale = scale;
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
            //Debug.Log("Sous-dossiers dans " + saveFolderPath + ":");

            foreach (string folder in folders)
            {
                //Debug.Log(Path.GetFileName(folder)); // Affiche uniquement le nom du dossier
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
        //Debug.Log($"MAX SAVE NBR {saveNbr}");
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

        //Debug.Log("end profiles saves");
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
            //Debug.Log("Fichiers dans " + saveFolderPath + ":");


            List<SaveData> ProfileSaves = new List<SaveData>();

            /*
            if (files.Length == 0)
            {
                Debug.Log("Create default save");
                NewSave(true, profileName);
            }*/

            foreach (string file in files)
            {
                //Debug.Log(Path.GetFileName(file)); // Affiche uniquement le nom du dossier
                string filePath = Path.GetFullPath(file);

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream stream = new FileStream(filePath, FileMode.Open);

                SaveData data = binaryFormatter.Deserialize(stream) as SaveData;
                //Debug.Log($"found data {data.SaveInfos.GUID} {data.SaveInfos.Time}");
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
