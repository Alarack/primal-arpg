using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StatDataGroup))]
public class StatDataGroupEditor : Editor
{
    private StatDataGroup statDataGroup;



    private GUIStyle statNameStyle;
    private GUIStyle errorStyle;
    private Color[] bgColors;


    private bool initStyle;

    private void InitStyles()
    {
        initStyle = true;

        statNameStyle = new GUIStyle(EditorStyles.boldLabel);
        errorStyle = new GUIStyle(EditorStyles.boldLabel);

        statNameStyle.normal.textColor = Color.white;
        errorStyle.normal.textColor = Color.red;

        bgColors = new Color[] { new Color(0f, 0f, 0.15f, 0.15f), new Color(0f, 0.15f, 0f, 0.15f) };
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        if (initStyle == false)
            InitStyles();

        statDataGroup = (StatDataGroup)target;

        if (GUILayout.Button("Add Stat") == true)
        {
            AddStat();
        }

        for (int i = 0; i < statDataGroup.dataList.Count; i++)
        {
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(bgColors[i % 2]));
            DrawStatData(statDataGroup.dataList[i]);
            EditorGUILayout.EndVertical();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

    }


    private void AddStat()
    {
        statDataGroup.dataList.Add(new StatData());
    }

    private void DrawStatData(StatData data)
    {
        //GUIStyle statNameStyle = new GUIStyle(EditorStyles.boldLabel);
        //statNameStyle.normal.textColor = Color.white;

        //GUIStyle errorStyle = new GUIStyle(EditorStyles.boldLabel);
        //errorStyle.normal.textColor = Color.red;

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(data.statName.ToString()), statNameStyle);

        if (GUILayout.Button("Delete", GUILayout.Width(100f)) == true)
        {
            statDataGroup.dataList.Remove(data);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 125f;

        data.statName = (StatName)EditorGUILayout.EnumPopup("Stat Name", data.statName);
        data.variant = (StatData.StatVariant)EditorGUILayout.EnumPopup("Stat Variant", data.variant);

        if (data.variant == StatData.StatVariant.Simple)
        {
            data.value = EditorGUILayout.FloatField("Starting Value: ", data.value);
        }

        if (data.variant == StatData.StatVariant.Range)
        {
            data.minValue = EditorGUILayout.FloatField("Min Value: ", data.minValue);
            data.maxValue = EditorGUILayout.FloatField("Max Value: ", data.maxValue);
            data.value = EditorGUILayout.Slider("Starting Value", data.value, data.minValue, data.maxValue);

            if (data.minValue > data.maxValue)
            {
                EditorGUILayout.LabelField("Min is higher than Max.", errorStyle);
            }

            if (data.value > data.maxValue)
            {
                EditorGUILayout.LabelField("Starting value is higher than Max.", errorStyle);
            }
        }

        if (AreStatsDuplicated(data) == true)
        {
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(data.statName.ToString()) + " is duplicated in this data set.", errorStyle);
        }

        EditorGUILayout.Separator();
    }


    private bool AreStatsDuplicated(StatData data)
    {
        for (int i = 0; i < statDataGroup.dataList.Count; i++)
        {
            StatData currentData = statDataGroup.dataList[i];

            if (currentData != data && currentData.statName == data.statName)
                return true;
        }

        return false;
    }


}


public static class BackgroundStyle
{
    private static GUIStyle style = new GUIStyle();
    private static Texture2D texture = new Texture2D(1, 1);


    public static GUIStyle Get(Color color)
    {
        //Debug.Log(color.b + " " + color.g);

        if (texture == null)
            texture = new Texture2D(1, 1);

        texture.SetPixel(0, 0, color);
        texture.Apply();
        style.normal.background = texture;
        return style;
    }
}
