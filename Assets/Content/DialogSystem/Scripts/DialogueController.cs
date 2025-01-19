using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        NotifyChoiceChange(nextDialogueNode.outputPortsChoiceId, nextDialogueNode.outputPorts);
    }

    #endregion

    #region Notify Dialogue / Choices changes
    private void NotifyDialogueChange(string dialogueText)
    {
        OnDialogueUpdated?.Invoke(dialogueText);
        OnDialogueUpdatedUnity?.Invoke();
    }
    private void NotifyChoiceChange(List<string> choicesText, List<string> outputPortGuid)
    {
        var availableChoices = new List<string>();

        //foreach (var choiceText in choicesText)
        for (int i = 0; i < choicesText.Count; ++i)
        {
            if (i >= outputPortGuid.Count) continue;

            var choiceText = choicesText[i];
            var choiceGuid = outputPortGuid[i];

            if (IsChoiceAvailable(choiceGuid))
            {
                availableChoices.Add(choiceText);
                Debug.Log("Choice availbable");
            }
        }

        OnChoiceUpdated?.Invoke(availableChoices);
        //OnChoiceUpdated?.Invoke(choicesText);
        OnChoiceUpdatedUnity?.Invoke();
    }

    #endregion

    #region Condition Check
    private bool IsChoiceAvailable(string choiceTextId)
    {
        DialogueNodeSO currentNode = _dialogueGraphSO.Nodes.FirstOrDefault(node => node.id == _currentNodeId);
        if (currentNode == null)
        {
            Debug.Log("Break at current node null");
            return false;
        }

        PortCondition condition = currentNode.portConditions.FirstOrDefault(cond => cond.portId == choiceTextId);
        if (condition == null)
        {
            Debug.Log("Pas de condition -> Toujours dispo");
            return true; // Pas de condition = toujours disponible
        }
        // Vérifier si toutes les conditions sont remplies
        foreach (var cond in condition.conditions)
        {
            if (!CheckCondition(cond))
            {
                Debug.Log("ne verifie pas les conditions -> pas dispo");
                return false; // Une condition échoue
            }
        }
        return true; // Toutes les conditions sont remplies
    }

    private bool CheckCondition(ConditionSO condition)
    {
        // Exemple : Comparer avec l'inventaire (à implémenter selon votre système d'inventaire)
        //var playerInventory = FindObjectOfType<PlayerInventory>();
        //return playerInventory.HasItem(condition.requiredItem, condition.requiredQuantity);

        if (DialogueInventory.Instance == null) return false;

        return DialogueInventory.Instance.HasItem(condition.requiredItem, condition.requiredQuantity);
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
