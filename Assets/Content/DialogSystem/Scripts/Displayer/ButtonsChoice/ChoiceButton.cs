using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChoiceButton : MonoBehaviour
{
    #region Fields
    private DialogueController _dialogueController;

    private int _associatedChoiceIndex;

    [SerializeField] private TextMeshProUGUI _choiceText;
    #endregion


    public void Init(DialogueController dialogueController, int id, string textValue = "Default Choice")   // Init Button data
    {
        _dialogueController = dialogueController;

        _associatedChoiceIndex = id;    // init id to indicate wich path to take

        SetButtonLabel(textValue);
    }
    public void SetButtonLabel(string text)
    {
        if (_choiceText == null) return;

        _choiceText.text = text;
    }

    public void TriggerSelectChoiceButton()     // send id button to indicate wich path to take -> trough UnityEvent
    {
        if (_dialogueController == null) return;

        _dialogueController.SelectChoice(_associatedChoiceIndex);
    }
}
