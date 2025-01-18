using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    #region Fields
    private DialogueGraphView _graphView;

    private Label _currentSaveLabel;

    #endregion


    #region Open Window

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = CreateWindow<DialogueGraph>(typeof(DialogueGraph), typeof(SceneView));

        window.titleContent = new GUIContent("Dialogue Graph");
    }
    public static void OpenDialogueGraphWindow(Object target)   // Doube click on GraphSO alternative
    {
        var window = CreateWindow<DialogueGraph>(typeof(DialogueGraph), typeof(SceneView));

        window.titleContent = new GUIContent("Dialogue Graph");

        var tempDialogueGraphSO = (DialogueGraphSO)target;

        if (tempDialogueGraphSO != null)
        {
            window.Show();
            window.LoadGraphAutomatic(tempDialogueGraphSO);
        }
        else
        {
            Debug.LogError("The provided target is not a valid DialogueGraphSO.");
        }
    }
    private void LoadGraphAutomatic(DialogueGraphSO dialogueGraphSO)    // Auto load Graph
    {
        if (dialogueGraphSO == null)
        {
            Debug.LogError("DialogueGraphSO is null. Cannot load graph.");
            return;
        }

        // Load Graph data 
        if (_graphView == null)
        {
            Debug.LogError("Graph view is not initialized.");
            return;
        }

        _graphView.LoadGraph(dialogueGraphSO);
        _currentSaveLabel.text = $"Loaded: {dialogueGraphSO.name}";
    }
    #endregion

    #region handle Enable / Disable
    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }
    private void OnDisable()
    {
        RemoveGraphView();
    }
    #endregion

    #region Construct / Remove Graph View
    private void ConstructGraphView()   // Add a Dialogue Graph (DialogueGraphView) window to the editor
    {
        _graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }
    private void RemoveGraphView()  // Add the Dialogue Graph (DialogueGraphView) window from the editor
    {
        if (_graphView == null) return;

        rootVisualElement.Remove(_graphView);
    }
    #endregion



    #region Generate Toolbar / Buttons

    #region Toolbar
    /// <summary>
    /// Create toolbar and associated elements
    /// </summary>
    private void GenerateToolbar()  // Create toolbar and associated elements
    {
        // Spawn toolbar
        var toolbarContainer = new VisualElement();
        InitStyleToolbarContainer(toolbarContainer);


        // Save Label
        GenerateCurrentSaveLabel(toolbarContainer);

        // Create Node
        GenerateCreateNodeButton(toolbarContainer);

        // Load Dialogue
        GenerateLoadDialogueButton(toolbarContainer);

        // Save Dialogue
        GenerateSaveDialogueButton(toolbarContainer);

        // Clear Graph
        GenerateClearGraphButton(toolbarContainer);

        // Add toolbar to the window
        rootVisualElement.Add(toolbarContainer);
    }

    /// <summary>
    /// Setup toolbar style with wrapping and spacing for child elements
    /// </summary>
    private void InitStyleToolbarContainer(VisualElement toolbarContainer)  // toolbar Style
    {
        if (toolbarContainer == null) return;

        toolbarContainer.style.flexDirection = FlexDirection.Row;
        toolbarContainer.style.flexWrap = Wrap.Wrap; // Wrap on multiple lines
        toolbarContainer.style.paddingTop = 4;
        toolbarContainer.style.paddingBottom = 4;
        toolbarContainer.style.justifyContent = Justify.SpaceBetween;
        toolbarContainer.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    }
    #endregion


    #region Labels
    /// <summary>
    /// Create Save label for the toolbar
    /// </summary>
    private void GenerateCurrentSaveLabel(VisualElement toolbarContainer)   // Create Save label for the toolbar
    {
        if (toolbarContainer == null) return;

        _currentSaveLabel = new Label("No Save Loaded");
        toolbarContainer.Add(_currentSaveLabel);
    }

    /// <summary>
    /// // Set currentSaveLabel Text
    /// </summary>
    private void SetCurrentSaveLabel(string currentSaveName)    // Set currentSaveLabel text
    {
        if (_currentSaveLabel == null) return;

        _currentSaveLabel.text = currentSaveName;
    }
    #endregion

    #region Buttons
    /// <summary>
    /// Create Create Node button for the toolbar
    /// </summary>
    private void GenerateCreateNodeButton(VisualElement toolbar)    // Create Node button
    {
        var nodeCreateButton = new Button(() =>
        {
            _graphView.CreateNode("Dialogue Node", Vector2.zero);
        });
        nodeCreateButton.text = "Create dialoge Node";
        SetButtonStyle(nodeCreateButton, new Color(0.3f, 0.7f, 0.3f), Color.black);
        toolbar.Add(nodeCreateButton);

        rootVisualElement.Add(toolbar);
    }

    /// <summary>
    /// Create Load Dialogue button for the toolbar
    /// </summary>
    private void GenerateLoadDialogueButton(VisualElement toolbar)  // Load Dialogue button
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

                    SetCurrentSaveLabel($"Loaded : {Path.GetFileName(relativePath)}");
                }
            }
        });
        dialogueLoadButton.text = "Load Dialogue Graph";
        SetButtonStyle(dialogueLoadButton, Color.blue, Color.black);
        toolbar.Add(dialogueLoadButton);

        rootVisualElement.Add(toolbar);
    }

    /// <summary>
    /// Create Save Dialogue button for the toolbar
    /// </summary>
    private void GenerateSaveDialogueButton(VisualElement toolbar)  // Save Dialogue button
    {
        var dialogueSaveButton = new Button(() =>
        {
            var absolutePath = EditorUtility.SaveFilePanelInProject("Save Dialogue Graph", "NewDialogueGraph", "asset", "Save Dialogue Graph");
            if (!string.IsNullOrEmpty(absolutePath))
            {
                _graphView.SaveGraph(absolutePath);

                string relativePath = absolutePath.Substring(Application.dataPath.Length);

                SetCurrentSaveLabel($"Saved : {Path.GetFileName(relativePath)}");
            }
        });
        dialogueSaveButton.text = "Save Dialogue Graph";
        SetButtonStyle(dialogueSaveButton, Color.green, Color.black);
        toolbar.Add(dialogueSaveButton);

        rootVisualElement.Add(toolbar);
    }

    /// <summary>
    /// Create Clear Graph button for the toolbar
    /// </summary>
    private void GenerateClearGraphButton(VisualElement toolbar)    // Clear Graph button
    {
        var dialogueClearGraphButton = new Button(() =>
        {
            _graphView.ClearGraph();

            _currentSaveLabel.text = "No Save Loaded";
        });
        dialogueClearGraphButton.text = "Clear graph";
        SetButtonStyle(dialogueClearGraphButton, Color.red, Color.black);
        toolbar.Add(dialogueClearGraphButton);

        rootVisualElement.Add(toolbar);
    }

    /// <summary>
    /// Set button style
    /// </summary>
    /// <param name="button"> Button to modify style</param>
    /// <param name="backgroundColor"> Button Background Color</param>
    /// <param name="textColor"> Button Text Color</param>
    private void SetButtonStyle(Button button, Color backgroundColor, Color textColor) // buttons style
    {
        if (button == null) return;

        button.style.backgroundColor = backgroundColor;
        button.style.color = textColor;
    }
    #endregion

    #endregion
}
