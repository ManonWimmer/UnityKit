using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LocalizationSystem : MonoBehaviour
{
    public delegate void OnLanguageChanged();
    public static event OnLanguageChanged onLanguageChanged;
    private Language language = Language.Fran�ais;

    private static bool isInit = false;
    private static Dictionary<string, string> localisedEN;
    private static Dictionary<string, string> localisedFR;
    private static Dictionary<string, string> localisedES;

    Dictionary<string, Dictionary<Enum, string>> dictionaryDialogue;

    public static LocalizationSystem Instance;

    [System.Serializable]
    public enum Language
    {
        Fran�ais = 1,
        English = 2,
        Espa�ol = 3
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Init()
    {
        localisedEN = CSVLoader.GetDictionaryValues(Language.English);
        localisedFR = CSVLoader.GetDictionaryValues(Language.Fran�ais);
        localisedES = CSVLoader.GetDictionaryValues(Language.Espa�ol);
        isInit = true;
    }

    public void ChangeLanguage(Int32 indexLanguage)
    {
        language = (Language)indexLanguage + 1;
        onLanguageChanged?.Invoke();
    }

    public static string GetLocalisedValue(string key)
    {
        if (!isInit) { Init();}
   
        string value = key;
        switch (LocalizationSystem.Instance.language)
        {
            case Language.English:
                localisedEN.TryGetValue(key, out value);
                        break;
            case Language.Fran�ais:
                localisedFR.TryGetValue(key, out value);
                break;
            case Language.Espa�ol:
                localisedES.TryGetValue(key, out value);
                break;
        }
        return value;
    }
}
