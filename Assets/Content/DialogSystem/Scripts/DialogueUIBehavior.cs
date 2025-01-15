using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUIBehavior : MonoBehaviour
{


    [SerializeField] private RectTransform _choicesButtonPanel;
    [SerializeField] private GameObject _buttonChoicePrefab;

    [SerializeField] private TextMeshProUGUI _dialogueText;


    private void Start()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnNextDialogue += DisplayText;
        }
    }
    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnNextDialogue -= DisplayText;
        }
    }

    public void DisplayChoicesButton(int numberOfChoices)
    {
        
    }

    public void DisplayText(string text)
    {
        _dialogueText.text = text;
    }

}
