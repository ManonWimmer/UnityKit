using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueController : MonoBehaviour
{
    #region Fields
    [SerializeField] private bool _autoInit;

    [SerializeField] private DialogueGraphSO _dialogueGraphSO;
    //[SerializeField] private IdToDialogueSO _idToDialogueSO;
    //[SerializeField] private IdToDialogueSO _idToDialogueChoicesSO;

    private string _currentNodeId = "";
    private string _currentDialogueText = "";
    private string _currentDialogueId = "";

    #endregion



    #region Properties
    public string CurrentDialogueText { get => _currentDialogueText; set => _currentDialogueText = value; }
    //public IdToDialogueSO IdToDialogueChoicesSO { get => _idToDialogueChoicesSO; set => _idToDialogueChoicesSO = value; }

    #endregion


    #region Delegates
    public UnityEvent OnDialogueUpdatedUnity;
    public event Action<string> OnDialogueUpdated;

    public UnityEvent OnChoiceUpdatedUnity;
    public event Action<List<string>> OnChoiceUpdated;

    #endregion

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
        //var temp = _idToDialogueSO.IdToDialogueConverter;   // Init dictionnary converter

        _currentNodeId = GetNodeIdEntryPoint();
        _currentDialogueId = GetDialogueIdEntryPoint();

        //_currentDialogueText = GetDialogueFromIdDialogue(_currentDialogueId);

        SelectChoice(0);
    }

    private string GetNodeIdEntryPoint()
    {
        if (_dialogueGraphSO == null) return "";
        if (_dialogueGraphSO.Nodes == null) return "";

        foreach (DialogueNodeSO node in _dialogueGraphSO.Nodes)
        {
            if (node == null) continue;

            if (node.entryPoint)
            {
                return node.id;
            }
        }

        return "";
    }
    private string GetDialogueIdEntryPoint()
    {
        if (_dialogueGraphSO == null) return "";
        if (_dialogueGraphSO.Nodes == null) return "";

        foreach (DialogueNodeSO node in _dialogueGraphSO.Nodes)
        {
            if (node == null) continue;

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

        Debug.Log("Test0");
        if (nextDialogueNode == null) return;

        string nextNodeId = nextDialogueNode.id;
        string nextDialogueId = nextDialogueNode.dialogueId;


        Debug.Log("Test1");

        if (!string.IsNullOrEmpty(nextNodeId))
        {
            if (!string.IsNullOrEmpty(nextDialogueId))
            {
                _currentDialogueId = nextDialogueId;
            }

            _currentNodeId = nextNodeId;
            Debug.Log($"From ID {_currentDialogueId} to ID {nextDialogueId}");
        }
        else
        {
            Debug.LogWarning("No valid dialogue found for this choice : " + nextDialogueId);
        }

        //_currentDialogueText = GetDialogueFromIdDialogue(_currentDialogueId);

        NotifyDialogueChange(_currentDialogueId);

        NotifyChoiceChange(nextDialogueNode.outputPortsChoiceId);
    }
    private void NotifyDialogueChange(string dialogueText)
    {
        OnDialogueUpdated?.Invoke(dialogueText);
        OnDialogueUpdatedUnity?.Invoke();
    }
    private void NotifyChoiceChange(List<string> choicesText)
    {
        OnChoiceUpdated?.Invoke(choicesText);
        OnChoiceUpdatedUnity?.Invoke();
    }

    /*
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
    */

    #region Parcours Graph Data
    private DialogueNodeSO GetNextDialogueNodeByChoiceId(int choiceId)
    {
        if (_dialogueGraphSO.Nodes == null) return null;
        if (_dialogueGraphSO.Edges == null) return null;
        if (string.IsNullOrEmpty(_currentNodeId)) return null;
        if (choiceId < 0) return null;


        Debug.Log("Test2");

        // Trouve le nœud actuel
        foreach (DialogueNodeSO node in _dialogueGraphSO.Nodes)
        {
            if (node.id == _currentNodeId)
            {
                // Récupère l'edge correspondant au choix
                DialogueEdgeSO edge = GetNextEdgeByChoiceId(choiceId, node.id);
                if (edge == null) return null; // Pas d'edge trouvé pour ce choix

                // Trouve le nœud cible
                foreach (DialogueNodeSO targetNode in _dialogueGraphSO.Nodes)
                {
                    Debug.Log($"TEST : {targetNode.id}, {edge.toNodeId}");
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
        foreach (DialogueEdgeSO edge in _dialogueGraphSO.Edges)
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
