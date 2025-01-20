using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LocalizationDataSO;

[CreateAssetMenu(fileName = "NewLocalizationData", menuName = "Localization/LocalizationData")]
public class LocalizationDataSO : ScriptableObject
{
    [System.Serializable]
    public class TranslationEntry
    {
        public string languageID; // Language (e.g., FR, EN, ES)
        public string text;       // Text for this language
    }

    [System.Serializable]
    public class KeyEntry
    {
        public string key; // Main key (e.g., 1, 2, ...)
        public List<TranslationEntry> translations = new List<TranslationEntry>();
    }

    //[SerializeField] private List<KeyEntry> _keyEntries = new List<KeyEntry>();

    [SerializeField] private Dictionary<string, Dictionary<string, string>> _localizationData = new Dictionary<string, Dictionary<string, string>>();

    /// <summary>
    /// Access the localization data as a dictionary at runtime.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> LocalizationData
    {
        get
        {
            /*
            if (_localizationData == null)
            {
                _localizationData = new Dictionary<string, Dictionary<string, string>>();

                foreach (var keyEntry in _keyEntries)
                {
                    if (!_localizationData.ContainsKey(keyEntry.key))
                    {
                        _localizationData[keyEntry.key] = new Dictionary<string, string>();
                    }

                    foreach (var translation in keyEntry.translations)
                    {
                        if (!_localizationData[keyEntry.key].ContainsKey(translation.languageID))
                        {
                            _localizationData[keyEntry.key][translation.languageID] = translation.text;
                        }
                        else
                        {
                            Debug.LogWarning($"Duplicate translation detected for key '{keyEntry.key}' and language '{translation.languageID}'");
                        }
                    }
                }
            }
            */

            return _localizationData;
        }
    }

    /// <summary>
    /// Add a translation entry. This can be used for dynamically building the ScriptableObject data.
    /// </summary>
    public void AddEntry(string key, string languageID, string text)
    {
        /*
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

        // Clear the runtime dictionary cache to rebuild it
        _localizationData = null;
        var temp = LocalizationData;
        */

        if (!_localizationData.ContainsKey(key))
        {
            _localizationData[key] = new Dictionary<string, string>();
        }

        
        if (!_localizationData[key].ContainsKey(languageID))
        {
            _localizationData[key][languageID] = text;
        }
        else
        {
            Debug.LogWarning($"Duplicate translation detected for key '{key}' and language '{languageID}'");
        }
    }
}