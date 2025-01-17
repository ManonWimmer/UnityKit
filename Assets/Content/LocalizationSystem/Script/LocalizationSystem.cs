using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationSystem : MonoBehaviour
{
    public enum Language
    {
        English,
        Français,
        Español
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
            case Language.Français:
                break;
            case Language.Español:
                break;
        }
        return "not finish";
    }


}
