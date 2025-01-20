using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationDynamicDialogue : MonoBehaviour
{
    public event Action OnNotifyChangeLanguage;


    private void Start()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageUpdated += NotifyChangeLanguage;
        }
    }
    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageUpdated -= NotifyChangeLanguage;
        }
    }

    public string GetDynamicLocalizedDialogueTextFromId(string textId)
    {
        if (LocalizationManager.Instance == null) return "";

        return LocalizationManager.Instance.GetLocalisedTextWithID(textId);
    }

    private void NotifyChangeLanguage(LocalizationManager.Language Language)
    {
        OnNotifyChangeLanguage?.Invoke();
    }
}
