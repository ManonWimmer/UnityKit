using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;

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
                //outputPorts = new List<string>(OutputPorts),
                outputPorts = outputContainer.Query<Port>().ToList().Select(port => port.name).ToList(),
                //outputPortsChoiceId = new List<string>(OutputPortsChoiceId),
                outputPortsChoiceId = outputContainer.Query<Port>().ToList().Select(port => port.portName).ToList(),

                portConditions = PortConditions
            };
        }
    }
}
