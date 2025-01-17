using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationSystem : MonoBehaviour
{
    public enum Language
    {
        English,
        Fran�ais,
        Espa�ol
    }

    public static Language language = Language.English;

    private Dictionary<string, string> localisedEN;
    private Dictionary<string, string> localisedFR;
    private Dictionary<string, string> localisedES;

    public static string GetLocalisedValue(string key)
    {
        switch (language)
        {
            case Language.English:
                break;
            case Language.Fran�ais:
                break;
            case Language.Espa�ol:
                break;
        }
        return "not finish";
    }


}
