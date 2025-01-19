using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueController : MonoBehaviour
{
    #region Fields
    [Header("Init Parameters")]
    [SerializeField] private bool _autoInit;

    [Space(20)]

    [Header("Associated Dialogue Graph")]
    [SerializeField] private DialogueGraphSO _dialogueGraphSO;

    private string _currentNodeId = "";
    private string _currentDialogueId = "";

    #endregion


    #region Delegates
    [Space(20)]

    [Header("Dialogue / Choices Events")]
    [Space(5)]
    public UnityEvent OnDialogueUpdatedUnity;
    public event Action<string> OnDialogueUpdated;

    [Space(5)]

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
        _currentNodeId = GetNodeIdEntryPoint();
        _currentDialogueId = GetDialogueIdEntryPoint();

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

    #region Selection Choice
    public void SelectChoice(int choiceId)
    {
        DialogueNodeSO nextDialogueNode = GetNextDialogueNodeByChoiceId(choiceId);

        if (nextDialogueNode == null) return;

        string nextNodeId = nextDialogueNode.id;
        string nextDialogueId = nextDialogueNode.dialogueId;


        if (!string.IsNullOrEmpty(nextNodeId))
        {
            if (!string.IsNullOrEmpty(nextDialogueId))
            {
                _currentDialogueId = nextDialogueId;
            }

            _currentNodeId = nextNodeId;
            //Debug.Log($"From ID {_currentDialogueId} to ID {nextDialogueId}");
        }
        else
        {
            Debug.LogWarning("No valid dialogue found for this choice : " + nextDialogueId);
        }

        NotifyDialogueChange(_currentDialogueId);

        NotifyChoiceChange(nextDialogueNode.outputPortsChoiceId);
    }

    #endregion

    #region Notify Dialogue / Choices changes
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

    #endregion

    #region Parcours Graph Data
    private DialogueNodeSO GetNextDialogueNodeByChoiceId(int choiceId)
    {
        if (_dialogueGraphSO.Nodes == null) return null;
        if (_dialogueGraphSO.Edges == null) return null;
        if (string.IsNullOrEmpty(_currentNodeId)) return null;
        if (choiceId < 0) return null;



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
                //Debug.Log($"Found edge: FromNodeId={edge.fromNodeId}, ToNodeId={edge.toNodeId}, ChoiceId={choiceId}");
                return edge;
            }
        }

        Debug.LogWarning($"No edge found for CurrentNodeId={currentNodeId}, ChoiceId={choiceId}");
        return null;
    }
    #endregion
}
