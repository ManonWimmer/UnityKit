using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationDataSO))]
public class LocalizationDataSOEditor : Editor
{
    private SerializedProperty keyEntriesProperty;

    private void OnEnable()
    {
        // Get reference to the serialized property for the key entries list
        keyEntriesProperty = serializedObject.FindProperty("_keyEntries");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Display the title
        GUILayout.Label("Localization Data", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Iterate over the key entries and display them
        for (int i = 0; i < keyEntriesProperty.arraySize; i++)
        {
            SerializedProperty keyEntry = keyEntriesProperty.GetArrayElementAtIndex(i);

            SerializedProperty keyProperty = keyEntry.FindPropertyRelative("key");
            SerializedProperty translationsProperty = keyEntry.FindPropertyRelative("translations");

            EditorGUILayout.PropertyField(keyProperty);

            // Display the translations for each key
            for (int j = 0; j < translationsProperty.arraySize; j++)
            {
                SerializedProperty translationEntry = translationsProperty.GetArrayElementAtIndex(j);

                SerializedProperty languageIDProperty = translationEntry.FindPropertyRelative("languageID");
                SerializedProperty textProperty = translationEntry.FindPropertyRelative("text");

                EditorGUILayout.PropertyField(languageIDProperty);
                EditorGUILayout.PropertyField(textProperty);
            }

            /*
            // Add a button to add more translations
            if (GUILayout.Button("Add Translation"))
            {
                translationsProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            }
            */
            GUILayout.Space(10);
        }

        /*
        // Add a button to add more keys
        if (GUILayout.Button("Add New Key"))
        {
            keyEntriesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();
        }
        */
        serializedObject.ApplyModifiedProperties();
    }
}
