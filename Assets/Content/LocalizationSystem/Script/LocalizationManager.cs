using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LocalizationManager : MonoBehaviour
{

    #region Fields
    private static LocalizationManager _instance;

    private Language _currentLanguage = Language.ENGLISH;

    [System.Serializable]
    public enum Language
    {
        ENGLISH = 0,
        FRENCH = 1,
        SPANISH = 2
    }

    #endregion


    #region Delegates

    public event Action<Language> OnLanguageUpdated;

    public UnityEvent OnLanguageUpdatedUnity;

    #endregion

    #region Properties
    public static LocalizationManager Instance { get => _instance; set => _instance = value; }

    #endregion

    

    public void ChangeLanguages(int nextLanguage)
    {
        _currentLanguage = (Language)nextLanguage;

        OnLanguageUpdated?.Invoke(_currentLanguage);
        OnLanguageUpdatedUnity?.Invoke();
    }

}
