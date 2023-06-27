using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorHelper {




    public static void DrawBufferBar(Color color) {
        EditorGUILayout.BeginVertical(BackgroundStyle.GetBackground(color));
        EditorGUILayout.Separator();
        EditorGUILayout.EndVertical();
    }

    public static void DrawBufferBar(Color color, string label) {
        EditorGUILayout.BeginVertical(BackgroundStyle.GetBackground(color));
        //EditorGUILayout.Separator();
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        //EditorGUILayout.Separator();
        EditorGUILayout.EndVertical();
    }


    public static class BackgroundStyle {
        private static GUIStyle style = new GUIStyle();
        private static Texture2D texture = new Texture2D(1, 1);


        public static GUIStyle GetBackground(Color color) {

            if (texture == null)
                texture = new Texture2D(1, 1);

            texture.SetPixel(1, 1, color);
            texture.Apply();
            style.normal.background = texture;

            return style;
        }

    }




}
