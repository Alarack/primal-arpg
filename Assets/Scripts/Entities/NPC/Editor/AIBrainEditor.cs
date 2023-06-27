using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LL.FSM;

[CustomEditor(typeof(AIBrain))]
public class AIBrainEditor : Editor
{

    private AIBrain aiBrain;



    #region GUI BUTTONS

    private void AddStateChangeButton() {
        aiBrain.stateChangeData.Add(new StateChangerData());
    }

    private void DeleteStateChangeButton(StateChangerData dataToRemove) {
        if(aiBrain.stateChangeData.Contains(dataToRemove) == true)
            aiBrain.stateChangeData.Remove(dataToRemove);
    }


    #endregion



    public override void OnInspectorGUI() {
        
        aiBrain = (AIBrain)target;


        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("State Data", EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add State") == true) {
            aiBrain.stateData.Add(null);
        }


        for (int i = 0; i < aiBrain.stateData.Count; i++) {
            aiBrain.stateData[i] = (StateData)EditorGUILayout.ObjectField("State", aiBrain.stateData[i], typeof(StateData), false);
        }

        if (aiBrain.stateData.Count > 0) {
            if (GUILayout.Button("Remove State") == true) {
                aiBrain.stateData.RemoveAt(aiBrain.stateData.Count - 1);
            }
        }


        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("State Changers", EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add State Change") == true) {
            AddStateChangeButton();
        }

        for (int i = 0; i < aiBrain.stateChangeData.Count; i++) {
            DrawStateChangeData(aiBrain.stateChangeData[i]);
            EditorGUILayout.Separator();
        }







        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }

        //base.OnInspectorGUI();
    }


    private void DrawStateData(StateData data) {

    }

    private void DrawStateChangeData(StateChangerData data) {

        EditorGUILayout.Separator();

        string stateTitle = data.toStateData != null ? data.toStateData.stateName : "";

        EditorGUILayout.LabelField("State Change: " + stateTitle, EditorStyles.boldLabel);
        EditorGUILayout.Separator();


        if (GUILayout.Button("Remove Changer") == true) {
            DeleteStateChangeButton(data);
        }


        data.fromStateData = (StateData)EditorGUILayout.ObjectField("From State:", data.fromStateData, typeof(StateData), false);


        //EditorGUILayout.BeginHorizontal();


        data.toStateData = (StateData)EditorGUILayout.ObjectField("To State:", data.toStateData, typeof(StateData), false);


        //EditorGUILayout.EndHorizontal();

        if(GUILayout.Button("Add Trigger") == true) {
            DrawTriggerHelper.AddTriggerButton(data);
        }


        for (int i = 0; i < data.triggerData.Count; i++) {
            DrawTriggerHelper.DrawTriggerData(data.triggerData[i], data.triggerData);
        }

    }

}
