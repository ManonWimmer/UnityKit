using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.Events;
using System.Reflection;

namespace CREMOT.DialogSystem
{
    public class DialogueNode : Node
    {
        private readonly Vector2 _defaultNodeSize = new Vector2(150, 200);


        public string DialogueText; // Match DialogueId -> DialogueNodeSO
        public string GIUD; // Match id -> DialogueNodeSO

        public bool EntryPoint = false;

        public List<string> OutputPorts = new List<string>();
        public List<string> OutputPortsChoiceId = new List<string>();

        public List<PortCondition> PortConditions = new List<PortCondition>();


        public List<NodeCallFunctionContainer> nodeEventsContainers = new List<NodeCallFunctionContainer>();


        // Synchronize with DialogueNodeSO
        public void InitializeFromSO(DialogueNodeSO nodeSO)
        {
            GIUD = nodeSO.id;
            DialogueText = nodeSO.dialogueId;
            this.SetPosition(new Rect(nodeSO.position, _defaultNodeSize));
            EntryPoint = nodeSO.entryPoint;
            OutputPorts = new List<string>(nodeSO.outputPorts);
            OutputPortsChoiceId = new List<string>(nodeSO.outputPortsChoiceId);

            PortConditions = new List<PortCondition>(nodeSO.portConditions);
        }

        public DialogueNodeSO ToSO()
        {
            return new DialogueNodeSO
            {
                id = GIUD,
                dialogueId = DialogueText,
                title = title,
                position = GetPosition().position,
                entryPoint = EntryPoint,
                outputPorts = outputContainer.Query<Port>().ToList().Select(port => port.name).ToList(),
                outputPortsChoiceId = outputContainer.Query<Port>().ToList().Select(port => port.portName).ToList(),

                portConditions = PortConditions,

                callFunctions = nodeEventsContainers.Select(container => new CallFunctionData
                {
                    gameObjectId = container.CallFunctionField.value != null ? container.CallFunctionField.value.GetInstanceID().ToString() : "",

                    methodName = container.MethodPopupField.value
                }).ToList()
            };
        }
    }

    public class NodeCallFunctionContainer : VisualElement
    {
        #region Fields
        public ObjectField CallFunctionField;

        public PopupField<string> MethodPopupField;
        public List<string> methodNames = new List<string>();

        #endregion



        #region Construct
        public NodeCallFunctionContainer(DialogueNode node)
        {
            CallFunctionField = new ObjectField("Event Field")
            {

            };
            CallFunctionField.RegisterValueChangedCallback(evt =>
            {
                UpdateMethodPopup(evt.newValue as GameObject);
            });

            CallFunctionField.objectType = typeof(GameObject);

            MethodPopupField = new PopupField<string>("Selected Method", methodNames, 0);


            node.mainContainer.Add(CallFunctionField);
            node.mainContainer.Add(MethodPopupField);
        }
        #endregion

        #region Popup
        private void UpdateMethodPopup(GameObject selectedObject)
        {
            methodNames.Clear();

            if (selectedObject != null)
            {
                var components = selectedObject.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    var methods = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                    foreach (var method in methods)
                    {
                        methodNames.Add($"{component.GetType().Name}.{method.Name}");
                    }
                }
            }
            MethodPopupField.choices = methodNames;
            MethodPopupField.index = 0;
        }
        #endregion
    }
}
