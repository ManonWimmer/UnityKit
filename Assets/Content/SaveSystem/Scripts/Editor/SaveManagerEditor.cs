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
    private GUIStyle boldBlueStyle;
    private GUIStyle boxStyle;
    private GUIStyle addScriptButton;
    private GUIStyle addEntryButton;
    private Color lightBlueColor;
    private Color redColor;
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

                if (savedEntry.Transform != null)
                    savedEntry.Transform = null;

                if (savedEntry.TransformSelection != null)
                    savedEntry.TransformSelection = null;
            }

            bool isGameObjectDuplicate = false;
            foreach (var checkSavedEntry in saveManager.SavedEntries)
            {
                if (checkSavedEntry != savedEntry && checkSavedEntry.SelectedGameObject == savedEntry.SelectedGameObject)
                    isGameObjectDuplicate = true;
            }

            if (isGameObjectDuplicate)
            {
                EditorGUILayout.HelpBox(
                    $"Error: This GameObject is already selected in another Saved Entry.",
                    MessageType.Error
                );
            }

            if (savedEntry.SelectedGameObject != null)
            {
                // Get script from gameobject
                GameObject gameObject = savedEntry.SelectedGameObject as GameObject;

                #region Transfrom Fouldout

                // Get Tarnsform
                if (savedEntry.Transform == null)
                {
                    Transform transform = gameObject.transform;
                    Debug.Log($"get transform {transform}");
                    savedEntry.Transform = transform;
                }
                    
                // Foldout menu transform
                if (showTransformFoldout.Count < i + 1) showTransformFoldout.Add(false);

                showTransformFoldout[i] = EditorGUILayout.Foldout(showTransformFoldout[i], "Save Transform", boldBlueStyle);
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
                        GUILayout.BeginVertical(boxStyle);
                        {
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

                                showVariablesFoldout[i] = EditorGUILayout.Foldout(showVariablesFoldout[i], "Save Variables", boldBlueStyle);
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
                        }
                        GUILayout.EndVertical();
                        // ----- VARIABLES ----- //
                    }
                }

                // Add script
                if (GUILayout.Button("Add Script Variables", addScriptButton))
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
        if (GUILayout.Button("Add Saved Entry", addEntryButton))
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
        Handles.color = lightBlueColor;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    private void SetStyles()
    {
        lightBlueColor = new Color(0.6681648f, 0.8293654f, 0.990566f, 1f);
        redColor = new Color(0.671f, 0.016f, 0.016f, 1f);

        // Remove button
        removeButtonStyle = new GUIStyle();
        removeButtonStyle.normal.textColor = redColor;
        removeButtonStyle.fontStyle = FontStyle.Bold;
        removeButtonStyle.alignment = TextAnchor.MiddleCenter;

        // Title
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        // Bold
        boldBlueStyle = new GUIStyle(EditorStyles.foldout);
        boldBlueStyle.fontStyle = FontStyle.Bold;
        boldBlueStyle.normal.textColor = lightBlueColor;
        boldBlueStyle.onNormal.textColor = lightBlueColor;
        boldBlueStyle.hover.textColor = lightBlueColor;
        boldBlueStyle.onHover.textColor = lightBlueColor;

        // Script Box
        Texture2D backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
        backgroundTexture.Apply();

        boxStyle = new GUIStyle();
        boxStyle.normal.background = backgroundTexture;
        boxStyle.border = new RectOffset(10, 10, 10, 10);
        boxStyle.margin = new RectOffset(0, 0, 10, 10);     
        boxStyle.padding = new RectOffset(20, 20, 10, 10);
 
        // Add Script Button
        addScriptButton = new GUIStyle(GUI.skin.button);
        addScriptButton.normal.textColor = lightBlueColor;
        addScriptButton.normal.background = backgroundTexture;
        addScriptButton.fontStyle = FontStyle.Bold;

        // Add Entry Button
        addEntryButton = new GUIStyle(GUI.skin.button);
        addEntryButton.normal.textColor = lightBlueColor;
        addEntryButton.normal.background = backgroundTexture;
        addEntryButton.fontStyle = FontStyle.Bold;
        
    }


}


