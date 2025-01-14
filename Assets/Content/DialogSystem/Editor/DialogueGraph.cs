using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();

        window.titleContent = new GUIContent("DialogueGraph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    #region Generate Toolbar / Buttons
    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        GenerateCreateNodeButton(toolbar);

        GenerateLoadDialogueButton(toolbar);

        GenerateSaveDialogueButton(toolbar);
    }

    private void GenerateCreateNodeButton(Toolbar toolbar)
    {
        var nodeCreateButton = new Button(() =>
        {
            _graphView.CreateNode("Dialogue Node", Vector2.zero);
        });
        nodeCreateButton.text = "Create dialoge Node";
        toolbar.Add(nodeCreateButton);

        rootVisualElement.Add(toolbar);
    }
    private void GenerateLoadDialogueButton(Toolbar toolbar)
    {
        var dialogueLoadButton = new Button(() =>
        {
            
        });
        dialogueLoadButton.text = "Load Dialogue";
        toolbar.Add(dialogueLoadButton);

        rootVisualElement.Add(toolbar);
    }
    private void GenerateSaveDialogueButton(Toolbar toolbar)
    {
        var dialogueSaveButton = new Button(() =>
        {
            
        });
        dialogueSaveButton.text = "Save Dialogue";
        toolbar.Add(dialogueSaveButton);

        rootVisualElement.Add(toolbar);
    }
    #endregion

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

}
