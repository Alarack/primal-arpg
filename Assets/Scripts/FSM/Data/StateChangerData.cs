using LL.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateChangerData 
{
    public StateData fromStateData;
    public StateData toStateData;
    //public string desiredState;
    public List<TriggerData> triggerData = new List<TriggerData>();


    public string GetStateConstraint() {

        for (int i = 0; i < triggerData.Count; i++) {
            for (int j = 0; j < triggerData[i].allConstraints.Count; j++) {
                for (int k = 0; k < triggerData[i].allConstraints[j].constraintData.Count; k++) {
                    ConstraintData current = triggerData[i].allConstraints[j].constraintData[k];

                    if (current.type == ConstraintType.IsInState)
                        return current.targetStateData.stateName;
                }
            }
        }


        return null;
    }
}
