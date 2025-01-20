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

    [SerializeField] private LocalizationDataSO _localizationDataSO;

    #endregion


    #region Delegates

    public event Action<Language> OnLanguageUpdated;
    public event Action OnLanguageUpdatedNoParam;

    public UnityEvent OnLanguageUpdatedUnity;

    #endregion

    #region Properties
    public static LocalizationManager Instance { get => _instance; set => _instance = value; }
    public Language CurrentLanguage { get => _currentLanguage; set => _currentLanguage = value; }

    #endregion

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        _instance = this;
    }

    public void ChangeLanguages(int nextLanguage)
    {
        _currentLanguage = (Language)nextLanguage;

        OnLanguageUpdated?.Invoke(_currentLanguage);
        OnLanguageUpdatedNoParam?.Invoke();
        OnLanguageUpdatedUnity?.Invoke();
    }

    public string GetLocalisedTextWithID(string textId)
    {
        if (_localizationDataSO == null) return "";

        if (!_localizationDataSO.LocalizationData.ContainsKey(textId)) return "";

        string languageKeyId = GetLanguagekeyIdFromEnum(_currentLanguage);

        if (!_localizationDataSO.LocalizationData[textId].ContainsKey(languageKeyId)) return "";

        return _localizationDataSO.LocalizationData[textId][languageKeyId];
    }

    public string GetLanguagekeyIdFromEnum(Language language)
    {
        switch (language)
        {
            case Language.ENGLISH:
                return "EN";
            case Language.FRENCH:
                return "FR";
            case Language.SPANISH:
                return "SP";
            default:
                return "";
        }
    }

}
