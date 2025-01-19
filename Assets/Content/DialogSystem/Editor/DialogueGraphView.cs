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
using static UnityEditor.Experimental.GraphView.GraphView;

public class DialogueGraphView : GraphView
{
    #region Fields
    private readonly Vector2 _defaultNodeSize = new Vector2(150, 200);
    private Vector2 _defaultEntryPointPosition = new Vector2(100, 200);
    private Vector2 _defaultEntryPointScale = new Vector2(100, 150);

    private Vector2 _lastMousePosition;
    private DialogueNode _entryPointNode;
    #endregion

    #region Constructor
    public DialogueGraphView()
    {
        ConfigureGraphView(); // Set up graph interactions (zoom, drag, selection)
        AddGridBackground(); // Add a grid background for visual clarity
        AddEntryPoint(); // Create and add the entry/start node
        RegisterRightClickEvents(); // Register right-click for context menu
    }

    private void ConfigureGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger()); // Allow graph panning
        this.AddManipulator(new SelectionDragger()); // Enable dragging selected elements
        this.AddManipulator(new RectangleSelector()); // Enable selection with a rectangle
    }

    private void AddGridBackground()
    {
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    private void AddEntryPoint()
    {
        AddElement(GenerateEntryPointNode(_defaultEntryPointPosition));
    }

    private void RegisterRightClickEvents()
    {
        RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.button == 1) // Right-click
            {
                _lastMousePosition = evt.mousePosition; // Save mouse position for new node placement
            }
        });
    }
    #endregion

    #region Context Menu
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)   // Add methods to right clic menu
    {
        // Add a custom "Create Node" option
        evt.menu.AppendAction("Create dialoge Node", _ =>
        {
            CreateNode("Dialogue Node", contentViewContainer.WorldToLocal(_lastMousePosition));
        });

        // Add a custom "Re-generate Start Node" option
        evt.menu.AppendAction("Re-generate Start Node", _ =>
        {
            AddElement(GenerateEntryPointNode(contentViewContainer.WorldToLocal(_lastMousePosition)));
            //Debug.Log("Re-generate Start Node");
        });

        base.BuildContextualMenu(evt);
    }
    #endregion

    #region Node Management
    private DialogueNode GenerateEntryPointNode(Vector2 position)   // generate an Entry Point Node
    {
        // If the entry node already exists, return it
        if (_entryPointNode != null && this.Contains(_entryPointNode)) return _entryPointNode;

        // Create the entry node with basic configuration
        var node = CreateBasicNode("START", position, _defaultEntryPointScale);
        node.EntryPoint = true;

        // Add an output port to the entry node
        var outputPort = GeneratePort(node, Direction.Output, typeof(float), Port.Capacity.Single);
        outputPort.portName = "Next";
        node.outputContainer.Add(outputPort);

        _entryPointNode = node;

        node.RefreshExpandedState();    // Update UI
        node.RefreshPorts();    //Update Ports
        return node;
    }
    public void CreateNode(string nodeName, Vector2 position)   // Create basic Dialogue Node
    {
        AddElement(CreateDialogueNode(nodeName, position));
    }
    private DialogueNode CreateDialogueNode(string nodeName, Vector2 position, List<string> outputPorts = null, List<string> outputPortsChoiceId = null, List<PortCondition> portConditions = null)    // Create a Dialogue Node
    {
        // Create a dialogue node with basic configuration
        var node = CreateBasicNode(nodeName, position, _defaultNodeSize);

        if (portConditions != null)
        {
            node.PortConditions = portConditions;
        }

        // Add input port
        var inputPort = GeneratePort(node, Direction.Input, typeof(string), Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        // Add functionality for creating choices and assigning IDs
        AddNewChoiceButton(node);
        AddIdTextField(node);

        // Add any predefined output ports
        AddOutputPorts(node, outputPorts, outputPortsChoiceId);

        node.RefreshExpandedState();
        node.RefreshPorts();
        return node;
    }
    private DialogueNode CreateBasicNode(string nodeName, Vector2 position, Vector2 size)   // Initialize a basic node with a title and unique GUID
    {
        var node = new DialogueNode
        {
            title = nodeName,
            DialogueText = nodeName,
            GIUD = Guid.NewGuid().ToString()
        };

        node.SetPosition(new Rect(position, size));
        node.RefreshExpandedState();
        node.RefreshPorts();
        return node;
    }
    private void AddOutputPorts(DialogueNode node, List<string> portNames, List<string> portChoiceIds)  // Add output ports if defined
    {
        if (portNames == null || portChoiceIds == null) return;

        for (int i = 0; i < portNames.Count; i++)
        {
            var portName = portNames[i];
            var portChoiceId = portChoiceIds.ElementAtOrDefault(i) ?? $"Choice {i}";

            var port = GeneratePort(node, Direction.Output, typeof(string));
            port.name = portName;   // Unique port name for saving/loading
            port.portName = portChoiceId;   // Visible label

            // Create a container with a text field and delete button for the port
            var portContainer = CreatePortContainer(port, portChoiceId, node);
            node.outputContainer.Add(portContainer);

            node.RefreshPorts();
            node.RefreshExpandedState();
        }
    }

    #endregion

    #region Ports Management
    private Port GeneratePort(DialogueNode node, Direction portDirection, Type type, Port.Capacity capacity = Port.Capacity.Single) // Create a port for the node
    {
        var port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
        port.name = Guid.NewGuid().ToString();
        return port;
    }
    private VisualElement CreatePortContainer(Port port, string defaultName, DialogueNode node = null)    // Create a container to hold the port, text field, and delete button
    {
        var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };

        AddEditConditionsButton(container, port, node);
        AddChoiceIdTextField(container, port, defaultName);
        AddDeletePortButton(container, port);

        container.Add(port);
        return container;
    }
    private void AddChoiceIdTextField(VisualElement container, Port port, string defaultName)   // Add a text field to rename the port
    {
        var textField = new TextField { value = defaultName, style = { flexGrow = 1, marginRight = 4 } };
        textField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
        container.Add(textField);
    }
    private void AddDeletePortButton(VisualElement container, Port port)    // Add a button to delete the port
    {
        var deleteButton = new Button(() =>
        {
            var node = (DialogueNode)port.node;
            DisconnectEdges(port);
            node.outputContainer.Remove(container);
            node.RefreshPorts();
            node.RefreshExpandedState();
        })
        {
            text = "X",
            style = { marginLeft = 4, backgroundColor = new Color(0.8f, 0.2f, 0.2f), color = Color.white, width = 20 }
        };
        container.Add(deleteButton);
    }
    private void AddEditConditionsButton(VisualElement container, Port port, DialogueNode node) // Add a button to open Edit Condition window
    {
        var editConditionsButton = new Button(() =>
        {
            ShowConditionEditor(node, port.name);   // Open edition window
        })
        {
            text = "Edit Conditions"
        };

        container.Add(editConditionsButton);
    }

    private void DisconnectEdges(Port port) // Remove all edges connected to a given port
    {
        edges.ToList().Where(edge => edge.input == port || edge.output == port).ToList().ForEach(RemoveElement);
    }

    #endregion

    #region Utility Buttons
    private void AddNewChoiceButton(DialogueNode node)  // Add a button for creating new output choices
    {
        var button = new Button(() => AddChoicePort(node)) { text = "New Choice" };
        node.titleContainer.Add(button);
    }
    private void AddChoicePort(DialogueNode node)   // Add a new output port for a choice
    {
        var port = GeneratePort(node, Direction.Output, typeof(string));
        port.portName = $"Choice {node.outputContainer.childCount}";    // Default name

        var container = CreatePortContainer(port, port.portName);

        /*
        // ----- Condition Part ----------

        var editConditionsButton = new Button(() =>
        {
            ShowConditionEditor(node, port.name);   // Ouvrir fenetre d'edition
        })
        {
            text = "Edit Conditions"
        };

        container.Add(editConditionsButton);

        // -------------------------------
        */


        node.outputContainer.Add(container);
        node.RefreshPorts();
        node.RefreshExpandedState();
    }
    private void AddIdTextField(DialogueNode node)  // Add a text field for the dialogue ID
    {
        var textField = new TextField("Dialogue ID") {
            value = node.DialogueText,
            name = "DialogueIdField"    // Assign a name for later retrieval
        }; 
        
        textField.RegisterValueChangedCallback(evt => node.DialogueText = evt.newValue);
        node.mainContainer.Add(textField);
    }

    #endregion

    #region Compatbile Ports (Not used)
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)  // Define compatible ports for connecting edges (not used)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        }
        );

        return compatiblePorts;
    }

    #endregion


    #region Save / Load Graph

    #region (save/load OLD)
    /*
    public void SaveGraph(string path)  // Save the graph in a DialogueGraphSO
    {
        // Create a new ScriptableObject to save the graph
        var dialogueGraph = ScriptableObject.CreateInstance<DialogueGraphSO>();

        // Save nodes
        foreach (var node in nodes.ToList().OfType<DialogueNode>())
        {
            var nodeData = new DialogueNodeSO
            {
                id = node.GIUD,
                dialogueId = node.DialogueText, // DialogueText stores the custom ID
                title = node.title,
                position = node.GetPosition().position,
                entryPoint = node.EntryPoint,
                outputPorts = node.outputContainer.Query<Port>()
                        .ToList()
                        .Select(port => port.name )
                        .ToList(), // Sauvegarder les noms des ports de sortie

                outputPortsChoiceId = node.outputContainer.Query<Port>()
                        .ToList()
                        .Select(port => port.portName)
                        .ToList(), // Sauvegarder les noms des ports de sortie
            };

            Debug.Log($"Saving node: {nodeData.title}, EntryPoint: {nodeData.entryPoint}, Position: {nodeData.position}");

            dialogueGraph.Nodes.Add(nodeData);
        }

        // Save edges
        foreach (var edge in edges.ToList().OfType<Edge>())
        {
            if (edge.input == null || edge.output == null)
                continue;

            // -------------------------------------------
            // partie pas propre : j'init l'index du port différement en fonction de si c'est un port dialogue ou pas (donc compris dans une VE parent)
            
            var tempFromDialogueNode = ((DialogueNode)edge.output.node);
            int tempFromPortIndex = 0;
            if (tempFromDialogueNode != null)
            {
                if (tempFromDialogueNode.EntryPoint)
                {
                    tempFromPortIndex = edge.output.parent.IndexOf(edge.output);
                }
                else
                {
                    tempFromPortIndex = edge.output.parent.parent.IndexOf(edge.output.parent);
                }
            }

            // --------------------------------------------

            var edgeData = new DialogueEdgeSO
            {
                fromNodeId = ((DialogueNode)edge.output.node).GIUD,
                fromPortId = edge.output.name, // Sauvegarder l'identifiant unique du port

                
                //fromPortIndex = edge.output.parent.parent.IndexOf(edge.output.parent),
                fromPortIndex = tempFromPortIndex,  // -> suite de partie pas propre juste au dessus

                toNodeId = ((DialogueNode)edge.input.node).GIUD,
                toPortId = edge.input.name, // Sauvegarder l'identifiant unique du port

                toPortIndex = edge.input.parent.IndexOf(edge.input),
            };

            dialogueGraph.Edges.Add(edgeData);
        }

        // Save the ScriptableObject as an asset
        AssetDatabase.CreateAsset(dialogueGraph, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Graph saved at: {path}");
    }

    public void LoadGraph(DialogueGraphSO dialogueGraph)    // Load and display a graph from a DialogueGraphSO
    {
        Debug.Log($"Try to load asset : {dialogueGraph.name}");

        ClearGraph();

        // Load nodes
        foreach (var nodeData in dialogueGraph.Nodes)
        {
            Debug.Log($"Loading node: {nodeData.title}, EntryPoint: {nodeData.entryPoint}, Position: {nodeData.position}");

            DialogueNode node;
            if (nodeData.entryPoint)
            {
                node = GenerateEntryPointNode(nodeData.position);
            }
            else
            {
                node = CreateDialogueNode(nodeData.title, nodeData.position, nodeData.outputPorts, nodeData.outputPortsChoiceId);
            }
            node.GIUD = nodeData.id;
            node.DialogueText = nodeData.dialogueId; // Custom ID

            // Mettre à jour visuellement le TextField
            var idField = node.mainContainer.Q<TextField>("DialogueIdField");
            if (idField != null)
            {
                idField.value = node.DialogueText;
            }

            AddElement(node);
        }

        // Load edges
        foreach (var edgeData in dialogueGraph.Edges)
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

            // Recup ports en utilisant l'index
            //var fromPort = fromNode.outputContainer.Children().OfType<Port>().ElementAt(edgeData.fromPortIndex);
            //var toPort = toNode.inputContainer.Children().OfType<Port>().ElementAt(edgeData.toPortIndex);

            var fromPort = fromNode.outputContainer.Query<Port>().ToList().FirstOrDefault(p => p.name == edgeData.fromPortId);
            if (fromNode.EntryPoint)
            {
                fromPort = fromNode.outputContainer.Children().OfType<Port>().ElementAt(edgeData.fromPortIndex);
            }
            //var toPort = toNode.inputContainer.Query<Port>().ToList().FirstOrDefault(p => p.name == edgeData.toPortId);
            var toPort = toNode.inputContainer.Children().OfType<Port>().ElementAt(edgeData.toPortIndex);

            //Debug.Log("FromPortId Name : " + edgeData.fromPortId + " / node name = " + edgeData.fromNodeId);
            //Debug.Log("ToPortId Name : " + edgeData.toPortId + " / node name = " + edgeData.toNodeId);

            if (fromPort == null || toPort == null)
            {
                Debug.LogWarning($"Failed to retrieve ports for edge: {fromNode.title} -> {toNode.title}");

                //if (fromPort == null)
                //{
                //    Debug.LogWarning("Failed to find FromPort");
                //}
                //if (toPort == null)
                //{
                //    Debug.LogWarning("Failed to find ToPort");
                //}
                continue;
            }

            var edge = fromPort.ConnectTo(toPort);
            AddElement(edge);
        }
    }
    */
    #endregion

    #region Save
    public void SaveGraph(string path)
    {
        var dialogueGraph = ScriptableObject.CreateInstance<DialogueGraphSO>();

        SaveNodes(dialogueGraph);
        SaveEdges(dialogueGraph);

        AssetDatabase.CreateAsset(dialogueGraph, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Graph saved at: {path}");
    }
    private void SaveNodes(DialogueGraphSO dialogueGraph)
    {
        foreach (var node in nodes.ToList().OfType<DialogueNode>())
        {
            /*
            var nodeData = new DialogueNodeSO
            {
                id = node.GIUD,
                dialogueId = node.DialogueText, // Custom ID in graph
                title = node.title,
                position = node.GetPosition().position,
                entryPoint = node.EntryPoint,
                outputPorts = ExtractPortNames(node),
                outputPortsChoiceId = ExtractPortLabels(node)
            };
            */
            var nodeData = node.ToSO();

            dialogueGraph.Nodes.Add(nodeData);
        }
    }
    private void SaveEdges(DialogueGraphSO dialogueGraph)
    {
        foreach (var edge in edges.ToList().OfType<Edge>())
        {
            if (edge.input == null || edge.output == null) continue;

            var fromNode = edge.output.node as DialogueNode;
            var toNode = edge.input.node as DialogueNode;

            if (fromNode == null || toNode == null) continue;

            var edgeData = new DialogueEdgeSO
            {
                fromNodeId = fromNode.GIUD,
                fromPortId = edge.output.name,
                fromPortIndex = GetPortIndex(edge.output, fromNode),
                toNodeId = toNode.GIUD,
                toPortId = edge.input.name,
                toPortIndex = GetPortIndex(edge.input, toNode)
            };

            dialogueGraph.Edges.Add(edgeData);
        }
    }
    private List<string> ExtractPortNames(DialogueNode node)
    {
        return node.outputContainer.Query<Port>().ToList().Select(port => port.name).ToList();
    }

    private List<string> ExtractPortLabels(DialogueNode node)
    {
        return node.outputContainer.Query<Port>().ToList().Select(port => port.portName).ToList();
    }

    private int GetPortIndex(Port port, DialogueNode node)
    {
        return node.EntryPoint
            ? port.parent.IndexOf(port) // Direct child of the output container
            : port.parent.parent.IndexOf(port.parent);
    }
    #endregion

    #region Load
    public void LoadGraph(DialogueGraphSO dialogueGraph)
    {
        Debug.Log($"Loading graph: {dialogueGraph.name}");
        ClearGraph();

        LoadNodes(dialogueGraph);
        LoadEdges(dialogueGraph);
    }
    private void LoadNodes(DialogueGraphSO dialogueGraph)
    {
        foreach (var nodeData in dialogueGraph.Nodes)
        {
            var node = nodeData.entryPoint
                ? GenerateEntryPointNode(nodeData.position)
                : CreateDialogueNode(nodeData.title, nodeData.position, nodeData.outputPorts, nodeData.outputPortsChoiceId, nodeData.portConditions);

            node.GIUD = nodeData.id;
            node.DialogueText = nodeData.dialogueId;    // Custom ID in graph

            UpdateNodeIdField(node);

            AddElement(node);
        }
    }
    private void UpdateNodeIdField(DialogueNode node)
    {
        var idField = node.mainContainer.Q<TextField>("DialogueIdField");
        if (idField != null)
        {
            idField.value = node.DialogueText;
        }
    }
    private void LoadEdges(DialogueGraphSO dialogueGraph)
    {
        foreach (var edgeData in dialogueGraph.Edges)
        {
            var fromNode = FindNodeById(edgeData.fromNodeId);
            var toNode = FindNodeById(edgeData.toNodeId);

            if (fromNode == null || toNode == null) continue;

            var fromPort = FindOutputPort(fromNode, edgeData.fromPortId, edgeData.fromPortIndex);
            var toPort = FindInputPort(toNode, edgeData.toPortIndex);

            if (fromPort != null && toPort != null)
            {
                var edge = fromPort.ConnectTo(toPort);
                AddElement(edge);
            }
        }
    }
    private DialogueNode FindNodeById(string id)
    {
        return nodes.ToList().OfType<DialogueNode>().FirstOrDefault(n => n.GIUD == id);
    }

    private Port FindOutputPort(DialogueNode node, string portId, int index)
    {
        return node.EntryPoint
            ? node.outputContainer.Children().OfType<Port>().ElementAtOrDefault(index)
            : node.outputContainer.Query<Port>().ToList().FirstOrDefault(p => p.name == portId);
    }

    private Port FindInputPort(DialogueNode node, int index)
    {
        return node.inputContainer.Children().OfType<Port>().ElementAtOrDefault(index);
    }
    #endregion

    #endregion

    public void ClearGraph()
    {
        foreach (var element in graphElements.ToList())
        {
            RemoveElement(element);
        }

        _entryPointNode = null;
    }


    #region Conditions
    private void ShowConditionEditor(DialogueNode node, string portId)
    {
        // Implémentez une fenêtre contextuelle pour éditer les conditions
        var window = EditorWindow.CreateInstance<ConditionEditorWindow>();
        window.Init(node, portId); // Passez le nœud et l'identifiant du port
        window.Show();
    }

    #endregion
}
