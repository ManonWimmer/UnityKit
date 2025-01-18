using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LocalizationSystem : MonoBehaviour
{
    public static Language language = Language.English;
    private static bool isInit = false;
    private static Dictionary<string, string> localisedEN;
    private static Dictionary<string, string> localisedFR;
    private static Dictionary<string, string> localisedES;

    public enum Language
    {
        English,
        Fran�ais,
        Espa�ol
    }

    private static void Init()
    {
        CSVLoader csvLoader = new CSVLoader();
        localisedEN = csvLoader.GetDictionaryValues("en");
        localisedFR = csvLoader.GetDictionaryValues("fr");
        localisedES = csvLoader.GetDictionaryValues("es");
        isInit = true;
    }

    public static string GetLocalisedValue(string key)
    {
        if (!isInit) { Init(); }
   
        string value = key;
        switch (language)
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
