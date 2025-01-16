using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;

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
    

    public void Save()
    {
        Dictionary <string, object> data = new Dictionary<string, object>();

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

        SaveSystem.Save(data);
    }

    public void Load()
    {
        SaveData data = SaveSystem.Load();

        foreach (var entry in data.data)
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







    /*
    foreach (var selection in data.scriptSelections)
    {
        if (selection.targetGameObject != null && selection.selectedScript != null && selection.variableSelections.Count > 0)
        {
            GameObject gameObject = selection.targetGameObject as GameObject;
            MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            { 
                MonoBehaviour scriptObject = selection.selectedScript as MonoBehaviour;
                if (script != null && scriptObject == script)
                {
                    foreach(var varSelection in selection.variableSelections) 
                    { 
                        if (varSelection.isSelected)
                        {
                            Debug.Log(varSelection.variableName);
                        }
                    }
                    break;
                }
            }
        }
    }
    */


    private ScriptSelection GetScriptSelectionFromGUID(string guid)
    {
        foreach(var selection in scriptSelections) {
            if (guid == selection.guid)
                return selection;
        }

        return null;
    }
}
