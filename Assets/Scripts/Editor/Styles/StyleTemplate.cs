using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName ="Style Template")]
public class StyleTemplate : ScriptableObject
{
    public GUIStyle style;
    public Color fontColor = Color.white;
    public int fontSize;
    public bool bold;

}
