using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace CREMOT.DialogSystem
{
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

        private string _currentSavedDialogueId;
        private List<string> _currentSavedChoicesIds;

        [Space(20)]

        [Header("Display Parameters")]

        [SerializeField] private bool _displayButtonsElements = true;
        [SerializeField] private bool _neverHideDisplayer = false;
        [SerializeField] private bool _startHidden = false;

        [Space(20)]

        [Header("Localization Parameters")]

        [SerializeField] private bool _useDynamicLocalizationDisplay;

        // Reflexion to avoid dependencies with Localization Package
        private MonoBehaviour _localizationManager;
        private MethodInfo _getLocalizedTextMethod;

        #endregion

        private void Awake()
        {
            if (_idToDialogueSO != null)
            {
                var temp = _idToDialogueSO.IdToTextConverter;   // Init dictionnary converter
            }

            if (_idToChoiceSO != null)
            {
                var temp2 = _idToChoiceSO.IdToTextConverter;    // Init dictionnary converter
            }

            if (_useDynamicLocalizationDisplay)
            {
                GetLocalizationObjectsTroughRelfexion();
                SubscribeToLanguageChange();
            }

            if (_localizationManager == null || _getLocalizedTextMethod == null)
            {
                _useDynamicLocalizationDisplay = false;
            }
        }

        #region Reflexion Localization setup (to avoid depencies trough packages)
        private void GetLocalizationObjectsTroughRelfexion()
        {
            _localizationManager = FindObjectsOfType<MonoBehaviour>().FirstOrDefault(m => m.GetType().Name == "LocalizationManager");

            if (_localizationManager != null)
            {
                // Get dynamic method "GetLocalizedTextWithID" in LocalizationManager
                _getLocalizedTextMethod = _localizationManager.GetType().GetMethod("GetLocalisedTextWithID", BindingFlags.Public | BindingFlags.Instance);

                if (_getLocalizedTextMethod == null)
                {
                    //Debug.LogWarning("La méthode GetLocalizedTextWithID n'a pas été trouvée dans LocalizationManager.");
                }
            }
            else
            {
                //Debug.LogWarning("Aucun LocalizationManager trouvé dans la scène.");
            }
        }
        private void SubscribeToLanguageChange()
        {
            if (_localizationManager == null) return;

            // Get event "OnLanguageUpdatedNoParam" in LocalizationManager
            var onLanguageUpdatedEvent = _localizationManager.GetType()
                .GetEvent("OnLanguageUpdatedNoParam", BindingFlags.Public | BindingFlags.Instance);

            if (onLanguageUpdatedEvent != null)
            {
                // Create delegate to RefreshAllText
                var handler = Delegate.CreateDelegate(
                    onLanguageUpdatedEvent.EventHandlerType,
                    this,
                    typeof(DialogueDisplayer).GetMethod("RefreshAllText", BindingFlags.Public | BindingFlags.Instance)
                );

                // Suscribe from event
                onLanguageUpdatedEvent.AddEventHandler(_localizationManager, handler);
                //Debug.Log("Abonné à l'événement OnLanguageUpdatedNoParam.");
            }
            else
            {
                //Debug.LogWarning("L'événement OnLanguageUpdatedNoParam n'a pas été trouvé.");
            }
        }
        private void UnsubscribeFromLanguageChange()
        {
            if (_localizationManager == null) return;

            // Get event "OnLanguageUpdatedNoParam" in LocalizationManager
            var onLanguageUpdatedEvent = _localizationManager.GetType()
                .GetEvent("OnLanguageUpdatedNoParam", BindingFlags.Public | BindingFlags.Instance);

            if (onLanguageUpdatedEvent != null)
            {
                // Create delegate to RefreshAllText
                var handler = Delegate.CreateDelegate(
                    onLanguageUpdatedEvent.EventHandlerType,
                    this,
                    typeof(DialogueDisplayer).GetMethod("RefreshAllText", BindingFlags.Public | BindingFlags.Instance)
                );

                // Unsuscribe from event
                onLanguageUpdatedEvent.RemoveEventHandler(_localizationManager, handler);
                //Debug.Log("Désabonné de l'événement OnLanguageUpdatedNoParam.");
            }
        }

        #endregion

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

            if (_useDynamicLocalizationDisplay)
            {
                UnsubscribeFromLanguageChange();
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

            _currentSavedDialogueId = textId;
        }

        #endregion

        #region Display choices
        private void DisplayChoices(List<string> choicesText)
        {
            if (_choicesContainer == null) return;
            if (_dialogueController == null) return;
            if (!_displayButtonsElements) return;

            ClearAllChildren(_choicesContainer.transform);

            for (int i = 0; i < choicesText.Count; ++i)
            {
                if (choicesText[i] == null) continue;
                AddChoiceButton(_dialogueController, i, choicesText[i]);
            }

            _currentSavedChoicesIds = choicesText;
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

        #region Relfexion Localization methods
        public string GetLocalizedTextTroughReflexion(string textId)
        {
            if (_localizationManager == null || _getLocalizedTextMethod == null)
            {
                Debug.LogWarning("Impossible d'obtenir le texte localisé : aucune méthode disponible.");
                return textId; // Retourne l'ID comme fallback
            }

            // Appeler dynamiquement la méthode "GetLocalizedTextWithID"
            return (string)_getLocalizedTextMethod.Invoke(_localizationManager, new object[] { textId });
        }

        #endregion


        #region Converter Dialogue / Choice     ID -> TEXT
        private string GetDialogueTextFromDialogueId(string idDialogue)
        {
            if (_useDynamicLocalizationDisplay && _localizationManager != null && _getLocalizedTextMethod != null)
            {
                return GetLocalizedTextTroughReflexion(idDialogue);
            }
            else
            {
                if (_idToDialogueSO == null) return "";

                if (_idToDialogueSO.IdToTextConverter.TryGetValue(idDialogue, out var dialogueText))
                {
                    return dialogueText;
                }
            }

            Debug.LogWarning($"Dialogue ID not found: {idDialogue}");
            return "";
        }
        private string GetChoiceTextFromChoiceId(string idChoice)
        {
            if (_useDynamicLocalizationDisplay && _localizationManager != null && _getLocalizedTextMethod != null)
            {
                return GetLocalizedTextTroughReflexion(idChoice);
            }
            else
            {
                if (_idToChoiceSO == null) return "";

                if (_idToChoiceSO.IdToTextConverter.TryGetValue(idChoice, out var choiceText))
                {
                    return choiceText;
                }
            }

            Debug.LogWarning($"Choice ID not found: {idChoice}");
            return "";
        }
        #endregion

        #region Refresh Dialogue / Choices

        public void RefreshAllText()
        {
            DisplayDialogueText(_currentSavedDialogueId);
            DisplayChoices(_currentSavedChoicesIds);
        }

        #endregion

    }
}