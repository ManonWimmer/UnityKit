using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class ToolsLocalization : EditorWindow
{
    [SerializeField]private List<Text> textList = new List<Text>();
    [MenuItem("Utilities/Localization/Text Finder")]
    public static void ShowWindow()
    {
        // Affiche la fenêtre de l'éditeur
        EditorWindow.GetWindow(typeof(ToolsLocalization));
    }

    private void OnGUI()
    {
        GUILayout.Label("Text Finder Tool", EditorStyles.boldLabel);
        if (GUILayout.Button("Find All Texts in Scene"))
        {
            FindAllTexts();
        }
        if (textList.Count > 0)
        {
            GUILayout.Label("Found UI Text Components:");
            foreach (var text in textList)
            {
                // Afficher le nom de l'objet contenant le Text
                GUILayout.Label($"GameObject: {text.gameObject.name}, Text: {text.text}");
            }
        }
        else
        {
            GUILayout.Label("No UI Text found.");
        }
    }

    private void FindAllTexts()
    {
        textList.Clear();
        Text[] texts = GameObject.FindObjectsOfType<Text>();
        foreach (Text text in texts)
        {
            textList.Add(text);
        }
        Repaint();
    }
}
