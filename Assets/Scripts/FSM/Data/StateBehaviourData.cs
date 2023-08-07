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
        Strafe
    }


    [CreateAssetMenu(fileName = "State Behaviour Data")]
    public class StateBehaviourData : ScriptableObject {
        public ExecutionMode mode;
        public StateBehaviourType behavourType;


        //Pursue
        public float chaseDistance;

        //Flee
        public float fleeDistance;

        //Wander
        public float wanderIdleTime;
        public float wanderMaxDistance;

        //Wait
        public float waitTime;

        //Spawn Object
        public GameObject spawn;
        public Vector2 spawnOffset;

        //AntiFlock
        public float minFlockDistance;

        //Ability Container
        public List<AbilityDefinition> abilities = new List<AbilityDefinition>();

    }

}