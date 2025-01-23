using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CREMOT.DialogSystem
{
    public class DialogueItemGiver : MonoBehaviour
    {
        #region Fields

        [Header("Key parameters")]
        [SerializeField] private string _itemKeyToGive;

        [SerializeField] private int _itemNumber;
        #endregion


        #region Give / Remove Item
        public void GiveItemToDialogueInventory()
        {
            if (_itemKeyToGive == null) return;
            if (_itemNumber <= 0) return;

            if (DialogueInventory.Instance == null) return;

            DialogueInventory.Instance.AddItem(_itemKeyToGive, _itemNumber);
        }
        public void removeItemToDialogueInventory()
        {
            if (_itemKeyToGive == null) return;
            if (_itemNumber <= 0) return;

            if (DialogueInventory.Instance == null) return;

            DialogueInventory.Instance.RemoveItem(_itemKeyToGive, _itemNumber);
        }

        #endregion
    }
}
