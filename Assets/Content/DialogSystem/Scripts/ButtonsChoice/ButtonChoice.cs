using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonChoice : MonoBehaviour
{
    #region Fields
    private int _associatedChoiceIndex;
    [SerializeField] private TextMeshProUGUI _choiceText;

    #endregion


    public void Init(int id, string textValue = "Default Choice")   // Init Button data
    {
        _associatedChoiceIndex = id;    // init id to indicate wich path to take

        SetButtonLabel(textValue);
    }
    public void TriggerSelectChoiceButton()     // send id button to indicate wich path to take -> trough UnityEvent
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
