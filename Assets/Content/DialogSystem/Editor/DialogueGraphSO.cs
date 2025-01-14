using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueGraph", menuName = "Dialogue/Dialogue Graph")]
public class DialogueGraphSO : ScriptableObject
{
    public List<DialogueNodeSO> nodes = new List<DialogueNodeSO>();
    public List<DialogueEdgeSO> edges = new List<DialogueEdgeSO>();
}

[System.Serializable]
public class DialogueNodeSO
{
    public string id;
    public string dialogueId;
    public string title;
    public Vector2 position;
    public bool entryPoint = false;
}

[System.Serializable]
public class DialogueEdgeSO
{
    public string fromNodeId;
    public string toNodeId;
}
