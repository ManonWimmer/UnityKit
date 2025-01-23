using CREMOT.DialogSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

namespace CREMOT.DialogSystem
{
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
        [CREMOT.DialogSystem.ReadOnly] public string id;
        [CREMOT.DialogSystem.ReadOnly] public string dialogueId;
        [CREMOT.DialogSystem.ReadOnly] public string title;
        [CREMOT.DialogSystem.ReadOnly] public Vector2 position;
        [CREMOT.DialogSystem.ReadOnly] public bool entryPoint = false;
        [CREMOT.DialogSystem.ReadOnly] public List<string> outputPorts = new List<string>(); // Liste des GUID des ports de sortie

        [CREMOT.DialogSystem.ReadOnly] public List<string> outputPortsChoiceId = new List<string>(); // List des custom ID des ports de sortie


        [CREMOT.DialogSystem.ReadOnly] public List<PortCondition> portConditions = new List<PortCondition>();

        [CREMOT.DialogSystem.ReadOnly] public List<CallFunctionData> callFunctions = new List<CallFunctionData>();
    }

    [System.Serializable]
    public class DialogueEdgeSO
    {
        [CREMOT.DialogSystem.ReadOnly] public string fromNodeId;   // Guid node de départ
        [CREMOT.DialogSystem.ReadOnly] public string fromPortId;   // Guid port de départ

        [CREMOT.DialogSystem.ReadOnly] public string toNodeId;     // Guid node d'arrivée
        [CREMOT.DialogSystem.ReadOnly] public string toPortId;     // Guid port d'arrivée

        [CREMOT.DialogSystem.ReadOnly] public int fromPortIndex;   // position du port de départ (pas utilisé car bug à cause du container visual element)
        [CREMOT.DialogSystem.ReadOnly] public int toPortIndex;     // position du port d'arrivée (utilisé car pas de container sur ce port)
    }

    // --------------- Condition part -------------------
    [System.Serializable]
    public class ConditionSO
    {
        [CREMOT.DialogSystem.ReadOnly] public string requiredItem; // Nom de l'item requis
        [CREMOT.DialogSystem.ReadOnly] public int requiredQuantity; // Quantité requise
        [CREMOT.DialogSystem.ReadOnly] public bool isMet; // État de la condition (calculé à l'exécution)
    }

    [System.Serializable]
    public class PortCondition
    {
        [CREMOT.DialogSystem.ReadOnly] public string portId;
        [CREMOT.DialogSystem.ReadOnly] public List<ConditionSO> conditions = new List<ConditionSO>();
    }
    [System.Serializable]
    public class CallFunctionData
    {
        public string gameObjectPersistantGUID;
        public string methodName;
    }
}
