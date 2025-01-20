using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLocaliserUI : MonoBehaviour
{

    TextMeshProUGUI textField;
    [SerializeField] private string key;

    void OnEnable()
    {
        LocalizationSystem.onLanguageChanged += UpdateText ;
        CSVLoader.csvIsLoad += UpdateText;
    }

    void OnDisable()
    {
        LocalizationSystem.onLanguageChanged -= UpdateText;
        CSVLoader.csvIsLoad -= UpdateText;
    }


    void UpdateText()
    {
        textField = GetComponent<TextMeshProUGUI>();
        textField.text = LocalizationSystem.GetLocalisedValue(key);
    }
}
