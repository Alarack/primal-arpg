using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourGraphView : GraphView
{


    public BehaviourGraphView() { 
        AddManipulators();
        AddGridBackground();

        CreateNode();
        
        AddStyles();
    
    }


    private void CreateNode() {
        BehaviorNode node = new BehaviorNode();

        node.Draw();
        AddElement(node);
    }

    private void AddManipulators() {

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        
    }

    private void AddStyles() {
        StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load("Behavior System/BehaviorGraphViewStyles.uss");
        styleSheets.Add(styleSheet);
    }

    private void AddGridBackground() {
        GridBackground gridBG = new GridBackground();
        gridBG.StretchToParentSize();

        Insert(0, gridBG);
    }

}
