using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInventory : MonoBehaviour
{
    #region Fields
    private static DialogueInventory _instance;

    private Dictionary<string, int> _dialogueInventory = new Dictionary<string, int>();

    #endregion

    #region Properties
    public static DialogueInventory Instance { get => _instance; set => _instance = value; }


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
        if (_dialogueInventory.ContainsKey(stringName))
        {
            _dialogueInventory[stringName] += quantity;
        }
        else
        {
            _dialogueInventory[stringName] = quantity;
        }

        OnUpdatedDialogueInventory?.Invoke();
    }
    public bool HasItem(string item, int quantity)
    {
        if (quantity <= 0) return true;

        if (!_dialogueInventory.ContainsKey(item)) return false;

        if (_dialogueInventory[item] < quantity) return false;

        return true;
    }
}
