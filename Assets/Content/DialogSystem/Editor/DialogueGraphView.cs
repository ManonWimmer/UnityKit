using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using UnityEditor;
using System.Linq;
using System.IO;

public class DialogueGraphView : GraphView
{

    private readonly Vector2 _defaultNodeSize = new Vector2(150, 200);

    private Vector2 _lastMousePosition;

    private DialogueNode _entryPointNode;

    private Vector2 _defaultEntryPointPosition = new Vector2(100, 200);
    private Vector2 _defaultEntryPointScale = new Vector2(100, 150);


    public DialogueGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());


        // Create grid (not working yet)
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();


        AddElement(GenererateEntryPointNode(_defaultEntryPointPosition));

        RegisterCallback<MouseDownEvent>(OnMouseDown);
    }


    #region Setup Context action events
    private void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button == 1) // Bouton droit
        {
            // Capturer la position de la souris en coordonnées globales
            _lastMousePosition = evt.mousePosition;
        }
    }
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        // Add a custom "Create Node" option
        evt.menu.AppendAction("Create dialoge Node", action =>
        {
            CreateNode("Dialogue Node", contentViewContainer.WorldToLocal(_lastMousePosition));
            Debug.Log("Position : " + contentViewContainer.WorldToLocal(_lastMousePosition));
        });

        // Add a custom "Re-generate Start Node" option
        evt.menu.AppendAction("Re-generate Start Node", action =>
        {
            AddElement(GenererateEntryPointNode(contentViewContainer.WorldToLocal(_lastMousePosition)));
            Debug.Log("Re-generate Start Node");
        });

        base.BuildContextualMenu(evt);
    }
    #endregion

    #region Ports / Node
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        }
        );

        return compatiblePorts;
    }

    private Port GeneratePort(DialogueNode node, Direction portDirection, Type type, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
    }

    private DialogueNode GenererateEntryPointNode(Vector2 position)
    {
        if (_entryPointNode !=  null && this.Contains(_entryPointNode)) return _entryPointNode;

        var node = new DialogueNode
        {
            title = "START",
            GIUD = Guid.NewGuid().ToString(),
            DialogueText = "ENTRYPOINT",
            EntryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output, typeof(float));
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(position, _defaultEntryPointScale));
        _entryPointNode = node;

        return node;
    }

    public void CreateNode(string nodeName, Vector2 position)
    {
        AddElement(CreateDialogueNode(nodeName, position));
    }

    public DialogueNode CreateDialogueNode(string nodeName, Vector2 position)
    {
        var dialogueNode = new DialogueNode()
        {
            title = nodeName,
            DialogueText = nodeName,
            GIUD = Guid.NewGuid().ToString()
        };

        var inputPort = GeneratePort(dialogueNode, Direction.Input, typeof(string), Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        AddNewChoiceButton(dialogueNode);

        AddIdTextField(dialogueNode);

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position, _defaultNodeSize));


        return dialogueNode;
    }



    private void AddChoicePort(DialogueNode dialogueNode)
    {
        var generatePort = GeneratePort(dialogueNode, Direction.Output, typeof(string));

        var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count ;
        var outputPortName = $"Choice {outputPortCount}";

        dialogueNode.outputContainer.Add(generatePort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }
    #endregion

    private void AddNewChoiceButton(DialogueNode dialogueNode)
    {
        var button = new Button(() =>
        {
            AddChoicePort(dialogueNode);
        });

        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);
    }

    private void AddIdTextField(DialogueNode dialogueNode)
    {
        // Create a text field for Dialogue ID
        var idField = new TextField("Dialogue ID")
        {
            value = "Set Id" // Default to the current GUID
        };

        dialogueNode.mainContainer.Add(idField);
    }

    #region Save / Load Graph

    public void SaveGraph(string path)
    {
        // Create a new ScriptableObject to save the graph
        var dialogueGraph = ScriptableObject.CreateInstance<DialogueGraphSO>();

        // Save nodes
        foreach (var node in nodes.ToList().OfType<DialogueNode>())
        {
            var nodeData = new DialogueNodeSO
            {
                id = node.GIUD,
                dialogueId = node.DialogueText, // Assuming DialogueText stores the custom ID
                title = node.title,
                position = node.GetPosition().position,
                entryPoint = node.EntryPoint
            };

            Debug.Log($"Saving node: {nodeData.title}, EntryPoint: {nodeData.entryPoint}, Position: {nodeData.position}");

            dialogueGraph.nodes.Add(nodeData);
        }

        // Save edges
        foreach (var edge in edges.ToList().OfType<Edge>())
        {
            if (edge.input == null || edge.output == null)
                continue;

            var edgeData = new DialogueEdgeSO
            {
                fromNodeId = ((DialogueNode)edge.output.node).GIUD,
                toNodeId = ((DialogueNode)edge.input.node).GIUD
            };

            dialogueGraph.edges.Add(edgeData);
        }

        // Save the ScriptableObject as an asset
        AssetDatabase.CreateAsset(dialogueGraph, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Graph saved at: {path}");
    }

    public void LoadGraph(DialogueGraphSO dialogueGraph)
    {
        Debug.Log($"Try to load asset : {dialogueGraph.name}");

        ClearGraph();

        // Load nodes
        foreach (var nodeData in dialogueGraph.nodes)
        {
            Debug.Log($"Loading node: {nodeData.title}, EntryPoint: {nodeData.entryPoint}, Position: {nodeData.position}");

            DialogueNode node;
            if (nodeData.entryPoint)
            {
                node = GenererateEntryPointNode(nodeData.position);
            }
            else
            {
                node = CreateDialogueNode(nodeData.title, nodeData.position);
            }
            node.GIUD = nodeData.id;
            node.DialogueText = nodeData.dialogueId; // Custom ID
            AddElement(node);
        }

        // Load edges
        foreach (var edgeData in dialogueGraph.edges)
        {
            var fromNode = nodes.ToList().OfType<DialogueNode>().FirstOrDefault(n => n.GIUD == edgeData.fromNodeId);
            var toNode = nodes.ToList().OfType<DialogueNode>().FirstOrDefault(n => n.GIUD == edgeData.toNodeId);

            if (fromNode == null || toNode == null)
            {
                Debug.LogWarning($"Failed to find nodes for edge: {edgeData.fromNodeId} -> {edgeData.toNodeId}");
                continue;
            }

            if (fromNode.outputContainer.childCount == 0 || toNode.inputContainer.childCount == 0)
            {
                Debug.LogWarning($"Node ports are missing for edge: {fromNode.title} -> {toNode.title}");
                continue;
            }

            // Connect the first available ports
            var fromPort = fromNode.outputContainer.ElementAt(0) as Port;
            var toPort = toNode.inputContainer.ElementAt(0) as Port;

            if (fromPort == null || toPort == null)
            {
                Debug.LogWarning($"Failed to retrieve ports for edge: {fromNode.title} -> {toNode.title}");
                continue;
            }

            var edge = fromPort.ConnectTo(toPort);
            AddElement(edge);
        }
    }

    public void ClearGraph()
    {
        var elements = graphElements.ToList();
        foreach (var element in elements)
        {
            RemoveElement(element);
        }
    }
    #endregion

}
