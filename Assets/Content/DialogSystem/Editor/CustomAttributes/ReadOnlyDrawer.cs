using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CREMOT.DialogSystem
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Garder la possibilité de déplier, mais rendre la modification impossible
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
