using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LocalizationSystem : MonoBehaviour
{
    public static Language language = Language.English;
    private bool isInit = false;
    [SerializeField]GetCSV getCSV;

    private Dictionary<string, string> localisedEN;
    private Dictionary<string, string> localisedFR;
    private Dictionary<string, string> localisedES;

    public enum Language
    {
        English,
        Fran�ais,
        Espa�ol
    }

    private void Init()
    {
        getCSV = new GetCSV();
        localisedEN = getCSV.GetDictionaryValues("en");
        localisedFR = getCSV.GetDictionaryValues("fr");
        localisedES = getCSV.GetDictionaryValues("es");
        isInit = true;
    }

    public string GetLocalisedValue(string key)
    {
        if (isInit)
        {
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
        else
        {
            Init();
        }
        return null;
    }
}
