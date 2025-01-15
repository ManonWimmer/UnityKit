using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUIBehavior : MonoBehaviour
{


    [SerializeField] private RectTransform _choicesButtonPanel;
    [SerializeField] private GameObject _buttonChoicePrefab;

    [SerializeField] private TextMeshProUGUI _dialogueText;

    [SerializeField] private bool _forceInitDialogueManager;


    private void Start()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnChoiceChange += DisplayChoicesButton;

            DialogueManager.Instance.OnNextDialogue += DisplayText;
            DisplayText(DialogueManager.Instance.CurrentDialogueText);


            if (_forceInitDialogueManager)
            {
                DialogueManager.Instance.Init();
            }
        }

    }
    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnChoiceChange -= DisplayChoicesButton;

            DialogueManager.Instance.OnNextDialogue -= DisplayText;
        }
    }

    public void DisplayChoicesButton(DialogueNodeSO dialogueNode)
    {
        if (_choicesButtonPanel == null) return;

        GameObject[] allChildren = new GameObject[_choicesButtonPanel.childCount];
        int tempIndex = 0;

        foreach (Transform child in _choicesButtonPanel.transform)
        {
            allChildren[tempIndex] = child.gameObject;
            ++tempIndex;
        }
        foreach (GameObject child in allChildren)
        {
            Destroy(child.gameObject);
        }


        for (int i = 0; i < dialogueNode.outputPorts.Count; ++i)
        {
            CreateButtonChoice(i, dialogueNode.outputPorts[i]);
        }
    }
    private void CreateButtonChoice (int Id, string textValue = "Default Choice")
    {
        GameObject buttonChoiceGameObject = Instantiate(_buttonChoicePrefab, _choicesButtonPanel);

        ButtonChoice buttonChoice = buttonChoiceGameObject.GetComponent<ButtonChoice>();

        buttonChoice.Init(Id, textValue);
    }

    public void DisplayText(string text)
    {
        _dialogueText.text = text;
    }

}
