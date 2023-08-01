using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;

namespace LL.FSM {

    public class State {

        public bool defaultState;
        public string stateName;
        private Dictionary<ExecutionMode, List<StateBehaviour>> behaviourDict = new Dictionary<ExecutionMode, List<StateBehaviour>>();

        private AIBrain ownerBrain;

        public State(StateData data, AIBrain brain, AISensor sensor) {
            this.defaultState = data.defaultState;
            this.stateName = data.stateName;
            this.ownerBrain = brain;

            CreateBehaviours(data, brain, sensor);
        }

        private void CreateBehaviours(StateData data, AIBrain brain, AISensor sensor) {
            for (int i = 0; i < data.behaviourData.Count; i++) {

                List<StateBehaviour> behaviours = new List<StateBehaviour>();

                StateBehaviour behaviour = StateFactory.CreateStateBehaviour(data.behaviourData[i], brain, sensor);
                behaviours.Add(behaviour);

                if (behaviourDict.TryGetValue(data.behaviourData[i].mode, out List<StateBehaviour> existingBehaviours) == true) {
                    existingBehaviours.Add(behaviour);
                }
                else {
                    behaviourDict.Add(data.behaviourData[i].mode, behaviours);
                }

            }
        }

        public void ManagedUpdate() {
            foreach (var item in behaviourDict.Values) {
                for (int i = 0; i < item.Count; i++) {
                    item[i].ManagedUpdate();
                }
            }

            OnUpdate();
        }

        private void EnterBehavior() {
            foreach (var item in behaviourDict.Values) {
                for (int i = 0; i < item.Count; i++) {
                    item[i].OnEnter();
                }
            }
        }

        private void ExitBehavior() {
            foreach (var item in behaviourDict.Values) {
                for (int i = 0; i < item.Count; i++) {
                    item[i].OnExit();
                }
            }
        }

        public void OnEnter() {

            EnterBehavior();

            EventData data = new EventData();
            data.AddEntity("Target", ownerBrain.Owner);
            data.AddString("State", stateName);
            EventManager.SendEvent(GameEvent.StateEntered, data);

            ExecuteBehaviours(ExecutionMode.Enter);
        }

        public void OnExit() {
            ExitBehavior();

            ExecuteBehaviours(ExecutionMode.Exit);
        }

        public void OnUpdate() {
            ExecuteBehaviours(ExecutionMode.Update);
        }

        public void OnFixedUpdate() {
            ExecuteBehaviours(ExecutionMode.FixedUpdate);
        }

        private void EquipStateAbilities() {

        }

        private void UnequipStateAbilities() {

        }


        private List<StateBehaviour> GetStateBehaviours(ExecutionMode mode) {
            if (behaviourDict.TryGetValue(mode, out List<StateBehaviour> results)) {
                return results;
            }

            return null;
        }


        private void ExecuteBehaviours(ExecutionMode mode) {
            List<StateBehaviour> behaviours = GetStateBehaviours(mode);

            if (behaviours == null)
                return;

            for (int i = 0; i < behaviours.Count; i++) {
                behaviours[i].Execute();
            }
        }


    }

}
