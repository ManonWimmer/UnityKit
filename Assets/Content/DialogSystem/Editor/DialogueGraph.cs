using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;

    private Label _currentSaveLabel;


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
        var toolbarContainer = new VisualElement();
        InitStyleToolbarContainer(toolbarContainer);

        _currentSaveLabel = new Label("No Save Loaded");
        toolbarContainer.Add(_currentSaveLabel);

        GenerateCreateNodeButton(toolbarContainer);

        GenerateLoadDialogueButton(toolbarContainer);

        GenerateSaveDialogueButton(toolbarContainer);

        GenerateClearGraphButton(toolbarContainer);

        rootVisualElement.Add(toolbarContainer);
    }
    private void InitStyleToolbarContainer(VisualElement toolbarContainer)
    {
        toolbarContainer.style.flexDirection = FlexDirection.Row;
        toolbarContainer.style.flexWrap = Wrap.Wrap; // Wrap sur plusieurs lignes
        toolbarContainer.style.paddingTop = 4;
        toolbarContainer.style.paddingBottom = 4;
        toolbarContainer.style.justifyContent = Justify.SpaceBetween;
        toolbarContainer.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    }

    private void GenerateCreateNodeButton(VisualElement toolbar)
    {
        var nodeCreateButton = new Button(() =>
        {
            _graphView.CreateNode("Dialogue Node", Vector2.zero);
        });
        nodeCreateButton.text = "Create dialoge Node";
        toolbar.Add(nodeCreateButton);

        rootVisualElement.Add(toolbar);
    }
    private void GenerateLoadDialogueButton(VisualElement toolbar)
    {
        var dialogueLoadButton = new Button(() =>
        {

            var absolutePath = EditorUtility.OpenFilePanel("Load Dialogue Graph", Application.dataPath, "asset");

            if (!string.IsNullOrEmpty(absolutePath))
            {
                var relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                var graphSO = AssetDatabase.LoadAssetAtPath<DialogueGraphSO>(relativePath);
                if (graphSO != null)
                {
                    _graphView.LoadGraph(graphSO);

                    _currentSaveLabel.text = $"Loaded : {Path.GetFileName(relativePath)}";
                }
            }
        });
        dialogueLoadButton.text = "Load Dialogue Graph";
        toolbar.Add(dialogueLoadButton);

        rootVisualElement.Add(toolbar);
    }
    private void GenerateSaveDialogueButton(VisualElement toolbar)
    {
        var dialogueSaveButton = new Button(() =>
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Dialogue Graph", "NewDialogueGraph", "asset", "Save Dialogue Graph");
            if (!string.IsNullOrEmpty(path))
            {
                _graphView.SaveGraph(path);
            }
        });
        dialogueSaveButton.text = "Save Dialogue Graph";
        toolbar.Add(dialogueSaveButton);

        rootVisualElement.Add(toolbar);
    }

    private void GenerateClearGraphButton(VisualElement toolbar)
    {
        var dialogueClearGraphButton = new Button(() =>
        {
            _graphView.ClearGraph();

            _currentSaveLabel.text = "No Save Loaded";
        });
        dialogueClearGraphButton.text = "Clear all graph Graph";
        toolbar.Add(dialogueClearGraphButton);

        rootVisualElement.Add(toolbar);
    }
    #endregion

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

}
