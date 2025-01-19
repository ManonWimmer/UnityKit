using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGivePommesSous : MonoBehaviour
{

    [ContextMenu("Test Give Pomme sous")]
    public void GiveOnePommesSous()
    {
        if (DialogueInventory.Instance == null) return;
        
        DialogueInventory.Instance.AddItem("PommesSous", 1);
    }
}
