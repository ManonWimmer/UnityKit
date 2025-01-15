using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;

    [SerializeField] private bool _autoInit;

    [SerializeField] private DialogueGraphSO _dialogueGraphSO;
    [SerializeField] private IdToDialogueSO _idToDialogueSO;

    private string _currentDialogueId = "";
    private string _currentDialogueText = "";

    [SerializeField] private RectTransform _choiceButtonPanel;


    public static DialogueManager Instance { get => _instance; set => _instance = value; }
    public string CurrentDialogueText { get => _currentDialogueText; set => _currentDialogueText = value; }

    public UnityEvent OnNextDialogueUnity;
    public event Action<string> OnNextDialogue;

    public UnityEvent OnChoiceChangeUnity;
    public event Action<DialogueNodeSO> OnChoiceChange;


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
        }
        _instance = this;
    }
    private void Start()
    {
        if (_autoInit)
        {
            Init();
        }

    }

    #region Init
    public void Init()
    {
        var temp = _idToDialogueSO.IdToDialogueConverter;

        _currentDialogueId = GetDialogueIdEntryPoint();

        string tempText = GetDialogueFromIdDialogue(_currentDialogueId);
        _currentDialogueText = tempText;

        SelectChoice(0);
    }

    private string GetDialogueIdEntryPoint()
    {
        if (_dialogueGraphSO == null) return "";
        if (_dialogueGraphSO.nodes == null) return "";

        foreach (DialogueNodeSO node in _dialogueGraphSO.nodes)
        {
            if (node ==  null) continue;

            if (node.entryPoint)
            {
                return node.dialogueId;
            }
        }

        return "";
    }
    #endregion

    public void SelectChoice(int choiceId)
    {
        DialogueNodeSO nextDialogueNode = GetNextDialogueNodeByChoiceId(choiceId);

        if (nextDialogueNode == null)   return;

        string nextDialogueId = nextDialogueNode.dialogueId;

        if (!string.IsNullOrEmpty(nextDialogueId))
        {
            Debug.Log($"From ID {_currentDialogueId} to ID {nextDialogueId}");
            _currentDialogueId = nextDialogueId;
        }
        else
        {
            Debug.LogWarning("No valid dialogue found for this choice : " + nextDialogueId);
        }
        
        string tempText = GetDialogueFromIdDialogue(_currentDialogueId);

        _currentDialogueText = tempText;

        NotifyDialogueChange(tempText);

        NotifyChoiceChange(nextDialogueNode);
    }
    private void NotifyDialogueChange(string dialogueText)
    {
        OnNextDialogue?.Invoke(dialogueText);
        OnNextDialogueUnity?.Invoke();
    }
    private void NotifyChoiceChange(DialogueNodeSO dialogueNode)
    {
        OnChoiceChange?.Invoke(dialogueNode);
        OnChoiceChangeUnity?.Invoke();
    }

    private string GetDialogueFromIdDialogue(string idDialogue)
    {
        if (_idToDialogueSO == null) return "";
        if (_idToDialogueSO.IdToDialogueConverter.TryGetValue(idDialogue, out var dialogueText))
        {
            return dialogueText;
        }

        Debug.LogWarning($"Dialogue ID not found: {idDialogue}");
        return "";
    }

    #region Parcours Graph Data
    private DialogueNodeSO GetNextDialogueNodeByChoiceId(int choiceId)
    {
        if (_dialogueGraphSO.nodes == null) return null;
        if (_dialogueGraphSO.edges == null) return null;
        if (string.IsNullOrEmpty(_currentDialogueId)) return null;
        if (choiceId < 0) return null;

        // Trouve le nœud actuel
        foreach (DialogueNodeSO node in _dialogueGraphSO.nodes)
        {
            if (node.dialogueId == _currentDialogueId)
            {
                // Récupère l'edge correspondant au choix
                DialogueEdgeSO edge = GetNextEdgeByChoiceId(choiceId, node.id);
                if (edge == null) return null; // Pas d'edge trouvé pour ce choix

                // Trouve le nœud cible
                foreach (DialogueNodeSO targetNode in _dialogueGraphSO.nodes)
                {
                    if (targetNode.id == edge.toNodeId)
                    {
                        return targetNode; // Retourne l'ID du dialogue suivant
                    }
                }
            }
        }

        return null; // Aucun dialogue trouvé
    }

    private DialogueEdgeSO GetNextEdgeByChoiceId(int choiceId, string currentNodeId)    // Retourne l'edge qui fait le lien vers le node cible
    {
        foreach (DialogueEdgeSO edge in _dialogueGraphSO.edges)
        {
            if (edge.fromNodeId == currentNodeId && edge.fromPortIndex == choiceId) // Si depuis bon node && bon choix
            {
                Debug.Log($"Found edge: FromNodeId={edge.fromNodeId}, ToNodeId={edge.toNodeId}, ChoiceId={choiceId}");
                return edge;
            }
        }

        Debug.LogWarning($"No edge found for CurrentNodeId={currentNodeId}, ChoiceId={choiceId}");
        return null;
    }
    #endregion
}
