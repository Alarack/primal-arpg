using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using static AbilityTrigger;

[System.Serializable]
public class StateChanger {



    private AIBrain brain;
    private List<AbilityTrigger> triggers = new List<AbilityTrigger>();
    private StateChangerData changerData;

    public StateChanger(StateChangerData data, AIBrain brain) {
        this.changerData = data;
        this.brain = brain;
        CreateTriggers();

    }

    private void CreateTriggers() {
        for (int i = 0; i < changerData.triggerData.Count; i++) {
            AbilityTrigger newTrigger = AbilityFactory.CreateAbilityTrigger(changerData.triggerData[i], brain.Owner);
            newTrigger.ActivationCallback = ReceiveStateTrigger;
            triggers.Add(newTrigger);
        }
    }

    public void TearDown() {
        for (int i = 0; i < triggers.Count; i++) {
            triggers[i].TearDown();
        }
    }


    private void ReceiveStateTrigger(TriggerInstance instance) {


        if(changerData.fromStateData != null) {
            if (brain.CurrentStateName != changerData.fromStateData.stateName) {
                //Debug.LogWarning("Can't change due to incorrect from state: " + brain.CurrentStateName + " : " + changerData.fromStateData.stateName);
                
                return;
            }
        }


        brain.ReceiveStateChange(changerData.toStateData.stateName, instance);
    }


}
