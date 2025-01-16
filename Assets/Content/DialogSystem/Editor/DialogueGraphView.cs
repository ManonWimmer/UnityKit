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
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        }
        );

        return compatiblePorts;
    }

    private Port GeneratePort(DialogueNode node, Direction portDirection, Type type, Port.Capacity capacity = Port.Capacity.Single)
    {
        var port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
        port.name = Guid.NewGuid().ToString(); // Identifiant unique pour ce port
        return port;
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

    public DialogueNode CreateDialogueNode(string nodeName, Vector2 position, List<string> outputPorts = null, string inputPortName = "")
    {
        var dialogueNode = new DialogueNode()
        {
            title = nodeName,
            DialogueText = nodeName,
            GIUD = Guid.NewGuid().ToString()
        };

        var inputPort = GeneratePort(dialogueNode, Direction.Input, typeof(string), Port.Capacity.Multi);
        if (inputPortName != "")
        {
            inputPort.name = inputPortName;
        }
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        AddNewChoiceButton(dialogueNode);
        AddIdTextField(dialogueNode);

        // Ajouter les ports de sortie sauvegardés
        if (outputPorts != null)
        {
            foreach (var portName in outputPorts)
            {
                var port = GeneratePort(dialogueNode, Direction.Output, typeof(string));
                var portContainer = new VisualElement();
                port.name = portName;

                var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
                var outputPortName = $"Choice {outputPortCount}";
                port.portName = $"Choice {outputPortCount}";   // Replace by actual display name later

                portContainer.style.flexDirection = FlexDirection.Row;

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


    private void AddChoicePort(DialogueNode dialogueNode)
    {
        var generatePort = GeneratePort(dialogueNode, Direction.Output, typeof(string));

        var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count ;
        var outputPortName = $"Choice {outputPortCount}";

        generatePort.portName = outputPortName;

        // Créer un conteneur horizontal pour le port et le bouton de suppression
        var portContainer = new VisualElement();
        portContainer.style.flexDirection = FlexDirection.Row;

        CreateDeleteChoiceButton(dialogueNode, portContainer);
        
        // Ajouter le port au conteneur
        portContainer.Add(generatePort);

        // Conteneur au node
        dialogueNode.outputContainer.Add(portContainer);


        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }
    private void CreateDeleteChoiceButton(DialogueNode dialogueNode, VisualElement portContainer)
    {
        // Créer le bouton de suppression
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

            // Rafraîchir les ports et l'état du nœud
            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();
        })
        {
            text = "X" // Texte ou icône du bouton
        };

        // style bouton pour qu'il soit petit
        deleteButton.style.marginLeft = 4;
        deleteButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
        deleteButton.style.color = Color.white;
        deleteButton.style.width = 20;

        // bouton au conteneur
        portContainer.Add(deleteButton);
    }
    #endregion

    #region Add Button / Fields to node
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
            value = dialogueNode.DialogueText, // Default to the current GUID
            name = "Dialogue ID"
        };

        // Synchroniser la valeur du champ avec le DialogueText du noeud
        idField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueText = evt.newValue; // Mettre à jour le DialogueText
        });

        dialogueNode.mainContainer.Add(idField);
    }

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
                        .ToList() // Sauvegarder les noms des ports de sortie
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
                fromPortId = edge.output.name, // Sauvegarder l'identifiant unique du port

                fromPortIndex = edge.output.parent.IndexOf(edge.output),

                toNodeId = ((DialogueNode)edge.input.node).GIUD,
                toPortId = edge.input.name, // Sauvegarder l'identifiant unique du port

                toPortIndex = edge.input.parent.IndexOf(edge.input),
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
                node = CreateDialogueNode(nodeData.title, nodeData.position, nodeData.outputPorts);
            }
            node.GIUD = nodeData.id;
            node.DialogueText = nodeData.dialogueId; // Custom ID

            // Mettre à jour visuellement le TextField
            var idField = node.mainContainer.Q<TextField>("Dialogue ID");
            if (idField != null)
            {
                idField.value = node.DialogueText;
            }

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
