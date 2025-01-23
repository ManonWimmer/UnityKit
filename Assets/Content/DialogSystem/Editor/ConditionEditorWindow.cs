using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CREMOT.DialogSystem
{
    public class ConditionEditorWindow : EditorWindow
    {
        private DialogueNode _node;
        private string _portId;

        private List<ConditionSO> _conditions;

        public void Init(DialogueNode node, string portId)
        {
            _node = node;
            _portId = portId;

            var portCondition = _node.PortConditions.FirstOrDefault(cond => cond.portId == portId);
            if (portCondition == null)
            {
                portCondition = new PortCondition { portId = _portId };
                _node.PortConditions.Add(portCondition);
            }
            _conditions = portCondition.conditions;
        }

        private void OnGUI()
        {
            if (_conditions == null) return;

            GUILayout.Label("Conditions for port: " + _portId, EditorStyles.boldLabel);

            for (int i = 0; i < _conditions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                _conditions[i].requiredItem = EditorGUILayout.TextField("Item", _conditions[i].requiredItem);
                _conditions[i].requiredQuantity = EditorGUILayout.IntField("Quantity", _conditions[i].requiredQuantity);

                if (GUILayout.Button("Remove"))
                {
                    _conditions.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Condition"))
            {
                _conditions.Add(new ConditionSO());
            }

            if (GUILayout.Button("Save"))
            {
                Close();
            }
        }
    }
}
