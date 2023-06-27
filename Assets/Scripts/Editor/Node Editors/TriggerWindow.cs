using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TriggerWindow : EditorWindow
{

    //public TriggerData triggerData;
    private Vector2 scrollPosition;

    public static StateChangerData currentData;

    [MenuItem("Window/Trigger Editor")]
    private static void OpenWindow() {
        TriggerWindow window = GetWindow<TriggerWindow>();
        window.titleContent = new GUIContent("Trigger Editor");
    }

    private void OnEnable() {
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnGUI() {

        EditorGUILayout.BeginVertical();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (currentData != null) {
            //currentData.desiredState = EditorGUILayout.TextField("Transition State", currentData.desiredState);

            ShowTriggerData(currentData.triggerData);
        }


        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();


        if (GUI.changed == true)
            Repaint();
    }

    private void OnInspectorUpdate() {
        Repaint();
    }

    private void OnSelectionChanged() {
        if(Selection.activeGameObject == null) {
            currentData = null;
        }
    }

    public static void ShowTriggerData(List<TriggerData> container) {

        for (int i = 0; i < container.Count; i++) {

            //EditorGUILayout.BeginVertical(EditorHelper.BackgroundStyle.GetBackground(Color.black));
            //EditorGUILayout.Separator();
            //EditorGUILayout.EndVertical();

            

            DrawTriggerHelper.DrawTriggerData(container[i], container);

            //EditorHelper.DrawBufferBar(Color.black);
            
            
            //EditorHelper.DrawBufferBar(new Color(0.7f, 0f, 0f, 0.35f), "End of Trigger");
            
            
            
            //EditorHelper.DrawBufferBar(Color.black);


            //EditorGUILayout.Separator();
            
            
            
            
            //EditorHelper.DrawBufferBar(Color.black);

            //EditorGUILayout.BeginVertical(EditorHelper.BackgroundStyle.GetBackground(Color.black));
            //EditorGUILayout.Separator();
            //EditorGUILayout.EndVertical();

        }


    }
}
