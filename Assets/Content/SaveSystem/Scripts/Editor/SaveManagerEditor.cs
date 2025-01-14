using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    private List<bool> showVariablesFoldout = new List<bool>();
    private GUIStyle removeButtonStyle; // Style pour le bouton "Remove"

    private GUIStyle titleStyle; // Style personnalis� pour le label

    private void OnEnable()
    {
        // Initialiser le style du label
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 14;  // Change la taille de la police
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white; // (Optionnel) Change la couleur du texte
    }


    public override void OnInspectorGUI()
    {
      
        SaveManager saveManager = (SaveManager)target;


        // Parcourt chaque s�lection pour afficher les champs
        for (int i = 0; i < saveManager.scriptSelections.Count; i++)
        {
            if (i == 0 )
            {
                DrawLine();
            }
            var scriptSelection = saveManager.scriptSelections[i];

            EditorGUILayout.LabelField($"Save Variables {i + 1}", titleStyle);

            // S�lectionner un GameObject
            EditorGUI.BeginChangeCheck();
            var newTargetGameObject = (GameObject)EditorGUILayout.ObjectField(
                "Target GameObject",
                scriptSelection.targetGameObject,
                typeof(GameObject),
                true
            );

            if (EditorGUI.EndChangeCheck())
            {
                scriptSelection.targetGameObject = newTargetGameObject;
                scriptSelection.selectedScript = null; // R�initialiser le script
                if (scriptSelection.variableSelections != null)
                    scriptSelection.variableSelections.Clear(); // R�initialiser les variables
            }

            if (scriptSelection.targetGameObject != null)
            {
                // S�lectionner un script attach� au GameObject
                MonoBehaviour[] scripts = scriptSelection.targetGameObject.GetComponents<MonoBehaviour>();
                string[] scriptNames = scripts.Select(s => s.GetType().Name).ToArray();
                int selectedIndex = ArrayUtility.IndexOf(scripts, scriptSelection.selectedScript);
                selectedIndex = EditorGUILayout.Popup("Select Script", selectedIndex, scriptNames);

                if (selectedIndex >= 0)
                {
                    scriptSelection.selectedScript = scripts[selectedIndex];

                    // V�rifier si un autre script s�lectionn� est d�j� attach� au m�me GameObject
                    bool isDuplicate = saveManager.scriptSelections
                        .Where((_, idx) => idx != i) // Ignorer la s�lection actuelle
                        .Any(sel =>
                            sel.targetGameObject == scriptSelection.targetGameObject &&
                            sel.selectedScript == scriptSelection.selectedScript);

                    // Afficher un warning si c'est un doublon
                    if (isDuplicate)
                    {
                        EditorGUILayout.HelpBox(
                            $"Error: This script is already selected for the same GameObject.",
                            MessageType.Error
                        );
                    }

                    // Afficher les variables du script s�lectionn�
                    var fields = scriptSelection.selectedScript.GetType()
                        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                        .Where(f => !f.IsDefined(typeof(HideInInspector), true))
                        .ToList();

                    if (scriptSelection.variableSelections == null)
                        scriptSelection.variableSelections = new List<SaveManager.VariableSelection>();

                    // Synchroniser la liste des variables
                    foreach (var field in fields)
                    {
                        if (!scriptSelection.variableSelections.Any(v => v.variableName == field.Name))
                        {
                            scriptSelection.variableSelections.Add(new SaveManager.VariableSelection { variableName = field.Name, isSelected = false });
                        }
                    }

                    // Supprimer les variables obsol�tes
                    scriptSelection.variableSelections = scriptSelection.variableSelections
                        .Where(v => fields.Any(f => f.Name == v.variableName))
                        .ToList();


                    if (showVariablesFoldout.Count < i + 1) showVariablesFoldout.Add(false);

                    showVariablesFoldout[i] = EditorGUILayout.Foldout(showVariablesFoldout[i], "Save Variables");

                    if (showVariablesFoldout[i])
                    {
                        foreach (var variable in scriptSelection.variableSelections)
                        {
                            // Cr�er un toggle pour chaque variable
                            EditorGUILayout.BeginHorizontal();
                            variable.isSelected = EditorGUILayout.Toggle(variable.variableName, variable.isSelected);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"Error: You need to select a script",
                        MessageType.Error
                    );
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                        $"Error: You need to have a game object selected",
                        MessageType.Error
                    );
            }

            EditorGUILayout.Space();

            // Initialiser le style du bouton
            removeButtonStyle = new GUIStyle();
            removeButtonStyle.normal.textColor = Color.red; // Couleur du texte
            removeButtonStyle.fontStyle = FontStyle.Bold; // Texte en gras
            removeButtonStyle.alignment = TextAnchor.MiddleCenter; // Centrer le texte

            // Ajouter un bouton pour ajouter de nouvelles s�lections de script
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

        // Ajouter un bouton pour ajouter de nouvelles s�lections de script
        if (GUILayout.Button("Add Save Variables"))
        {
            saveManager.scriptSelections.Add(new SaveManager.ScriptSelection());
        }

        // Sauvegarder les changements dans l'inspecteur
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
}
