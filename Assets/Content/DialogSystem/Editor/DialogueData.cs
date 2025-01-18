using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueData
{
    #region Fields
    [SerializeField] private string _dialogueText;

    [SerializeField] private List<string> _choices;

    #endregion


    #region Properties
    public string DialogueText { get => _dialogueText; set => _dialogueText = value; }
    public List<string> Choices { get => _choices; set => _choices = value; }


    #endregion
}
