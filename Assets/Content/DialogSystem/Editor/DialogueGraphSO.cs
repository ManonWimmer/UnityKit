using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
    public List<string> outputPorts = new List<string>(); // Liste des noms des ports de sortie
}

[System.Serializable]
public class DialogueEdgeSO
{
    public string fromNodeId;
    public string fromPortId;

    public string toNodeId;
    public string toPortId;

    public int fromPortIndex;
    public int toPortIndex;
}
