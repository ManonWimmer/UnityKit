using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CSVToSOEditor : EditorWindow
{
    private TextAsset csvFile; // Field to select the CSV file
    private LocalizationDataSO localizationDataSO; // Field to select the ScriptableObject
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

        // Select the existing ScriptableObject to fill
        localizationDataSO = (LocalizationDataSO)EditorGUILayout.ObjectField("LocalizationDataSO", localizationDataSO, typeof(LocalizationDataSO), false);
        GUILayout.Space(10);

        // Button to convert the CSV
        if (GUILayout.Button("Convert to ScriptableObjects"))
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a CSV file.", "OK");
                return;
            }

            if (localizationDataSO == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a LocalizationDataSO to fill.", "OK");
                return;
            }

            // Generate the ScriptableObjects
            FillLocalizationData(localizationDataSO);
        }

        // Display the last used folder
        if (!string.IsNullOrEmpty(lastSelectedFolder))
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Last used folder:", lastSelectedFolder, EditorStyles.wordWrappedLabel);
        }
    }

    private void FillLocalizationData(LocalizationDataSO localizationDataSO)
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
                localizationDataSO.AddEntry(key, languageID, text);
            }
        }

        // Save the ScriptableObject after filling it
        EditorUtility.SetDirty(localizationDataSO); // Mark the asset as dirty to save changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Confirm success
        EditorUtility.DisplayDialog("Success", "The ScriptableObject has been successfully updated.", "OK");
    }
}