using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using UnityEditor;

public class DialogueGraphView : GraphView
{

    private readonly Vector2 _defaultNodeSize = new Vector2(150, 200);

    private Vector2 _lastMousePosition;

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


        AddElement(GenererateEntryPointNode());

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

        // You can add more menu options here
        evt.menu.AppendAction("Another Option", action =>
        {
            Debug.Log("Another Option Clicked");
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

    private DialogueNode GenererateEntryPointNode()
    {
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

        node.SetPosition(new Rect(100, 200, 100, 150));

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
    #endregion


}
