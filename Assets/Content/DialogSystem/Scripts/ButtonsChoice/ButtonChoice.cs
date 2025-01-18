using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonChoice : MonoBehaviour
{
    private int _associatedChoiceIndex;
    [SerializeField] private TextMeshProUGUI _choiceText;

    public void Init(int id, string textValue = "Default Choice")
    {
        _associatedChoiceIndex = id;

        SetButtonLabel(textValue);
    }
    public void TriggerSelectChoiceButton()
    {
        if (DialogueManager.Instance == null) return;

        DialogueManager.Instance.SelectChoice(_associatedChoiceIndex);
    }
    public void SetButtonLabel(string text)
    {
        if (_choiceText == null) return;

        _choiceText.text = text;
    }
}
