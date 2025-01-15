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

        if (_choiceText != null)
        {
            _choiceText.text = textValue;
        }
    }
    public void TriggerSelectChoiceButton()
    {
        if (DialogueManager.Instance == null) return;

        DialogueManager.Instance.SelectChoice(_associatedChoiceIndex);
    }
}
