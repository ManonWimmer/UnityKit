using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueGraph", menuName = "Dialogue/Dialogue Graph")]
public class DialogueGraphSO : ScriptableObject
{
    [SerializeField] private List<DialogueNodeSO> nodes = new List<DialogueNodeSO>();
    [SerializeField] private List<DialogueEdgeSO> edges = new List<DialogueEdgeSO>();


    public List<DialogueNodeSO> Nodes { get => nodes; set => nodes = value; }
    public List<DialogueEdgeSO> Edges { get => edges; set => edges = value; }
}

[System.Serializable]
public class DialogueNodeSO
{
    public string id;
    public string dialogueId;
    public string title;
    public Vector2 position;
    public bool entryPoint = false;
    public List<string> outputPorts = new List<string>(); // Liste des GUID des ports de sortie

    public List<string> outputPortsChoiceId = new List<string>(); // List des custom ID des ports de sortie


    public List<PortCondition> portConditions = new List<PortCondition>();
}

[System.Serializable]
public class DialogueEdgeSO
{
    public string fromNodeId;   // Guid node de d�part
    public string fromPortId;   // Guid port de d�part

    public string toNodeId;     // Guid node d'arriv�e
    public string toPortId;     // Guid port d'arriv�e

    public int fromPortIndex;   // position du port de d�part (pas utilis� car bug � cause du container visual element)
    public int toPortIndex;     // position du port d'arriv�e (utilis� car pas de container sur ce port)
}

// --------------- Condition part -------------------
[System.Serializable]
public class ConditionSO
{
    public string requiredItem; // Nom de l'item requis
    public int requiredQuantity; // Quantit� requise
    public bool isMet; // �tat de la condition (calcul� � l'ex�cution)
}

[System.Serializable]
public class PortCondition
{
    public string portId;
    public List<ConditionSO> conditions = new List<ConditionSO>();
}
