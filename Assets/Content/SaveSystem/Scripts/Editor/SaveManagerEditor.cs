using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    // ----- FIELDS ----- //
    private List<bool> showVariablesFoldout = new List<bool>();
    private List<bool> showTransformFoldout = new List<bool>();

    private GUIStyle removeButtonStyle;
    private GUIStyle titleStyle;
    private GUIStyle boldStyle;
    // ----- FIELDS ----- //

    public override void OnInspectorGUI()
    {
        SetStyles();

        SaveManager saveManager = (SaveManager)target;

        // Check each gameobject -> script -> variables
        for (int i = 0; i < saveManager.SavedEntries.Count; i++)
        {
            if (i == 0)
            {
                DrawLine();
            }
            var scriptSelection = saveManager.SavedEntries[i];

            EditorGUILayout.LabelField($"Save Variables {i + 1}", titleStyle);

            // Get gameobject
            EditorGUI.BeginChangeCheck();
            var newTargetGameObject = (GameObject)EditorGUILayout.ObjectField(
                "Target GameObject",
                (GameObject)scriptSelection.SelectedGameObject,
                typeof(GameObject),
                true
            );

            // On change -> reset script & variables
            if (EditorGUI.EndChangeCheck())
            {
                scriptSelection.SelectedGameObject = newTargetGameObject;
                //scriptSelection.SelectedScript = null;

                //if (scriptSelection.VariableSelections != null)
                    //scriptSelection.VariableSelections.Clear();

                if (scriptSelection.TransformSelection != null)
                    scriptSelection.TransformSelection = null;
            }

            if (scriptSelection.SelectedGameObject != null)
            {
                // Get script from gameobject
                GameObject gameObject = scriptSelection.SelectedGameObject as GameObject;

                // ----- TRANSFORM ----- //
                // Foldout menu transform
                if (showTransformFoldout.Count < i + 1) showTransformFoldout.Add(false);

                showTransformFoldout[i] = EditorGUILayout.Foldout(showTransformFoldout[i], "Save Transform", boldStyle);
                if (showTransformFoldout[i])
                {
                    // -- POSITION -- //
                    GUIStyle positionStyle = new GUIStyle(EditorStyles.label);
                    positionStyle.normal.textColor = scriptSelection.TransformSelection.positionSelected ? Color.white : Color.gray;

                    // Set toggle & style
                    EditorGUILayout.BeginHorizontal();
                    scriptSelection.TransformSelection.positionSelected = EditorGUILayout.Toggle(scriptSelection.TransformSelection.positionSelected, GUILayout.Width(20));
                    EditorGUILayout.LabelField("Position", positionStyle);
                    EditorGUILayout.EndHorizontal();
                    // -- POSITION -- //

                    // -- ROTATION -- //
                    GUIStyle rotationStyle = new GUIStyle(EditorStyles.label);
                    rotationStyle.normal.textColor = scriptSelection.TransformSelection.rotationSelected ? Color.white : Color.gray;

                    // Set toggle & style
                    EditorGUILayout.BeginHorizontal();
                    scriptSelection.TransformSelection.rotationSelected = EditorGUILayout.Toggle(scriptSelection.TransformSelection.rotationSelected, GUILayout.Width(20));
                    EditorGUILayout.LabelField("Rotation", rotationStyle);
                    EditorGUILayout.EndHorizontal();
                    // -- ROTATION -- //

                    // -- SCALE -- //
                    GUIStyle scaleStyle = new GUIStyle(EditorStyles.label);
                    scaleStyle.normal.textColor = scriptSelection.TransformSelection.scaleSelected ? Color.white : Color.gray;

                    // Set toggle & style
                    EditorGUILayout.BeginHorizontal();
                    scriptSelection.TransformSelection.scaleSelected = EditorGUILayout.Toggle(scriptSelection.TransformSelection.scaleSelected, GUILayout.Width(20));
                    EditorGUILayout.LabelField("Scale", scaleStyle);
                    EditorGUILayout.EndHorizontal();
                    // -- SCALE -- //
                }
                // ----- TRANSFORM ----- //

                // Add script
                if (GUILayout.Button("Add Script Variables"))
                {
                    scriptSelection.ScriptDatas.Add(new SaveManager.ScriptData());
                }

                /*
                MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
                string[] scriptNames = scripts.Select(s => s.GetType().Name).ToArray();
                int selectedIndex = ArrayUtility.IndexOf(scripts, scriptSelection.SelectedScript);
                selectedIndex = EditorGUILayout.Popup("Select Script", selectedIndex, scriptNames);

                if (selectedIndex >= 0)
                {
                    scriptSelection.SelectedScript = scripts[selectedIndex];

                    // Check if script not already saved
                    bool isDuplicate = saveManager.ScriptSelections
                        .Where((_, idx) => idx != i) // exclude current script
                        .Any(sel =>
                            sel.SelectedGameObject == scriptSelection.SelectedGameObject &&
                            sel.SelectedScript == scriptSelection.SelectedScript);

                    // Show warning if already used
                    if (isDuplicate)
                    {
                        EditorGUILayout.HelpBox(
                            $"Error: This script is already selected for the same GameObject.",
                            MessageType.Error
                        );
                    }


                    // ----- VARIABLES ----- //
                    // Get script -> variables
                    var fields = scriptSelection.SelectedScript.GetType()
                        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                        .Where(f => !f.IsDefined(typeof(HideInInspector), true))
                        .ToList();

                    // Create selection if doesn't exists
                    if (scriptSelection.VariableSelections == null)
                        scriptSelection.VariableSelections = new List<SaveManager.VariableSelection>();

                    // Check if variable exists in selection
                    foreach (var field in fields)
                    {
                        if (!scriptSelection.VariableSelections.Any(v => v.VariableName == field.Name))
                        {
                            scriptSelection.VariableSelections.Add(new SaveManager.VariableSelection { VariableName = field.Name, IsSaved = false});
                        }
                    }

                    // Delete selection if not variables from script
                    scriptSelection.VariableSelections = scriptSelection.VariableSelections
                        .Where(v => fields.Any(f => f.Name == v.VariableName))
                    .ToList();
                    // ----- VARIABLES ----- //

                    // ----- TRANSFORM ----- //
                    // Get script -> transform
                    Transform transform = scriptSelection.SelectedScript.transform;
                    Debug.Log($"get transform {transform}");
                    scriptSelection.Transform = transform;
                    //scriptSelection.transformSelection = new SaveManager.TransformSelection(transform);
                    //Debug.Log($"ADD TRANSFORM {transform}");
                    // ----- TRANSFORM ----- //

                    // GUID
                    if (scriptSelection.SelectionGUID == "") scriptSelection.SelectionGUID = Guid.NewGuid().ToString();
                    EditorGUILayout.LabelField($"GUID : {scriptSelection.SelectionGUID}");

                    // ----- VARIABLES ----- //
                    // Foldout menu variables
                    if (showVariablesFoldout.Count < i + 1) showVariablesFoldout.Add(false);

                    showVariablesFoldout[i] = EditorGUILayout.Foldout(showVariablesFoldout[i], "Save Variables", boldStyle);
                    if (showVariablesFoldout[i])
                    {
                        foreach (var variable in scriptSelection.VariableSelections)
                        {
                            // Change style if selected or not
                            GUIStyle variableStyle = new GUIStyle(EditorStyles.label);
                            variableStyle.normal.textColor = variable.IsSaved ? Color.white : Color.gray;

                            // Set toggle & style
                            EditorGUILayout.BeginHorizontal();
                            variable.IsSaved = EditorGUILayout.Toggle(variable.IsSaved, GUILayout.Width(20));
                            EditorGUILayout.LabelField(variable.VariableName, variableStyle);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    // ----- VARIABLES ----- //
                    
            }
            else
                {
                    // Error when no script selected
                    EditorGUILayout.HelpBox(
                        $"Error: You need to select a script",
                        MessageType.Error
                    );
                }
                */
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
                saveManager.SavedEntries.RemoveAt(i);
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
            saveManager.SavedEntries.Add(new SaveManager.SavedEntry());
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
        titleStyle = new GUIStyle();
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
