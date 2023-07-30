using LL.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CustomEditor(typeof(StateBehaviourData))]
public class StateBehaviourDataEditor : Editor
{

    private StateBehaviourData behaviourData;



    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();

        behaviourData = (StateBehaviourData)target;

        behaviourData.behavourType = (StateBehaviourType)EditorGUILayout.EnumPopup("Behaviour Type", behaviourData.behavourType);
        behaviourData.mode = (ExecutionMode)EditorGUILayout.EnumPopup("Execution Mode", behaviourData.mode);


        EditorGUILayout.Separator();

        switch (behaviourData.behavourType) {
            case StateBehaviourType.None:
                break;
            case StateBehaviourType.Flee:
                behaviourData.fleeDistance = EditorGUILayout.FloatField("Min Distance", behaviourData.fleeDistance);
                break;
            case StateBehaviourType.Chase:
                behaviourData.chaseDistance = EditorGUILayout.FloatField("Max Distance", behaviourData.chaseDistance);
                break;
            case StateBehaviourType.Attack:
                break;
            case StateBehaviourType.Wander:
                behaviourData.wanderMaxDistance = EditorGUILayout.FloatField("Max Distance", behaviourData.wanderMaxDistance);
                behaviourData.wanderIdleTime = EditorGUILayout.FloatField("Idle Time", behaviourData.wanderIdleTime);

                break;
            case StateBehaviourType.RotateToward:
                break;
            case StateBehaviourType.Wait:

                behaviourData.waitTime = EditorGUILayout.FloatField("Wait Time", behaviourData.waitTime);
                break;

            case StateBehaviourType.SpawnObject:
                behaviourData.spawn = (GameObject)EditorGUILayout.ObjectField("Spawn", behaviourData.spawn, typeof(GameObject), false);
                behaviourData.spawnOffset = EditorGUILayout.Vector2Field("Offset", behaviourData.spawnOffset);
                break;
            case StateBehaviourType.AbilityContainer:
                behaviourData.abilities = EditorHelper.DrawList("Abilities", behaviourData.abilities, null, AbilityEditorHelper.DrawAbilityDefinitionList);
                break;
                
        }



        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }

    }


}
