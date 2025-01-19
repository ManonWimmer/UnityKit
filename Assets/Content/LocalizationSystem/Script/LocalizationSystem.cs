using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LocalizationSystem : MonoBehaviour
{
    public static Language language = Language.Español;
    private static bool isInit = false;
    private static Dictionary<string, string> localisedEN;
    private static Dictionary<string, string> localisedFR;
    private static Dictionary<string, string> localisedES;
    public CSVLoader csvLoader;

    public enum Language
    {
        Français = 1,
        English = 2,
        Español = 3
    }

    public static void Init()
    {
        localisedEN = CSVLoader.GetDictionaryValues(Language.English);
        localisedFR = CSVLoader.GetDictionaryValues(Language.Français);
        localisedES = CSVLoader.GetDictionaryValues(Language.Español);
        isInit = true;
    }

    public static string GetLocalisedValue(string key)
    {
        if (!isInit) { Init();}
   
        string value = key;
        switch (language)
        {
            case Language.English:
                localisedEN.TryGetValue(key, out value);
                        break;
            case Language.Français:
                localisedFR.TryGetValue(key, out value);
                break;
            case Language.Español:
                localisedES.TryGetValue(key, out value);
                break;
        }
        return value;
    }
}
