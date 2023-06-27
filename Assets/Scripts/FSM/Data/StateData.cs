using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL.FSM {

    [CreateAssetMenu(fileName = "State Data")]
    public class StateData : ScriptableObject {

        public bool defaultState;
        public string stateName;

        public List<StateBehaviourData> behaviourData = new List<StateBehaviourData>();

    }

}
