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
            DisplayText(DialogueManager.Instance.CurrentDialogueText);
        }
    }
    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnNextDialogue -= DisplayText;
        }
    }

    public void DisplayChoicesButton(DialogueNodeSO dialogueNode)
    {
        if (_choicesButtonPanel == null) return;

        foreach (Transform child in _choicesButtonPanel.transform)
        {
            Destroy(child);
        }

        for (int i = 0; i < dialogueNode.outputPorts.Count; ++i)
        {
            CreateButtonChoice(i);
        }
    }
    private void CreateButtonChoice (int Id)
    {
        GameObject buttonChoiceGameObject = Instantiate(_buttonChoicePrefab, _choicesButtonPanel);

        ButtonChoice buttonChoice = buttonChoiceGameObject.GetComponent<ButtonChoice>();

        buttonChoice.Init(Id);
    }

    public void DisplayText(string text)
    {
        _dialogueText.text = text;
    }

}
