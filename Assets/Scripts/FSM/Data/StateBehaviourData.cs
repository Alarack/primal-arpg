using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LL.FSM {

    public enum StateBehaviourType {
        None,
        Flee,
        Chase,
        Attack,
        Wander,
        RotateToward,
        Wait,
        SpawnObject,
        AbilityContainer,
        AntiFlock,
        Strafe,
        ChangeTargeting
    }


    [CreateAssetMenu(fileName = "State Behaviour Data")]
    public class StateBehaviourData : ScriptableObject {
        public ExecutionMode mode;
        public StateBehaviourType behavourType;


        //Pursue
        public float chaseDistance;
        public bool chaseMouse;
        public bool accelerateViaDistance;

        //Flee
        public float fleeDistance;
        public bool fleeMouse;

        //Strafe
        public float rotationSpeedModifier = 1f;
        public float changeDirectionChance;
        public float changeDirecitonFrequency;

        //Wander
        public float wanderIdleTime;
        public float wanderMaxDistance;
        public bool leashToOrigin;

        //Wait
        public float waitTime;

        //Spawn Object
        public GameObject spawn;
        public Vector2 spawnOffset;

        //AntiFlock
        public float minFlockDistance;

        //Ability Container
        public List<AbilityDefinition> abilities = new List<AbilityDefinition>();

        //Change Targeting
        public MaskTargeting newMaskTargeting;
        public bool reverseTargeting;
    }

}