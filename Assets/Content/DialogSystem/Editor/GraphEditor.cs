using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CREMOT.DialogSystem
{
    [CustomEditor(typeof(DialogueGraphSO))]
    public class GraphEditor : Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            Object @object = EditorUtility.InstanceIDToObject(instanceId);
            if (@object.GetType() == typeof(DialogueGraphSO))
            {
                DialogueGraph.OpenDialogueGraphWindow(@object);

                return true;
            }

            return false;
        }
    }
}
