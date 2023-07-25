using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EffectDefinition))]
public class EffectDefEditor : Editor
{
    EffectDefinition def;

    private void OnEnable() {
        def = target as EffectDefinition;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();


        AbilityEditorHelper.DrawEffectData(def.effectData);


        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }
    }
}
