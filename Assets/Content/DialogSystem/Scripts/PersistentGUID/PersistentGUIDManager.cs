using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CREMOT.DialogSystem
{
    public class PersistentGUIDManager : MonoBehaviour
    {
        #region Fields
        private static PersistentGUIDManager _instance;

        // Dictionnaires pour stocker les correspondances
        private Dictionary<string, GameObject> _guidToObject = new Dictionary<string, GameObject>();
        private Dictionary<GameObject, string> _objectToGuid = new Dictionary<GameObject, string>();


        #endregion

        #region Properties
        public static PersistentGUIDManager Instance { get => _instance; set => _instance = value; }



        #endregion


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(gameObject);

            RefreshDictionaries();
        }

        public void RefreshDictionaries()
        {
            _guidToObject.Clear();
            _objectToGuid.Clear();

            // Get all GameObjects with a PersistentGUID
            var persistentGUIDs = FindObjectsOfType<PersistentGUID>();
            foreach (var persistentGUID in persistentGUIDs)
            {
                var gameObject = persistentGUID.gameObject;

                // Add to dict if not yet added
                if (!_guidToObject.ContainsKey(persistentGUID.GUID))
                {
                    _guidToObject[persistentGUID.GUID] = gameObject;
                    _objectToGuid[gameObject] = persistentGUID.GUID;
                }
            }

            Debug.Log("Dictionaries refreshed. Total GUIDs: " + _guidToObject.Count);
        }

        // Get a GameObject from a GUID
        public GameObject GetObjectByGUID(string guid)
        {
            _guidToObject.TryGetValue(guid, out var gameObject);
            return gameObject;
        }

        // Get a GUID from a GameObject
        public string GetGUIDByObject(GameObject gameObject)
        {
            _objectToGuid.TryGetValue(gameObject, out var guid);
            return guid;
        }

        // Set a GUID to a GameObject if not set yet
        public void AssignGUID(GameObject gameObject)
        {
            if (!_objectToGuid.ContainsKey(gameObject))
            {
                var persistentGUID = gameObject.GetComponent<PersistentGUID>();
                if (persistentGUID == null)
                {
                    persistentGUID = gameObject.AddComponent<PersistentGUID>();
                }

                // Generate new GUID if necessary
                if (string.IsNullOrEmpty(persistentGUID.GUID))
                {
                    persistentGUID.GenerateGUID();
                }

                // Add to dict
                _guidToObject[persistentGUID.GUID] = gameObject;
                _objectToGuid[gameObject] = persistentGUID.GUID;

                Debug.Log($"Assigned GUID {persistentGUID.GUID} to {gameObject.name}");
            }
        }

        // Remove a GUID from gameObject
        public void RemoveGUID(GameObject gameObject)
        {
            if (_objectToGuid.TryGetValue(gameObject, out var guid))
            {
                _guidToObject.Remove(guid);
                _objectToGuid.Remove(gameObject);

                var persistentGUID = gameObject.GetComponent<PersistentGUID>();
                if (persistentGUID != null)
                {
                    DestroyImmediate(persistentGUID);
                }

                Debug.Log($"Removed GUID {guid} from {gameObject.name}");
            }
        }
    }
}
