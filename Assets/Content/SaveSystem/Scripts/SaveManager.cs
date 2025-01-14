using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class SaveManager : MonoBehaviour
{
    // ----- VARIABLES ----- //
    [System.Serializable]
    public class VariableSelection
    {
        public string variableName;
        public bool isSelected;
    }
    // ----- VARIABLES ----- //

    // ----- GAME OBJECT, SCRIPT & VARIABLES ----- //
    [System.Serializable]
    public class ScriptSelection
    {
        public GameObject targetGameObject;
        public MonoBehaviour selectedScript;
        public List<VariableSelection> variableSelections;
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
                        object value = GetFieldValue(selection.selectedScript, varSelection.variableName);
                        Debug.Log($"Save Variable: {varSelection.variableName}, Value: {value} | in GameObject: {selection.targetGameObject.name} & Script : {selection.selectedScript.name}");
                    }
                }
            }
        }
    }
}
