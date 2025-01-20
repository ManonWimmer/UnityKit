using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CSVToSOEditor : EditorWindow
{
    private TextAsset csvFile; // Field to select the CSV file
    private string lastSelectedFolder; // Last selected folder

    [MenuItem("Tools/CSV to ScriptableObjects")] // Adds an entry in the Unity menu
    public static void ShowWindow()
    {
        GetWindow<CSVToSOEditor>("CSV to ScriptableObjects");
    }

    private void OnGUI()
    {
        // Title
        GUILayout.Label("CSV to ScriptableObjects", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // CSV file selection
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        GUILayout.Space(10);

        // Button to convert the CSV
        if (GUILayout.Button("Convert to ScriptableObjects"))
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a CSV file.", "OK");
                return;
            }

            // Open a folder selection dialog
            string folderPath = EditorUtility.SaveFilePanelInProject("Save LocalizationData", "NewLocalizationData", "asset", "Save LocalizationData");

            if (!string.IsNullOrEmpty(folderPath))
            {
                lastSelectedFolder = folderPath;

                

                // Generate the ScriptableObjects
                GenerateScriptableObjects(folderPath);
            }
        }

        // Display the last used folder
        if (!string.IsNullOrEmpty(lastSelectedFolder))
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Last used folder:", lastSelectedFolder, EditorStyles.wordWrappedLabel);
        }
    }

    private void GenerateScriptableObjects(string folderPath)
    {
        // Read the lines of the CSV
        string[] lines = csvFile.text.Split('\n');
        if (lines.Length <= 1)
        {
            EditorUtility.DisplayDialog("Error", "The CSV file is empty or incorrectly formatted.", "OK");
            return;
        }

        // Retrieve the headers (first line)
        string[] headers = lines[0].Split(',');

        // Create an instance of the ScriptableObject
        LocalizationDataSO localizationData = ScriptableObject.CreateInstance<LocalizationDataSO>();

        // Loop through each line of the CSV (skipping the header)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Split the line into columns
            string[] columns = line.Split(',');

            if (columns.Length < 3)
            {
                Debug.LogWarning($"Line ignored (insufficient data): {line}");
                continue;
            }

            // The first column contains the language ID
            string languageID = columns[0].Trim();

            // Loop through the remaining columns to get the keys and translations
            for (int j = 1; j < columns.Length; j++)
            {
                string key = headers[j].Trim();         // Main key (from the header)
                string text = columns[j].Trim();       // Translation text

                // Add the data to the ScriptableObject
                localizationData.AddEntry(key, languageID, text);
            }

            // Define the save path
            string assetPath = $"{folderPath}";

            if (AssetDatabase.LoadAssetAtPath<LocalizationDataSO>($"{folderPath}/LocalizationData.asset") != null)
            {
                // Si l'asset existe déjà, on le supprime
                AssetDatabase.DeleteAsset($"{folderPath}/LocalizationData.asset");
            }

            // Save the ScriptableObject
            AssetDatabase.CreateAsset(localizationData, assetPath);
            EditorUtility.SetDirty(localizationData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Confirm success
            //EditorUtility.DisplayDialog("Success", "The ScriptableObject has been successfully generated.", "OK");
        }
    }
}