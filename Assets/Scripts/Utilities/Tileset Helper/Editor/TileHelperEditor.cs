using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TilesetHelper))]
public class TileHelperEditor : Editor
{
    private TilesetHelper tilesetHelper;

    private void OnEnable()
    {
        tilesetHelper = (TilesetHelper)target; 
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            tilesetHelper.CreateFloorGrid();
        }

    }

}
