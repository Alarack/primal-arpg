using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public static class EditorHelper2 {

    private static Dictionary<string, GUIStyle> loadedStyles = new Dictionary<string, GUIStyle>();


    public static GUIStyle LoadStyle(string name) {

        if (loadedStyles.ContainsKey(name) == true) {
            return loadedStyles[name];
        }
        else {
            GUIStyle targetStyle = Resources.Load<StyleTemplate>("Style Templates/" + name).style;

            if (targetStyle == null) {
                Debug.LogError("Couldn't Load Style: " + name);
                return null;
            }

            loadedStyles.Add(name, targetStyle);
            return targetStyle;
        }

    }

    //public static T DrawListOfEnums<T>(List<T> list, int index, string label) where T : struct, System.IFormattable, System.IConvertible {
    //    T result = EditorHelper.EnumPopup(label, list[index]);

    //    return result;
    //}


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
