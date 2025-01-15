using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIdToDialogueConverter", menuName = "Dialogue/IdToDialogueConverter")]
public class IdToDialogueSO : ScriptableObject
{
    private Dictionary<string, string> _idToDialogueConverter;


    public Dictionary<string, string> IdToDialogueConverter { get => _idToDialogueConverter; set => _idToDialogueConverter = value; }
}
