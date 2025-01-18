using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static SaveManager;

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
            var savedEntry = saveManager.SavedEntries[i];

            EditorGUILayout.LabelField($"Saved Entry {i + 1}", titleStyle);

            // Get gameobject
            EditorGUI.BeginChangeCheck();
            var newTargetGameObject = (GameObject)EditorGUILayout.ObjectField(
                "Target GameObject",
                (GameObject)savedEntry.SelectedGameObject,
                typeof(GameObject),
                true
            );

            // On change -> reset script & variables
            if (EditorGUI.EndChangeCheck())
            {
                savedEntry.SelectedGameObject = newTargetGameObject;
                //scriptSelection.SelectedScript = null;

                //if (scriptSelection.VariableSelections != null)
                //scriptSelection.VariableSelections.Clear();

                if (savedEntry.TransformSelection != null)
                    savedEntry.TransformSelection = null;
            }

            if (savedEntry.SelectedGameObject != null)
            {
                // Get script from gameobject
                GameObject gameObject = savedEntry.SelectedGameObject as GameObject;

                #region Transfrom Fouldout
                Transform transform = gameObject.transform;
                Debug.Log($"get transform {transform}");
                savedEntry.Transform = transform;

                // Foldout menu transform
                if (showTransformFoldout.Count < i + 1) showTransformFoldout.Add(false);

                showTransformFoldout[i] = EditorGUILayout.Foldout(showTransformFoldout[i], "Save Transform", boldStyle);
                if (showTransformFoldout[i])
                {
                    // -- POSITION -- //
                    GUIStyle positionStyle = new GUIStyle(EditorStyles.label);
                    positionStyle.normal.textColor = savedEntry.TransformSelection.positionSelected ? Color.white : Color.gray;

                    // Set toggle & style
                    EditorGUILayout.BeginHorizontal();
                    savedEntry.TransformSelection.positionSelected = EditorGUILayout.Toggle(savedEntry.TransformSelection.positionSelected, GUILayout.Width(20));
                    EditorGUILayout.LabelField("Position", positionStyle);
                    EditorGUILayout.EndHorizontal();
                    // -- POSITION -- //

                    // -- ROTATION -- //
                    GUIStyle rotationStyle = new GUIStyle(EditorStyles.label);
                    rotationStyle.normal.textColor = savedEntry.TransformSelection.rotationSelected ? Color.white : Color.gray;

                    // Set toggle & style
                    EditorGUILayout.BeginHorizontal();
                    savedEntry.TransformSelection.rotationSelected = EditorGUILayout.Toggle(savedEntry.TransformSelection.rotationSelected, GUILayout.Width(20));
                    EditorGUILayout.LabelField("Rotation", rotationStyle);
                    EditorGUILayout.EndHorizontal();
                    // -- ROTATION -- //

                    // -- SCALE -- //
                    GUIStyle scaleStyle = new GUIStyle(EditorStyles.label);
                    scaleStyle.normal.textColor = savedEntry.TransformSelection.scaleSelected ? Color.white : Color.gray;

                    // Set toggle & style
                    EditorGUILayout.BeginHorizontal();
                    savedEntry.TransformSelection.scaleSelected = EditorGUILayout.Toggle(savedEntry.TransformSelection.scaleSelected, GUILayout.Width(20));
                    EditorGUILayout.LabelField("Scale", scaleStyle);
                    EditorGUILayout.EndHorizontal();
                    // -- SCALE -- //
                }
                #endregion

                // GUID
                if (savedEntry.SelectionGUID == "") savedEntry.SelectionGUID = Guid.NewGuid().ToString();
                EditorGUILayout.LabelField($"GUID : {savedEntry.SelectionGUID}");

                if (savedEntry.ScriptDatas != null && savedEntry.ScriptDatas.Count > 0)
                {
                    foreach (var scriptData in savedEntry.ScriptDatas)
                    {
                        // Définir un style pour la boîte de fond
                        GUIStyle boxStyle = new GUIStyle();
                        boxStyle.normal.background = Texture2D.whiteTexture;  // Fond de la boîte
                        boxStyle.padding = new RectOffset(100, 100, 100, 100); // Ajoute de l'espace autour des éléments

                        // Créer la boîte avec un fond
                        Rect rect = EditorGUILayout.BeginVertical();
                        GUI.Box(rect, GUIContent.none, boxStyle);



                        MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
                        string[] scriptNames = scripts.Select(s => s.GetType().Name).ToArray();
                        int selectedIndex = ArrayUtility.IndexOf(scripts, scriptData.SelectedScript);
                        selectedIndex = EditorGUILayout.Popup("Select Script", selectedIndex, scriptNames);

                        if (selectedIndex >= 0)
                        {
                            scriptData.SelectedScript = scripts[selectedIndex];

                            // Duplication check
                            bool isDuplicate = false;
                            foreach (var script in savedEntry.ScriptDatas)
                            {
                                if (script != scriptData && script.SelectedScript == scriptData.SelectedScript)
                                    isDuplicate = true;
                            }

                            if (isDuplicate)
                            {
                                EditorGUILayout.HelpBox(
                                    $"Error: This script is already selected for the same GameObject.",
                                    MessageType.Error
                                );
                            }


                            // ----- VARIABLES ----- //
                            // Get script -> variables
                            var fields = scriptData.SelectedScript.GetType()
                                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                                .Where(f => !f.IsDefined(typeof(HideInInspector), true))
                                .ToList();

                            // Create selection if doesn't exists
                            if (scriptData.VariableSelections == null)
                                scriptData.VariableSelections = new List<SaveManager.VariableSelection>();

                            // Check if variable exists in selection
                            foreach (var field in fields)
                            {
                                if (!scriptData.VariableSelections.Any(v => v.VariableName == field.Name))
                                {
                                    scriptData.VariableSelections.Add(new SaveManager.VariableSelection { VariableName = field.Name, IsSaved = false });
                                }
                            }

                            // Delete selection if not variables from script
                            scriptData.VariableSelections = scriptData.VariableSelections
                                .Where(v => fields.Any(f => f.Name == v.VariableName))
                            .ToList();
                            // ----- VARIABLES ----- //

                            // ----- VARIABLES ----- //
                            // Foldout menu variables
                            if (showVariablesFoldout.Count < i + 1) showVariablesFoldout.Add(false);

                            showVariablesFoldout[i] = EditorGUILayout.Foldout(showVariablesFoldout[i], "Save Variables", boldStyle);
                            if (showVariablesFoldout[i])
                            {
                                foreach (var variable in scriptData.VariableSelections)
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
                        }
                        else
                        {
                            // Error when no script selected
                            EditorGUILayout.HelpBox(
                                $"Error: You need to select a script",
                                MessageType.Error
                            );
                        }

                        // Remove script
                        if (GUILayout.Button("Remove Script Variables", removeButtonStyle))
                        {
                            Debug.Log("Remove script variables");
                            savedEntry.ScriptDatas.RemoveAt(savedEntry.ScriptDatas.IndexOf(scriptData));
                            GUI.changed = true;
                            break;
                        }




                        EditorGUILayout.EndVertical();
                        // ----- VARIABLES ----- //
                    }
                }

                // Add script
                if (GUILayout.Button("Add Script Variables"))
                {
                    if (savedEntry.ScriptDatas == null)
                    {
                        savedEntry.ScriptDatas = new List<SaveManager.ScriptData>();
                    }

                    // Ajout d'une nouvelle ScriptData
                    savedEntry.ScriptDatas.Add(new SaveManager.ScriptData());

                    // Assurer que VariableSelections est également initialisé si nécessaire
                    foreach (var scriptData in savedEntry.ScriptDatas)
                    {
                        if (scriptData.VariableSelections == null)
                        {
                            scriptData.VariableSelections = new List<SaveManager.VariableSelection>();
                        }
                    }
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
                saveManager.SavedEntries.RemoveAt(i);
                GUI.changed = true;
                break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawLine();
        }

        // Add save
        if (GUILayout.Button("Add Saved Entry"))
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
