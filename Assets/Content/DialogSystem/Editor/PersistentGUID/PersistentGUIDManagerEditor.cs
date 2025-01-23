using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CREMOT.DialogSystem
{
    [CustomEditor(typeof(PersistentGUIDManager))]
    public class PersistentGUIDManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PersistentGUIDManager manager = (PersistentGUIDManager)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Assign GUID to All Objects"))
            {
                var allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    manager.AssignGUID(obj);
                }

                Debug.Log("Assigned GUID to all objects in the scene.");

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            }

            if (GUILayout.Button("Remove All GUIDs"))
            {
                var allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    manager.RemoveGUID(obj);
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

                Debug.Log("Removed all GUID from objects in the scene.");
            }

            if (GUILayout.Button("Refresh Dictionaries"))
            {
                manager.RefreshDictionaries();

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            }
        }
    }
}
