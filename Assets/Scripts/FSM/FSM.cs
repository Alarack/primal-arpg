using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL.FSM {
    public class FSM  {

        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; }

        public NPC Owner { get; private set; }

        private List<State> states = new List<State>();

        public FSM(NPC owner, List<StateData> stateData) {
            this.Owner = owner;
            CreateStates(stateData);

            EnterDefaultState();
        }

        private void CreateStates(List<StateData> stateData) {
            for (int i = 0; i < stateData.Count; i++) {
                states.Add(new State(stateData[i], Owner.Brain, Owner.Brain.Sensor));
            }
        }

        private void EnterDefaultState() {
            for (int i = 0; i < states.Count; i++) {
                if (states[i].defaultState == true) {
                    ChangeState(states[i]);
                    break;
                }
            }
        }

        public void ManagedUpdate() {
            if(CurrentState != null)
                CurrentState.ManagedUpdate();
        }

        public void ManagedFixedUpdate() {
            if (CurrentState != null)
                CurrentState.OnFixedUpdate();
        }


        public void ChangeState(string stateName) {
            State targetState = GetStateByName(stateName);

            if(targetState == null) {
                Debug.LogError(Owner.gameObject.name + " could not find a state called: " + stateName);
                return;
            }

            ChangeState(targetState);
        }

        public void ChangeState(State newState) {
            if (CurrentState != null && CurrentState.stateName == newState.stateName) {
                Debug.LogError(Owner.gameObject.name + " is trying to transition to the same state it's already in: " + CurrentState.stateName);
                return;
            }


            if (CurrentState != null)
                CurrentState.OnExit();

            Debug.Log("Changing State: " + newState.stateName);

            PreviousState = CurrentState;
            CurrentState = newState;

            CurrentState.OnEnter();
        }

        public void SwapToPreviousState() {
            if (PreviousState == CurrentState)
                return;

            if (PreviousState != null && CurrentState != null) {
                ChangeState(PreviousState);
            }
        }

        private State GetStateByName(string stateName) {
            for (int i = 0; i < states.Count; i++) {
                if (states[i].stateName == stateName)
                    return states[i];
            }

            return null;
        }

    }

}
