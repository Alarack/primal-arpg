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
                behaviourData.chaseMouse = EditorGUILayout.Toggle("Target Mouse", behaviourData.chaseMouse);
                behaviourData.accelerateViaDistance = EditorGUILayout.Toggle("Dynamic Speed", behaviourData.accelerateViaDistance);
                break;
            case StateBehaviourType.Attack:
                break;
            case StateBehaviourType.Wander:
                behaviourData.wanderMaxDistance = EditorGUILayout.FloatField("Max Distance", behaviourData.wanderMaxDistance);
                behaviourData.wanderIdleTime = EditorGUILayout.FloatField("Idle Time", behaviourData.wanderIdleTime);
                behaviourData.leashToOrigin = EditorGUILayout.Toggle("Leash Origin", behaviourData.leashToOrigin);

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
            case StateBehaviourType.AntiFlock:
                behaviourData.minFlockDistance = EditorGUILayout.FloatField("Min Distance", behaviourData.minFlockDistance);
                break;

            case StateBehaviourType.Strafe:
                behaviourData.rotationSpeedModifier = EditorGUILayout.FloatField("Rotate Mod", behaviourData.rotationSpeedModifier);
                behaviourData.changeDirectionChance = EditorGUILayout.FloatField("Change Dir Chance", behaviourData.changeDirectionChance);
                behaviourData.changeDirecitonFrequency = EditorGUILayout.FloatField("Change Dir Frequency", behaviourData.changeDirecitonFrequency);

                break;

            case StateBehaviourType.ChangeTargeting:
                behaviourData.reverseTargeting = EditorGUILayout.Toggle("Reverse", behaviourData.reverseTargeting);

                if(behaviourData.reverseTargeting == false) {
                    behaviourData.newMaskTargeting = EditorHelper.EnumPopup("Targeting", behaviourData.newMaskTargeting);
                }



                break;
                
        }



        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }

    }


}
