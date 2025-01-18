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
        ConfigureGraphView();
        AddGridBackground();
        AddEntryPoint();
        RegisterRightClickEvents();
    }

    private void ConfigureGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
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
                _lastMousePosition = evt.mousePosition; // Hide the mouse position
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
    private DialogueNode GenerateEntryPointNode(Vector2 position)
    {
        if (_entryPointNode != null && this.Contains(_entryPointNode)) return _entryPointNode;

        var node = CreateBasicNode("START", position, _defaultEntryPointScale);
        node.EntryPoint = true;

        var outputPort = GeneratePort(node, Direction.Output, typeof(float), Port.Capacity.Single);
        outputPort.portName = "Next";
        node.outputContainer.Add(outputPort);

        _entryPointNode = node;

        node.RefreshExpandedState();
        node.RefreshPorts();
        return node;
    }
    public void CreateNode(string nodeName, Vector2 position)
    {
        AddElement(CreateDialogueNode(nodeName, position));
    }
    private DialogueNode CreateDialogueNode(string nodeName, Vector2 position, List<string> outputPorts = null, List<string> outputPortsChoiceId = null)
    {
        var node = CreateBasicNode(nodeName, position, _defaultNodeSize);

        var inputPort = GeneratePort(node, Direction.Input, typeof(string), Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        AddNewChoiceButton(node);
        AddIdTextField(node);

        AddOutputPorts(node, outputPorts, outputPortsChoiceId);

        node.RefreshExpandedState();
        node.RefreshPorts();
        return node;
    }
    private DialogueNode CreateBasicNode(string nodeName, Vector2 position, Vector2 size)
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
    private void AddOutputPorts(DialogueNode node, List<string> portNames, List<string> portChoiceIds)
    {
        if (portNames == null || portChoiceIds == null) return;

        for (int i = 0; i < portNames.Count; i++)
        {
            var portName = portNames[i];
            var portChoiceId = portChoiceIds.ElementAtOrDefault(i) ?? $"Choice {i}";

            var port = GeneratePort(node, Direction.Output, typeof(string));
            port.name = portName;
            port.portName = portChoiceId;

            var portContainer = CreatePortContainer(port, portChoiceId);
            node.outputContainer.Add(portContainer);

            node.RefreshPorts();
            node.RefreshExpandedState();
        }
    }

    #endregion

    #region Ports Management
    private Port GeneratePort(DialogueNode node, Direction portDirection, Type type, Port.Capacity capacity = Port.Capacity.Single)
    {

        var port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
        port.name = Guid.NewGuid().ToString();
        return port;
    }
    private VisualElement CreatePortContainer(Port port, string defaultName)
    {
        var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };

        AddChoiceIdTextField(container, port, defaultName);
        AddDeletePortButton(container, port);

        container.Add(port);
        return container;
    }
    private void AddChoiceIdTextField(VisualElement container, Port port, string defaultName)
    {
        var textField = new TextField { value = defaultName, style = { flexGrow = 1, marginRight = 4 } };
        textField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
        container.Add(textField);
    }
    private void AddDeletePortButton(VisualElement container, Port port)
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
    private void DisconnectEdges(Port port)
    {
        edges.ToList().Where(edge => edge.input == port || edge.output == port).ToList().ForEach(RemoveElement);
    }

    #endregion

    #region Utility Buttons
    private void AddNewChoiceButton(DialogueNode node)
    {
        var button = new Button(() => AddChoicePort(node)) { text = "New Choice" };
        node.titleContainer.Add(button);
    }
    private void AddChoicePort(DialogueNode node)
    {
        var port = GeneratePort(node, Direction.Output, typeof(string));
        port.portName = $"Choice {node.outputContainer.childCount}";
        var container = CreatePortContainer(port, port.portName);
        node.outputContainer.Add(container);
        node.RefreshPorts();
        node.RefreshExpandedState();
    }
    private void AddIdTextField(DialogueNode node)
    {
        var textField = new TextField("Dialogue ID") { value = node.DialogueText, name = "DialogueIdField" };
        textField.RegisterValueChangedCallback(evt => node.DialogueText = evt.newValue);
        node.mainContainer.Add(textField);
    }

    #endregion


    #region Ports (OLD)
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
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

    /*
    private Port GeneratePort(DialogueNode node, Direction portDirection, Type type, Port.Capacity capacity = Port.Capacity.Single)
    {
        var port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
        port.name = Guid.NewGuid().ToString(); // unique Identifiant for this port
        return port;
    }
    */
    /*
    private void AddChoicePort(DialogueNode dialogueNode)
    {
        var generatePort = GeneratePort(dialogueNode, Direction.Output, typeof(string));

        var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count ;
        var outputPortName = $"Choice {outputPortCount}";

        generatePort.portName = outputPortName;

        // Cr�er un conteneur horizontal pour le port et le bouton de suppression
        var portContainer = new VisualElement();
        portContainer.style.flexDirection = FlexDirection.Row;

        AddChoiceIdTextField(dialogueNode, generatePort, portContainer);

        CreateDeleteChoiceButton(dialogueNode, portContainer);
        
        // Ajouter le port au conteneur
        portContainer.Add(generatePort);

        // Conteneur au node
        dialogueNode.outputContainer.Add(portContainer);


        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }
    */
    #endregion


    #region Node (OLD)
    //private DialogueNode GenerateEntryPointNode(Vector2 position)
    //{
    //    if (_entryPointNode !=  null && this.Contains(_entryPointNode)) return _entryPointNode;

    //    var node = new DialogueNode
    //    {
    //        title = "START",
    //        GIUD = Guid.NewGuid().ToString(),
    //        DialogueText = "ENTRYPOINT",
    //        EntryPoint = true
    //    };

    //    var generatedPort = GeneratePort(node, Direction.Output, typeof(float));
    //    generatedPort.portName = "Next";
    //    node.outputContainer.Add(generatedPort);

    //    node.RefreshExpandedState();
    //    node.RefreshPorts();

    //    node.SetPosition(new Rect(position, _defaultEntryPointScale));
    //    _entryPointNode = node;

    //    return node;
    //}

    /*
    public DialogueNode CreateDialogueNode(string nodeName, Vector2 position, List<string> outputPorts = null, List<string> outputPortsChoiceId = null)
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

        // Ajouter les ports de sortie sauvegard�s
        if (outputPorts != null)
        {
            //foreach (var portName in outputPorts)
            for (int i = 0; i < outputPorts.Count; ++i)
            {
                var port = GeneratePort(dialogueNode, Direction.Output, typeof(string));
                var portContainer = new VisualElement();

                var portName = "";
                var portChoiceId = "";
                if (i < outputPorts.Count && outputPorts != null)
                {
                    portName = outputPorts[i];
                    port.name = portName;
                }
                if (i < outputPortsChoiceId.Count && outputPortsChoiceId != null)
                {
                    portChoiceId = outputPortsChoiceId[i];
                    port.portName = portChoiceId;
                }
                else
                {
                    var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
                    var outputPortName = $"Choice {outputPortCount}";
                    port.portName = $"Choice {outputPortCount}";   // DefaultName for now
                }


                portContainer.style.flexDirection = FlexDirection.Row;

                AddChoiceIdTextField(dialogueNode, port, portContainer, portChoiceId);
                CreateDeleteChoiceButton(dialogueNode, portContainer);
                portContainer.Add(port);
                dialogueNode.outputContainer.Add(portContainer);
            }
        }

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position, _defaultNodeSize));


        return dialogueNode;
    }
    */
    #endregion

    #region Add Button / Fields to node (OLD)
    /*
    private void CreateDeleteChoiceButton(DialogueNode dialogueNode, VisualElement portContainer)
    {
        // Cr�er le bouton de suppression
        var deleteButton = new Button(() =>
        {
            // trouver port du choix suppr
            var port = portContainer.Q<Port>();

            // Verif si lien depuis ce port
            if (port != null)
            {
                var connectedEdges = edges.ToList().Where(edge => edge.input == port || edge.output == port).ToList();

                foreach (var edge in connectedEdges)
                {
                    edge.input.Disconnect(edge);
                    edge.output.Disconnect(edge);
                    RemoveElement(edge);
                }
            }


            // Supprimer le port de l'UI
            dialogueNode.outputContainer.Remove(portContainer);

            // Rafra�chir les ports et l'�tat du n�ud
            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();
        })
        {
            text = "X" // Texte ou ic�ne du bouton
        };

        // style bouton pour qu'il soit petit
        deleteButton.style.marginLeft = 4;
        deleteButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
        deleteButton.style.color = Color.white;
        deleteButton.style.width = 20;

        // bouton au conteneur
        portContainer.Add(deleteButton);
    }
    */
    /*
    private void AddNewChoiceButton(DialogueNode dialogueNode)
    {
        var button = new Button(() =>
        {
            AddChoicePort(dialogueNode);
        });

        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);
    }
    */
    /*
    private void AddChoiceIdTextField(DialogueNode dialogueNode, Port associatedPort, VisualElement portContainer, string defaultPortName = "DefaultName")
    {
        var textField = new TextField()
        {
            value = defaultPortName, // Nom initial
            style =
                {
                    flexGrow = 1,
                    marginRight = 4, // Espacement
                }
        };

        // Mettre � jour dynamiquement le nom du port lorsque le texte change
        textField.RegisterValueChangedCallback(evt =>
        {
            associatedPort.portName = evt.newValue; // Mettre � jour le nom affich� sur le port
        });

        portContainer.Add(textField);
    }
    */
    /*
    private void AddIdTextField(DialogueNode dialogueNode)
    {
        // Create a text field for Dialogue ID
        var idField = new TextField("Dialogue ID")
        {
            value = dialogueNode.DialogueText, // Default to the current GUID
            name = "Dialogue ID"
        };

        // Synchroniser la valeur du champ avec le DialogueText du noeud
        idField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueText = evt.newValue; // Mettre � jour le DialogueText
        });

        dialogueNode.mainContainer.Add(idField);
    }
    */

    #endregion


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
            // partie pas propre : j'init l'index du port diff�rement en fonction de si c'est un port dialogue ou pas (donc compris dans une VE parent)
            
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

    public void LoadGraph(DialogueGraphSO dialogueGraph)
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

            // Mettre � jour visuellement le TextField
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

    public void ClearGraph()
    {
        var elements = graphElements.ToList();
        foreach (var element in elements)
        {
            RemoveElement(element);
        }

        _entryPointNode = null;
    }
    #endregion

}
