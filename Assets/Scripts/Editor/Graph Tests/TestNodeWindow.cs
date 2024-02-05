using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TestNodeWindow : EditorWindow
{
    [MenuItem("Window/UI Toolkit/TestGraphWindow")]
    public static void Open()
    {
        GetWindow<TestNodeWindow>("Test Graph Window EX");
       
    }

    public void CreateGUI() {
        AddGrahView();
        AddStyles();
    }

    private void AddStyles() {
        StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load("Behavior System/BehaviorStyleVariables.uss");
        rootVisualElement.styleSheets.Add(styleSheet);
    }
    private void AddGrahView() {
        BehaviourGraphView graphView = new BehaviourGraphView();
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }
}
