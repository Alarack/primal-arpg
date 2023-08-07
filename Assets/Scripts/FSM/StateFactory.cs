using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL.FSM {
    public static class StateFactory {


        public static State CreateState(StateData data, AIBrain brain, AISensor sensor) {

            return new State(data, brain, sensor);
        }

        public static StateBehaviour CreateStateBehaviour(StateBehaviourData data, AIBrain brain, AISensor sensor) {
            StateBehaviour behaviour = null;

            behaviour = data.behavourType switch {
                StateBehaviourType.None => null,
                StateBehaviourType.Flee => new FleeBehaviour(data, brain, sensor),
                StateBehaviourType.Chase => new ChaseBehaviour(data, brain, sensor),
                StateBehaviourType.RotateToward => new RotateTowardTargetBehaviour(data, brain, sensor),
                StateBehaviourType.Wander => new WanderBehaviour(data, brain, sensor),
                StateBehaviourType.Attack => new AttackBehaviour(data, brain, sensor),
                StateBehaviourType.Wait => new WaitBehaviour(data, brain, sensor),
                StateBehaviourType.SpawnObject => new SpawnObjectBehaviour(data, brain, sensor),
                StateBehaviourType.AbilityContainer => new AbilityBehaviour(data, brain, sensor),
                StateBehaviourType.AntiFlock => new AntiFlockBehaviour(data, brain, sensor),
                StateBehaviourType.Strafe => new StrafeBehaviour(data, brain, sensor),
                _ => null,
            };

            if(behaviour == null) {
                Debug.LogError("A behaviour of type: " + data.behavourType + " has no factory method");
            }


            return behaviour;
        }
    }

}
