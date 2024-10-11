using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;

//using TriggerActivationInstance = AbilityTrigger.TriggerActivationInstance;
using TriggerInstance = AbilityTrigger.TriggerInstance;

public class TriggerActivationCounter 
{

    public Ability ParentAbility { get; private set; }
    public Entity Source { get; private set; }
    public int Count { get; private set; }

    private AbilityTrigger refreshTrigger;
   
    private bool useCustomRefreshTrigger;
    private bool requireMultipleTriggers;
    private bool limitNumberOfTriggers;

    private int maxTriggerCount = -1;
    private int minTriggerCount = -1;

    public TriggerActivationCounterData Data { get; private set; }

    public TriggerActivationCounter(TriggerActivationCounterData data, Entity source, Ability parentAbility = null) {
        this.ParentAbility = parentAbility;
        this.Source = source;
        this.Data = data;
        useCustomRefreshTrigger = data.useCustomRefreshTrigger;
        requireMultipleTriggers = data.requireMultipleTriggers;
        limitNumberOfTriggers = data.limitedNumberOfTriggers;
        maxTriggerCount = data.maxTriggerCount;
        minTriggerCount = data.minTriggerCount;

        if (useCustomRefreshTrigger == true)
            CreateCustomRefreshTrigger(data.customRefreshTriggerData);
    }

    public void TearDown() {
        if (refreshTrigger != null)
            refreshTrigger.TearDown();
    }

    private void CreateCustomRefreshTrigger(TriggerData data) {
        refreshTrigger = AbilityFactory.CreateAbilityTrigger(data, Source, ParentAbility);
        refreshTrigger.ActivationCallback = RefreshCount;
    }

    private void RefreshCount(TriggerInstance activationInstance = null) {
        Count = 0;
        //Debug.Log(ParentAbility.abilityName + " on "  + ParentAbility.Source.cardName + " is refreshing a counter");
    }

    public void RefreshCount() {
        RefreshCount(null);
    }

    public int GetMinTriggerCount() {
        int targetMinTriggerCount = Data.reduceTriggerCountByAbilityLevel == false ? minTriggerCount : minTriggerCount - (ParentAbility.AbilityLevel - 1);


        return targetMinTriggerCount;
    }

    public bool Evaluate() {
        Count++;

        //Debug.Log("Checking a counter: " + Count + " : " + minTriggerCount);

        SendActivationCountEvent(requireMultipleTriggers);

        //int targetMinTriggerCount = Data.reduceTriggerCountByAbilityLevel == false ? MinTriggerCount : MinTriggerCount - ParentAbility.AbilityLevel;


        if (requireMultipleTriggers == true && Count < GetMinTriggerCount())
            return false;

        if (limitNumberOfTriggers == true && Count > maxTriggerCount)
            return false;

        if (useCustomRefreshTrigger == false && requireMultipleTriggers == true) {
            RefreshCount(null);
            SendActivationCountEvent(requireMultipleTriggers);
        }



        return true;
    }

    private void SendActivationCountEvent(bool multiCount) {
        EventData data = new EventData();
        data.AddEntity("Entity", Source);
        data.AddInt("Count", Count);
        data.AddInt("RequiredCount", minTriggerCount);
        data.AddBool("MultiCount", multiCount);

        EventManager.SendEvent(GameEvent.TriggerCounterActivated, data);
    }
}
