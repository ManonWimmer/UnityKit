using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLocaliserUI : MonoBehaviour
{

    TextMeshProUGUI textField;
    [SerializeField] private string key;

    void Start()
    {
        textField = GetComponent<TextMeshProUGUI>();
        string value  = LocalizationSystem.GetLocalisedValue(key);
        textField.text = value; 
    }
}
