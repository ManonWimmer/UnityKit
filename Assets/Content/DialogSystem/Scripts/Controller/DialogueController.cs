using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CREMOT.DialogSystem
{

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

        private DialogueNodeSO _currentDialogueNodeSO;

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

                if (DialogueInventory.Instance != null)
                {
                    DialogueInventory.Instance.OnUpdatedDialogueInventory += RefreshChoicesButton;
                }

                Init();
            }
        }
        private void OnDestroy()
        {
            if (DialogueInventory.Instance != null)
            {
                DialogueInventory.Instance.OnUpdatedDialogueInventory -= RefreshChoicesButton;
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
        private void RefreshChoicesButton()
        {
            if (_currentDialogueNodeSO == null) return;

            NotifyChoiceChange(_currentDialogueNodeSO.outputPortsChoiceId, _currentDialogueNodeSO.outputPorts);
        }
        #endregion

        #region Selection Choice
        public void SelectChoice(int choiceId)
        {
            DialogueNodeSO nextDialogueNode = GetNextDialogueNodeByChoiceId(choiceId);

            if (nextDialogueNode == null) return;

            string nextNodeId = nextDialogueNode.id;
            string nextDialogueId = nextDialogueNode.dialogueId;

            _currentDialogueNodeSO = nextDialogueNode;

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

            ApplyNodeAssociatedFunctions(_currentDialogueNodeSO);
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
                    //Debug.Log("Choice availbable");
                }
                else
                {
                    availableChoices.Add(null);
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
                //Debug.Log("Break at current node null");
                return false;
            }

            PortCondition condition = currentNode.portConditions.FirstOrDefault(cond => cond.portId == choiceTextId);
            if (condition == null)
            {
                //Debug.Log("Pas de condition -> Toujours dispo");
                return true; // Pas de condition = toujours disponible
            }
            // Vérifier si toutes les conditions sont remplies
            foreach (var cond in condition.conditions)
            {
                if (!CheckCondition(cond))
                {
                    //Debug.Log("ne verifie pas les conditions -> pas dispo");
                    return false; // Une condition échoue
                }
            }
            return true; // Toutes les conditions sont remplies
        }

        private bool CheckCondition(ConditionSO condition)
        {
            if (DialogueInventory.Instance == null) return false;

            return DialogueInventory.Instance.HasItem(condition.requiredItem, condition.requiredQuantity);
        }

        #endregion

        #region Apply node associated Functions

        private void ApplyNodeAssociatedFunctions(DialogueNodeSO nodeData)
        {
            if (nodeData == null) return;

            foreach (var callFunData in nodeData.callFunctions)
            {
                if (callFunData == null) continue;

                if (string.IsNullOrEmpty(callFunData.gameObjectId) || string.IsNullOrEmpty(callFunData.methodName)) continue;

                var tempGameObject = EditorUtility.InstanceIDToObject(int.Parse(callFunData.gameObjectId)) as GameObject;

                if (tempGameObject == null) continue;

                // Split the method name to get the component type and method name  "componentName.methodName"
                var methodParts = callFunData.methodName.Split('.');
                if (methodParts.Length != 2) continue;

                var componentName = methodParts[0];
                var methodName = methodParts[1];

                // Get the component
                var component = tempGameObject.GetComponent(componentName);
                if (component == null) continue;

                // Get the method
                var method = component.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null) continue;

                // Invoke the method
                method.Invoke(component, null);
            }
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
}
