using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AbilityDefinition))]
public class AbilityDefEditor : Editor
{

    AbilityDefinition def;

    private void OnEnable() {
        def = target as AbilityDefinition;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();


        AbilityEditorHelper.DrawAbilityData(def.AbilityData);


        if(GUI.changed) {
            EditorUtility.SetDirty(target);
        }
    }

}
