using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using LL.FSM;

public class Node {
    public Rect rect;
    public string title;
    public bool isDragged;
    public bool isSelected;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    //public StateData stateData;

    public StateChangerData transitionData;
    private Editor transitionEditor;

    public Action<Node> OnRemoveNode;

    public Node(Vector2 position, float width, float height,
        GUIStyle nodeStyle,
        GUIStyle selectedStyle,
        GUIStyle inPointStyle,
        GUIStyle outPointStyle,
        Action<ConnectionPoint> OnClickInPoint,
        Action<ConnectionPoint> OnClickOutPoint,
        Action<Node> OnClickRemoveNode,
        StateChangerData transitionData = null) {
        rect = new Rect(position.x, position.y, width, height);
        style = nodeStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;
        this.transitionData = transitionData;
    }

    public void Drag(Vector2 delta) {
        rect.position += delta;
    }

    public void Draw() {
        //inPoint.Draw();
        //outPoint.Draw();
        GUI.Box(rect, title, style);

        Rect nameRect = new Rect(rect.position + new Vector2(15f, 12f), new Vector2(265f, 25f));
        EditorGUIUtility.labelWidth = 100f;
        EditorGUI.LabelField(nameRect, "Transition:", transitionData.toStateData.stateName);


        if(isSelected == true)
            DrawTransitionInfo();
    }

    public void DrawTransitionInfo() {
        if (transitionData == null)
            return;

        transitionData.toStateData = (StateData)EditorGUILayout.ObjectField("Desired State:", transitionData.toStateData, typeof(StateData), false);
        //transitionData.desiredState = EditorGUILayout.TextField("Transition State", transitionData.desiredState);
        TriggerWindow.currentData = transitionData;


        GUIStyle testStyle = GetBoxStyle();

        string stateConstraint = transitionData.GetStateConstraint();

        if(stateConstraint != null) {
            Rect stateRect = new Rect(rect.position - new Vector2(130f, 0f), new Vector2(125f, 50f));

            //GUI.Box(stateRect, "FROM: " + stateConstraint, style);

            GUIContent contentTest = new GUIContent(stateConstraint, "A trigger belonging to this transition has a state constraint");

            
            //testStyle.alignment = TextAnchor.MiddleCenter;
            //testStyle.normal.textColor = Color.white;
            //testStyle.fontStyle = FontStyle.Bold;
            //testStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            //testStyle.border = new RectOffset(12, 12, 12, 12);


            GUI.Box(stateRect, contentTest, testStyle);

            //GUILayout.Box("FROM: " + stateConstraint, style);
        }


        for (int i = 0; i < transitionData.triggerData.Count; i++) {
            Rect triggerRect = new Rect(rect.position + new Vector2(300f + (125f * i), 0f), new Vector2(125f, 50f));
            GUIContent contentTest = new GUIContent(ObjectNames.NicifyVariableName(transitionData.triggerData[i].type.ToString()));
            GUI.Box(triggerRect, contentTest, testStyle);
        }

        //TriggerWindow window = EditorWindow.GetWindow<TriggerWindow>();

        //window.titleContent = new GUIContent("Test Editor");
        //window.ShowTriggerData(transitionData.triggerData);

        //window.ShowTriggerData(transitionData.triggerData);

        //Debug.LogWarning("Drawing transition stuff for: " + transitionData.desiredState);


        //for (int i = 0; i < transitionData.triggerData.Count; i++) {
        //    DrawTriggerHelper.DrawTriggerData(transitionData.triggerData[i], transitionData.triggerData);
        //}

        //Rect nameRect = new Rect(rect.position + new Vector2(15f, 12f), new Vector2(265f, 25f));
        //EditorGUIUtility.labelWidth = 100f;
        //stateData.stateName = EditorGUI.TextField(nameRect, "State Name", stateData.stateName);

    }

    private GUIStyle GetBoxStyle() {

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        style.border = new RectOffset(12, 12, 12, 12);

        return style;
    }

    public bool ProcessEvents(Event e) {

        switch (e.type) {
            case EventType.MouseDown:
                if (e.button == 0) {
                    if (rect.Contains(e.mousePosition)) {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }

                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition)) {
                    ProcessContextMenu();
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged) {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    private void ProcessContextMenu() {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Trigger"), false, OnAddTriggerClicked);
        genericMenu.AddItem(new GUIContent("Remove Transition"), false, OnClickRemoveNode);

        genericMenu.ShowAsContext();
    }

    private void OnClickRemoveNode() {
        if (OnRemoveNode != null) {
            OnRemoveNode(this);
        }
    }

    private void OnAddTriggerClicked() {
        transitionData.triggerData.Add(new TriggerData());
    }


}
