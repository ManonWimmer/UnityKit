using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIdToDialogueConverter", menuName = "Dialogue/IdToDialogueConverter")]
public class IdToDialogueSO : ScriptableObject
{
    [Serializable]
    public class DialogueEntry
    {
        public string id;
        public string dialogueText;
    }

    [SerializeField] private List<DialogueEntry> _dialogueEntries;

    private Dictionary<string, string> _idToTextConverter;

    public Dictionary<string, string> IdToTextConverter
    {
        get
        {
            if (_idToTextConverter == null)
            {
                _idToTextConverter = new Dictionary<string, string>();
                foreach (var entry in _dialogueEntries)
                {
                    if (!_idToTextConverter.ContainsKey(entry.id))
                    {
                        _idToTextConverter.Add(entry.id, entry.dialogueText);
                    }
                    else
                    {
                        Debug.Log($"Duplicate dialogue ID detected: {entry.id}");
                    }
                }
            }
            return _idToTextConverter;
        }
    }
}
