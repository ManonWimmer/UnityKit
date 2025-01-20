using CREMOT.LocalizationSystem;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLocalizationData", menuName = "Localization/LocalizationData")]
public class LocalizationDataSO : ScriptableObject
{
    [System.Serializable]
    public class TranslationEntry
    {
        [CREMOT.LocalizationSystem.ReadOnly] public string languageID; // Language (e.g., FR, EN, ES)
        [CREMOT.LocalizationSystem.ReadOnly] public string text;       // Text for this language
    }

    [System.Serializable]
    public class KeyEntry
    {
        [CREMOT.LocalizationSystem.ReadOnly] public string key; // Main key (e.g., 1, 2, ...)
        [CREMOT.LocalizationSystem.ReadOnly] public List<TranslationEntry> translations = new List<TranslationEntry>();
    }

    [SerializeField] private List<KeyEntry> _keyEntries = new List<KeyEntry>();

    // Access to the localization data as a dictionary at runtime
    public Dictionary<string, Dictionary<string, string>> LocalizationData
    {
        get
        {
            // Build the dictionary from the list
            Dictionary<string, Dictionary<string, string>> localizationData = new Dictionary<string, Dictionary<string, string>>();

            foreach (var keyEntry in _keyEntries)
            {
                if (!localizationData.ContainsKey(keyEntry.key))
                {
                    localizationData[keyEntry.key] = new Dictionary<string, string>();
                }

                foreach (var translation in keyEntry.translations)
                {
                    if (!localizationData[keyEntry.key].ContainsKey(translation.languageID))
                    {
                        localizationData[keyEntry.key][translation.languageID] = translation.text;
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate translation detected for key '{keyEntry.key}' and language '{translation.languageID}'");
                    }
                }
            }

            return localizationData;
        }
    }

    // Add a translation entry. This can be used for dynamically building the ScriptableObject data.
    public void AddEntry(string key, string languageID, string text)
    {
        // Find the key entry
        var keyEntry = _keyEntries.Find(entry => entry.key == key);
        if (keyEntry == null)
        {
            keyEntry = new KeyEntry { key = key };
            _keyEntries.Add(keyEntry);
        }

        // Find the translation for the specific language
        var translationEntry = keyEntry.translations.Find(t => t.languageID == languageID);
        if (translationEntry == null)
        {
            keyEntry.translations.Add(new TranslationEntry { languageID = languageID, text = text });
        }
        else
        {
            translationEntry.text = text; // Update if it already exists
        }
    }
}