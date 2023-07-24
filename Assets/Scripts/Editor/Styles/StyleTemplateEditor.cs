using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StyleTemplate))]
public class StyleTemplateEditor : Editor
{
    private StyleTemplate style;



    private void OnEnable() {
        style = target as StyleTemplate;
    }

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();

        if (style.style == null)
            style.style = new GUIStyle();

        EditorGUILayout.LabelField("Example Style", style.style);
        EditorGUILayout.Separator();

        style.fontColor = EditorGUILayout.ColorField("Font Color", style.fontColor);
        style.fontSize = EditorGUILayout.IntField("Font Size", style.fontSize);
        style.bold = EditorGUILayout.Toggle("Bold", style.bold);


        if (GUI.changed) {

            style.style.normal.textColor = style.fontColor;
            style.style.fontSize = style.fontSize;
            style.style.fontStyle = style.bold == true ? FontStyle.Bold : FontStyle.Normal;


            EditorUtility.SetDirty(target);
        }
    }
}
