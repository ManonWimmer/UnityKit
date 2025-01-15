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
        public object targetGameObject;
        public object selectedScript;
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
                        varSelection.value = value;
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
                foreach (var varSelection in selection.variableSelections) { Debug.Log($"Variable : {varSelection.variableName}, Is Selected : {varSelection.isSelected}"); }
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
                Type scriptType = Type.GetType(component.name);

                // Log pour vérifier l'état du scriptType
                Debug.Log($"Processing GUID: {guid}");

                if (scriptType == null)
                {
                    Debug.LogError($"Script type is null for GUID: {guid}");
                }

                if (gameObject != null && scriptType != null)
                {
                    Component gameObjectScript = gameObject.GetComponent(scriptType);
                    if (gameObjectScript != null)
                    {
                        foreach (var varSelection in selection.variableSelections)
                        {
                            if (varSelection.isSelected)
                            {
                                object value = GetFieldValue(gameObjectScript, varSelection.variableName);
                                Debug.Log($"Loaded Variable: {varSelection.variableName}, Value: {value} in GameObject: {gameObject.name}, Script: {component.ToString()}");
                                varSelection.value = value;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"Script of type {scriptType.Name} not found on {gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Problemo: Invalid game object or script type for GUID {guid}");
                }
            }
            else
            {
                Debug.LogWarning($"Script selection with GUID {guid} not found.");
            }
        }
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
