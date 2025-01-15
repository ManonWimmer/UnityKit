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

    [SerializeField] private RectTransform _choiceButtonPanel;


    public static DialogueManager Instance { get => _instance; set => _instance = value; }


    public UnityEvent<string> OnNextDialogueUnity;

    public event Action<string> OnNextDialogue;




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
        _currentDialogueId = GetDialogueIdEntryPoint();

        string tempText = GetDialogueFromIdDialogue(_currentDialogueId);

        OnNextDialogue?.Invoke(tempText);
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
        string nextDialogueId = GetNextDialogueIdChoice(choiceId);
        if (!string.IsNullOrEmpty(nextDialogueId))
        {
            _currentDialogueId = nextDialogueId;
        }
        else
        {
            Debug.LogWarning("No valid dialogue found for this choice : " + nextDialogueId);
        }
        
        string tempText = GetDialogueFromIdDialogue(_currentDialogueId);

        OnNextDialogue?.Invoke(tempText);
    }
    private string GetDialogueFromIdDialogue(string idDialogue)
    {
        if (_idToDialogueSO == null) return "";

        return _idToDialogueSO.IdToDialogueConverter[idDialogue];
    }

    #region Parcours Graph Data
    private string GetNextDialogueIdChoice(int choiceId)
    {
        if (_dialogueGraphSO.nodes == null) return "";
        if (_dialogueGraphSO.edges == null) return "";
        if (string.IsNullOrEmpty(_currentDialogueId)) return "";
        if (choiceId < 0) return "";

        // Trouve le nœud actuel
        foreach (DialogueNodeSO node in _dialogueGraphSO.nodes)
        {
            if (node.dialogueId == _currentDialogueId)
            {
                // Récupère l'edge correspondant au choix
                DialogueEdgeSO edge = GetNextNodeByChoiceId(choiceId, node.id);
                if (edge == null) return ""; // Pas d'edge trouvé pour ce choix

                // Trouve le nœud cible
                foreach (DialogueNodeSO targetNode in _dialogueGraphSO.nodes)
                {
                    if (targetNode.id == edge.toNodeId)
                    {
                        return targetNode.dialogueId; // Retourne l'ID du dialogue suivant
                    }
                }
            }
        }

        return ""; // Aucun dialogue trouvé
    }

    private DialogueEdgeSO GetNextNodeByChoiceId(int choiceId, string currentNodeId)
    {
        foreach (DialogueEdgeSO edge in _dialogueGraphSO.edges)
        {
            if (edge.fromNodeId == currentNodeId && edge.fromPortIndex == choiceId)
            {
                return edge;
            }
        }

        return null; // Aucun edge correspondant trouvé
    }
    #endregion
}
