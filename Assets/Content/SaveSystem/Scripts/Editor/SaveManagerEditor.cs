using CodiceApp;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    // ----- FIELDS ----- //
    private List<bool> showVariablesFoldout = new List<bool>();

    private GUIStyle removeButtonStyle; 
    private GUIStyle titleStyle;
    private GUIStyle boldStyle;
    // ----- FIELDS ----- //

    public override void OnInspectorGUI()
    {
        SetStyles();

        SaveManager saveManager = (SaveManager)target;

        // Check each gameobject -> script -> variables
        for (int i = 0; i < saveManager.scriptSelections.Count; i++)
        {
            if (i == 0 )
            {
                DrawLine();
            }
            var scriptSelection = saveManager.scriptSelections[i];

            EditorGUILayout.LabelField($"Save Variables {i + 1}", titleStyle);

            // Get gameobject
            EditorGUI.BeginChangeCheck();
            var newTargetGameObject = (GameObject)EditorGUILayout.ObjectField(
                "Target GameObject",
                scriptSelection.targetGameObject,
                typeof(GameObject),
                true
            );

            // On change -> reset script & variables
            if (EditorGUI.EndChangeCheck())
            {
                scriptSelection.targetGameObject = newTargetGameObject;
                scriptSelection.selectedScript = null; 
                if (scriptSelection.variableSelections != null)
                    scriptSelection.variableSelections.Clear();
            }

            if (scriptSelection.targetGameObject != null)
            {
                // Get script from gameobject
                MonoBehaviour[] scripts = scriptSelection.targetGameObject.GetComponents<MonoBehaviour>();
                string[] scriptNames = scripts.Select(s => s.GetType().Name).ToArray();
                int selectedIndex = ArrayUtility.IndexOf(scripts, scriptSelection.selectedScript);
                selectedIndex = EditorGUILayout.Popup("Select Script", selectedIndex, scriptNames);

                if (selectedIndex >= 0)
                {
                    scriptSelection.selectedScript = scripts[selectedIndex];

                    // Check if script not already saved
                    bool isDuplicate = saveManager.scriptSelections
                        .Where((_, idx) => idx != i) // exclude current script
                        .Any(sel =>
                            sel.targetGameObject == scriptSelection.targetGameObject &&
                            sel.selectedScript == scriptSelection.selectedScript);

                    // Show warning if already used
                    if (isDuplicate)
                    {
                        EditorGUILayout.HelpBox(
                            $"Error: This script is already selected for the same GameObject.",
                            MessageType.Error
                        );
                    }

                    // Get script -> variables
                    var fields = scriptSelection.selectedScript.GetType()
                        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                        .Where(f => !f.IsDefined(typeof(HideInInspector), true))
                        .ToList();

                    // Create selection if doesn't exists
                    if (scriptSelection.variableSelections == null)
                        scriptSelection.variableSelections = new List<SaveManager.VariableSelection>();

                    // Check if variable exists in selection
                    foreach (var field in fields)
                    {
                        if (!scriptSelection.variableSelections.Any(v => v.variableName == field.Name))
                        {
                            scriptSelection.variableSelections.Add(new SaveManager.VariableSelection { variableName = field.Name, isSelected = false });
                        }
                    }

                    // Delete selection if not variables from script
                    scriptSelection.variableSelections = scriptSelection.variableSelections
                        .Where(v => fields.Any(f => f.Name == v.variableName))
                        .ToList();

                    // Foldout menu
                    if (showVariablesFoldout.Count < i + 1) showVariablesFoldout.Add(false);

                    showVariablesFoldout[i] = EditorGUILayout.Foldout(showVariablesFoldout[i], "Save Variables", boldStyle);
                    if (showVariablesFoldout[i])
                    {
                        foreach (var variable in scriptSelection.variableSelections)
                        {
                            // Change style if selected or not
                            GUIStyle variableStyle = new GUIStyle(EditorStyles.label);
                            variableStyle.normal.textColor = variable.isSelected ? Color.white : Color.gray;

                            // Set toggle & style
                            EditorGUILayout.BeginHorizontal();
                            variable.isSelected = EditorGUILayout.Toggle(variable.isSelected, GUILayout.Width(20));
                            EditorGUILayout.LabelField(variable.variableName, variableStyle);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    // Error when no script selected
                    EditorGUILayout.HelpBox(
                        $"Error: You need to select a script",
                        MessageType.Error
                    );
                }
            }
            else
            {
                // Error when no gameobject selected
                EditorGUILayout.HelpBox(
                        $"Error: You need to have a game object selected",
                        MessageType.Error
                );
            }

            EditorGUILayout.Space();

            // Remove saves
            if (GUILayout.Button("Remove Save Variables", removeButtonStyle))
            {
                Debug.Log("Remove save variables");
                saveManager.scriptSelections.RemoveAt(i);
                GUI.changed = true;
                break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawLine();
        }

        // Add save
        if (GUILayout.Button("Add Save Variables"))
        {
            saveManager.scriptSelections.Add(new SaveManager.ScriptSelection());
        }

        // Save changes in GUI
        if (GUI.changed)
        {
            EditorUtility.SetDirty(saveManager);
        }
    }

    private void DrawLine()
    {
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.cyan;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    private void SetStyles()
    {
        // Remove button
        removeButtonStyle = new GUIStyle();
        removeButtonStyle.normal.textColor = Color.red;
        removeButtonStyle.fontStyle = FontStyle.Bold;
        removeButtonStyle.alignment = TextAnchor.MiddleCenter;

        // Title
        titleStyle = new GUIStyle(EditorStyles.foldout);
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        // Bold
        boldStyle = new GUIStyle(EditorStyles.foldout);
        boldStyle.fontStyle = FontStyle.Bold;
        boldStyle.normal.textColor = Color.cyan;
        boldStyle.onNormal.textColor = Color.cyan;
        boldStyle.hover.textColor = Color.cyan;
        boldStyle.onHover.textColor = Color.cyan;
    }
}
