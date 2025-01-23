using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CREMOT.DialogSystem
{
    public class DialogueInventory : MonoBehaviour
    {
        #region Fields
        private static DialogueInventory _instance;

        private Dictionary<string, int> _dialogueItemsInventory = new Dictionary<string, int>();

        #endregion

        #region Properties
        public static DialogueInventory Instance { get => _instance; set => _instance = value; }
        public Dictionary<string, int> DialogueItemsInventory { get => _dialogueItemsInventory; set => _dialogueItemsInventory = value; }


        #endregion

        #region Delegates

        public event Action OnUpdatedDialogueInventory;

        #endregion


        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
            }
            _instance = this;
        }


        public void AddItem(string stringName, int quantity)
        {
            if (_dialogueItemsInventory.ContainsKey(stringName))
            {
                _dialogueItemsInventory[stringName] += quantity;
            }
            else
            {
                _dialogueItemsInventory[stringName] = quantity;
            }

            OnUpdatedDialogueInventory?.Invoke();
        }
        public void RemoveItem(string stringName, int quantity)
        {
            if (_dialogueItemsInventory.ContainsKey(stringName))
            {
                _dialogueItemsInventory[stringName] -= quantity;
                if (_dialogueItemsInventory[stringName] < 0)
                {
                    _dialogueItemsInventory[stringName] = 0;
                }
            }

            OnUpdatedDialogueInventory?.Invoke();
        }

        public bool HasItem(string item, int quantity)
        {
            if (quantity <= 0) return true;

            if (!_dialogueItemsInventory.ContainsKey(item)) return false;

            if (_dialogueItemsInventory[item] < quantity) return false;

            return true;
        }
    }
}