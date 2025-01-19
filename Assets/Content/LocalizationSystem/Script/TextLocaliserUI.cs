using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLocaliserUI : MonoBehaviour
{

    TextMeshProUGUI textField;
    [SerializeField] private string key;

    public CSVLoader csvLoader;

    void OnEnable()
    {
        csvLoader.csvIsLoad += SetTextUI;
    }

    void OnDisable()
    {
        csvLoader.csvIsLoad -= SetTextUI;
    }


    void SetTextUI()
    {
        textField = GetComponent<TextMeshProUGUI>();
        string value = LocalizationSystem.GetLocalisedValue(key);
        textField.text = value;
    }
}
