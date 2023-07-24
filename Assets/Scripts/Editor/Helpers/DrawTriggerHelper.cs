using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static log4net.Appender.ColoredConsoleAppender;
using LL.FSM;

public static class DrawTriggerHelper 
{

    #region GUI BUTTONS

    public static void AddTriggerButton(StateChangerData data) {
        data.triggerData.Add(new TriggerData());
    }

    public static void DeleteTriggerButon(List<TriggerData> container, TriggerData triggerToRemove) {
        if (container.Contains(triggerToRemove) == true)
            container.Remove(triggerToRemove);
    }


    public static void AddFocusButton(TriggerData data) {
        data.allConstraints.Add(new ConstraintDataFocus());
    }

    public static void AddConstraintButton(ConstraintDataFocus data) {
        data.constraintData.Add(new ConstraintData());
    }

    public static void DeleteConstraint(List<ConstraintData> container, ConstraintData dataToRemove) {
        if(container.Contains(dataToRemove) == true) {
            container.Remove(dataToRemove);
        }
    }

    public static void DeleteFocus(List<ConstraintDataFocus> container, ConstraintDataFocus focusToRemove) {
        if (container.Contains(focusToRemove) == true) {
            container.Remove(focusToRemove);
        }

    }

    #endregion  

    public static TriggerData DrawTriggerData(TriggerData triggerData, List<TriggerData> container) {

        Color[] bgColors = new Color[] { new Color(0f, 0f, 0.15f, 0.15f), new Color(0f, 0.15f, 0f, 0.15f) };

        EditorGUILayout.BeginHorizontal();

        EditorHelper2.DrawBufferBar(new Color(0.7f, 0f, 0f, 0.5f), "Start of Trigger: " + ObjectNames.NicifyVariableName( triggerData.type.ToString()));


        if (GUILayout.Button("+ Focus") == true) {
            AddFocusButton(triggerData);
        }

        if (GUILayout.Button("x Trigger") == true) {
            DeleteTriggerButon(container, triggerData);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        triggerData.type = (TriggerType)EditorGUILayout.EnumPopup("Trigger Type", triggerData.type);

        if (triggerData.type == TriggerType.Timed) {
            triggerData.triggerTimerDuration = EditorGUILayout.FloatField("Duration", triggerData.triggerTimerDuration);
        }

        for (int i = 0; i < triggerData.allConstraints.Count; i++) {

            //EditorHelper.DrawBufferBar(Color.gray);

            EditorGUILayout.BeginVertical(EditorHelper2.BackgroundStyle.GetBackground(bgColors[i % 2]));

            DrawConstraintDataFocus(triggerData.allConstraints[i], triggerData);

            EditorGUILayout.EndVertical();

            //EditorHelper.DrawBufferBar(Color.gray);

        }

        EditorHelper2.DrawBufferBar(new Color(0.7f, 0f, 0f, 0.2f), "End of Trigger: " + ObjectNames.NicifyVariableName(triggerData.type.ToString()));
        //EditorHelper.DrawBufferBar(Color.black);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        return triggerData;
    }


    public static ConstraintDataFocus DrawConstraintDataFocus(ConstraintDataFocus constraintFocusData, TriggerData triggerData) {

        Color[] bgColors = new Color[] { new Color(0.15f, 0f, 0f, 0.15f), new Color(0f, 0.15f, 0f, 0.15f) };

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();

        EditorHelper2.DrawBufferBar(new Color(0.0f, 0f, 0.7f, 0.25f), "Trigger Focus: " + constraintFocusData.focus);



        if (GUILayout.Button("+ Focus Constraint") == true) {
            AddConstraintButton(constraintFocusData);
        }

        if (GUILayout.Button("x Focus") == true) {
            DeleteFocus(triggerData.allConstraints, constraintFocusData);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        constraintFocusData.focus = (ConstraintFocus)EditorGUILayout.EnumPopup("Constraint Focus", constraintFocusData.focus);
        EditorGUILayout.Separator();

        for (int i = 0; i < constraintFocusData.constraintData.Count; i++) {

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginVertical(EditorHelper2.BackgroundStyle.GetBackground(bgColors[i % 2]));
            DrawConstraintData(constraintFocusData.constraintData[i], constraintFocusData);
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel--;
        }

        //EditorHelper.DrawBufferBar(new Color(0.0f, 0f, 0.7f, 0.15f), "End of Focus: " + constraintFocusData.focus);
        EditorHelper2.DrawBufferBar(new Color(0.0f, 0f, 0.7f, 0.25f));
        EditorGUILayout.Separator();

        EditorGUI.indentLevel--;

        return constraintFocusData;
    }


    public static ConstraintData DrawConstraintData(ConstraintData constraintData, ConstraintDataFocus focusData) {

        var errorStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        errorStyle.normal.textColor = Color.red;

        EditorGUILayout.BeginHorizontal();

        EditorHelper2.DrawBufferBar(new Color(0.0f, 0.5f, 0.0f, 0.25f), focusData.focus + " Constraint: " + ObjectNames.NicifyVariableName(constraintData.type.ToString()));

        if (GUILayout.Button("x Constraint") == true) {
            DeleteConstraint(focusData.constraintData, constraintData);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        constraintData.type = (ConstraintType)EditorGUILayout.EnumPopup("Constraint Type", constraintData.type);
        constraintData.inverse = EditorGUILayout.Toggle("Inverse", constraintData.inverse);

        //EditorGUILayout.Separator();

        switch (constraintData.type) {
            case ConstraintType.StatChanged:
                constraintData.statChangeTarget = (StatName)EditorGUILayout.EnumPopup("Target Stat", constraintData.statChangeTarget);
                constraintData.changeDirection = (GainedOrLost)EditorGUILayout.EnumPopup("Gained or Lost", constraintData.changeDirection);
                break;

            case ConstraintType.Range:

                constraintData.rangeToWhat = (EffectTarget)EditorGUILayout.EnumPopup("Range to What", constraintData.rangeToWhat);
                constraintData.minRange = EditorGUILayout.FloatField("Min Range", constraintData.minRange);
                constraintData.maxRange = EditorGUILayout.FloatField("Max Range", constraintData.maxRange);
                break;

            case ConstraintType.Subtype:
                constraintData.targetSubtype = (Entity.EntitySubtype)EditorGUILayout.EnumPopup("Subtype", constraintData.targetSubtype);

                break;
            case ConstraintType.PrimaryType:
                constraintData.targetPrimaryType = (Entity.EntityType)EditorGUILayout.EnumPopup("Primary Type", constraintData.targetPrimaryType);

                break;
            case ConstraintType.SourceOnly:
            case ConstraintType.HasTarget:
                break;

            case ConstraintType.IsInState:
                constraintData.targetStateData = (StateData)EditorGUILayout.ObjectField("To State:", constraintData.targetStateData, typeof(StateData), false);
                break;
            case ConstraintType.HasStatus:
            case ConstraintType.Owner:
            case ConstraintType.StatMinimum:
            case ConstraintType.StatMaximum:
            case ConstraintType.EntityName:
            case ConstraintType.ParentAbilityTag:
            case ConstraintType.Collision:
            default:
                EditorGUILayout.LabelField("Not Yet Ready", errorStyle, GUILayout.ExpandWidth(true));
                break;


        }

        EditorGUILayout.Separator();

        return constraintData;
    }


}
