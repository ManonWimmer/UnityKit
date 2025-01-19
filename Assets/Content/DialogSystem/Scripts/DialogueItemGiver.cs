using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueItemGiver : MonoBehaviour
{
    #region Fields

    [Header("Key parameters")]
    [SerializeField] private string _itemKeyToGive;

    [SerializeField] private int _itemNumberToGive;
    #endregion


    public void GiveItemToDialogueInventory()
    {
        if (_itemKeyToGive == null) return;
        if (_itemNumberToGive <= 0) return;

        Debug.Log("Null check");
        if (DialogueInventory.Instance == null) return;

        Debug.Log("Add Item");
        DialogueInventory.Instance.AddItem(_itemKeyToGive, _itemNumberToGive);
    }

}
