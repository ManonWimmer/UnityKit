using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizationInfoBox : MonoBehaviour
{
    #region Fields
    private TextMeshProUGUI _localizationTargetText;

    [SerializeField] private string _localizationId;

    #endregion

    private void Awake()
    {
        _localizationTargetText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageUpdated += DisplayLocalizationText;

            DisplayLocalizationText(LocalizationManager.Instance.CurrentLanguage);
        }
    }

    private void DisplayLocalizationText(LocalizationManager.Language language)
    {
        if (_localizationTargetText == null) return;

        if (LocalizationManager.Instance == null) return;

        _localizationTargetText.text = LocalizationManager.Instance.GetLocalisedTextWithID(_localizationId);
    }
}
