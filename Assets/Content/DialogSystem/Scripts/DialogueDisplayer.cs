using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueDisplayer : MonoBehaviour
{
    #region Fields
    [Header("Associated Controller")]
    [SerializeField] private DialogueController _dialogueController;

    [Space(20)]

    [Header("Converter")]
    [SerializeField] private IdToDialogueSO _idToDialogueSO;
    [SerializeField] private IdToDialogueSO _idToChoiceSO;

    [Space(20)]

    [Header("Display Elements")]

    [SerializeField] private Canvas _displayerCanvas;

    [SerializeField] private TextMeshProUGUI _dialogueText;

    [Space(20)]

    [SerializeField] private GameObject _choicesContainer;
    [SerializeField] private GameObject _buttonChoicePrefab;

    [Space(20)]

    [Header("Display Parameters")]

    [SerializeField] private bool _displayButtonsElements = true;
    [SerializeField] private bool _neverHideDisplayer = false;
    [SerializeField] private bool _startHidden = false;

    #endregion

    private void Awake()
    {
        var temp = _idToDialogueSO.IdToTextConverter;   // Init dictionnary converter
        var temp2 = _idToChoiceSO.IdToTextConverter;    // Init dictionnary converter
    }

    private void Start()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueUpdated += DisplayDialogueText;

            _dialogueController.OnChoiceUpdated += DisplayChoices;
        }

        if (_startHidden)
        {
            HideDisplayer();
        }
    }
    private void OnDestroy()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueUpdated -= DisplayDialogueText;

            _dialogueController.OnChoiceUpdated -= DisplayChoices;
        }
    }

    #region Show / hide Displayer

    public void ShowDisplayer()
    {
        if (_displayerCanvas == null) return;

        _displayerCanvas.gameObject.SetActive(true);
    }
    public void HideDisplayer()
    {
        if (_neverHideDisplayer) return;
        if (_displayerCanvas == null) return;

        _displayerCanvas.gameObject.SetActive(false);
    }

    #endregion

    #region Display Dialogue
    private void DisplayDialogueText(string textId)
    {
        if (_dialogueText == null)   return;

        string text = GetDialogueTextFromDialogueId(textId);

        _dialogueText.text = text;
    }

    #endregion

    #region Display choices
    private void DisplayChoices(List<string> choicesText)
    {
        if (_choicesContainer == null) return;
        if (_dialogueController == null) return;

        ClearAllChildren(_choicesContainer.transform);

        for (int i = 0; i < choicesText.Count; ++i)
        {
            if (choicesText[i] == null) continue;
            AddChoiceButton(_dialogueController, i, choicesText[i]);
        }
    }
    private void ClearAllChildren(Transform parent)
    {
        GameObject[] allChildren = new GameObject[parent.childCount];
        int tempIndex = 0;

        foreach (Transform child in parent)
        {
            allChildren[tempIndex] = child.gameObject;
            ++tempIndex;
        }
        foreach (GameObject child in allChildren)
        {
            Destroy(child.gameObject);
        }
    }
    private void AddChoiceButton(DialogueController dialogueController, int id, string idText = "Default Choice")
    {
        GameObject buttonChoiceGameObject = Instantiate(_buttonChoicePrefab, _choicesContainer.transform);

        ChoiceButton buttonChoice = buttonChoiceGameObject.GetComponent<ChoiceButton>();

        string textValue = GetChoiceTextFromChoiceId(idText);

        buttonChoice.Init(dialogueController, id, textValue);
    }
    #endregion

    #region Converter Dialogue / Choice     ID -> TEXT
    private string GetDialogueTextFromDialogueId(string idDialogue)
    {
        if (_idToDialogueSO == null) return "";
        if (_idToDialogueSO.IdToTextConverter.TryGetValue(idDialogue, out var dialogueText))
        {
            return dialogueText;
        }

        Debug.LogWarning($"Dialogue ID not found: {idDialogue}");
        return "";
    }
    private string GetChoiceTextFromChoiceId(string idChoice)
    {
        if (_idToChoiceSO == null) return "";
        if (_idToChoiceSO.IdToTextConverter.TryGetValue(idChoice, out var choiceText))
        {
            return choiceText;
        }

        Debug.LogWarning($"Choice ID not found: {idChoice}");
        return "";
    }
    #endregion

}
