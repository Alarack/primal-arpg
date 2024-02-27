using LL.Events;
using LL.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriggerInstance = AbilityTrigger.TriggerInstance;

public class AIBrain : MonoBehaviour {


    public List<StateChangerData> stateChangeData = new List<StateChangerData>();

    [Header("State Data")]
    public List<StateData> stateData = new List<StateData>();

    public AISensor Sensor { get; private set; }
    public NPCMovement Movement { get; private set; }
    public WeaponManager WeaponManager { get; private set; }
    public NPC Owner { get; private set; }

    public TriggerInstance LastStateTrigger { get; private set; }

    public string CurrentStateName { get { return fsm.CurrentState.stateName; } }

    public List<AbilityDefinition> abilityDefinitions = new List<AbilityDefinition>();

    private FSM fsm;
    private List<StateChanger> stateChangers = new List<StateChanger>();

    private List<Ability> abilities = new List<Ability>();

    private Dictionary<StateBehaviour, List<Ability>> stateBehaviorAbilities = new Dictionary<StateBehaviour, List<Ability>>();

    private void Awake() {
        Owner = GetComponent<NPC>();
        Movement = GetComponent<NPCMovement>();
        WeaponManager = GetComponent<WeaponManager>();
        Sensor = GetComponentInChildren<AISensor>();
    }

    private void OnDisable() {
        TearDownStateChangers();
    }

    private void Start() {
        Sensor.Initialize(Owner, this);

        fsm = new FSM(Owner, stateData);
        CreateStateChangers();

        CreateAbilities();

        //if(RoomManager.CurrentDifficulty > 5) {

        //    float roll = Random.Range(0f, 1f);
            
        //    if(roll >= 0.5f)
        //        Owner.BecomeElite(AffixDatabase.EliteAffixType.Overcharged);
        //}
    }

    private void CreateAbilities() {
        for (int i = 0; i < abilityDefinitions.Count; i++) {
            AddAbility(abilityDefinitions[i]);
            
            //Ability ability = AbilityFactory.CreateAbility(abilityDefinitions[i].AbilityData, Owner);
            //ability.Equip();
            //abilities.Add(ability);
        }
    }

    public void AddAbility(AbilityDefinition abilityDef) {
        Ability ability = AbilityFactory.CreateAbility(abilityDef.AbilityData, Owner);
        ability.Equip();
        abilities.Add(ability);
    }

    public void AddAbility(Ability ability) {
        abilities.Add(ability);
        ability.Equip();
    }

    public void RemoveAbility(Ability ability) {
        ability.Uneqeuip();
        abilities.Remove(ability);
    }

    public void AddAbilitiesFromBehavior(List<AbilityDefinition> newAbilities, StateBehaviour behavior) {
        List<Ability> abiliitesToAdd = new List<Ability>();
        
        for (int i = 0; i < newAbilities.Count; i++) {
            Ability ability = AbilityFactory.CreateAbility(newAbilities[i].AbilityData, Owner);
            //this.abilities.Add(ability);
            abiliitesToAdd.Add(ability);
        }

        UpdateBehaviorAbilityDictioanry(abiliitesToAdd, behavior);
    }

    private void UpdateBehaviorAbilityDictioanry(List<Ability> abilitiesToAdd, StateBehaviour behavior) {
        if(stateBehaviorAbilities.ContainsKey(behavior) == true) {
            for (int i = 0; i < abilitiesToAdd.Count; i++) {
                stateBehaviorAbilities[behavior].AddUnique(abilitiesToAdd[i]);
            }
        }
        else {
            stateBehaviorAbilities.Add(behavior, abilitiesToAdd);
        }
    }

    private void CreateStateChangers() {
        for (int i = 0; i < stateChangeData.Count; i++) {
            StateChanger changer = new StateChanger(stateChangeData[i], this);
            stateChangers.Add(changer);
        }
    }

    private void TearDownStateChangers() {
        for (int i = 0; i < stateChangers.Count; i++) {
            stateChangers[i].TearDown();
        }

        stateChangers.Clear();
    }

    private void Update() {
        if(Owner.active == false) 
            return;
        
        fsm.ManagedUpdate();
    }

    private void FixedUpdate() {
        if (Owner.active == false)
            return;

        fsm.ManagedFixedUpdate();
    }

    public void FireAllWeapons() {
        WeaponManager.FireAllWeapons();
    }

    public void ActivateAbility(AbilityData ability) {

    }

    public void TearDownAbilities() {
        for (int i = 0; i < abilities.Count; i++) {
            abilities[i].Uneqeuip();
        }

        foreach (var entry in stateBehaviorAbilities) {
            for (int i = 0; i < entry.Value.Count; i++) {
                entry.Value[i].Uneqeuip();
            }
        }
    }

    public void ActivateAllAbilities() {
        for (int i = 0; i < abilities.Count; i++) {
            abilities[i].ForceActivate();
        }
    }

    public void ActivateBehaviourAbilities(StateBehaviour behaviour) {
        if(stateBehaviorAbilities.TryGetValue(behaviour, out List<Ability> behaviorAbilities) == true) {
            for (int i = 0; i < behaviorAbilities.Count; i++) {
                //behaviorAbilities[i].ForceActivate();
                SendAIActivatedEvent(behaviorAbilities[i]);
            }
        }
    }

    private void SendAIActivatedEvent(Ability ability) {
        EventData data = new EventData();
        data.AddAbility("Ability", ability);

        EventManager.SendEvent(GameEvent.AIActivated, data);
    }

    public void EquipBehaviourAbilities(StateBehaviour behaviour) {
        if (stateBehaviorAbilities.TryGetValue(behaviour, out List<Ability> behaviorAbilities) == true) {
            for (int i = 0; i < behaviorAbilities.Count; i++) {
                behaviorAbilities[i].Equip();
            }
        }
    }

    public void UnequipBehaviourAbilities(StateBehaviour behaviour) {
        if (stateBehaviorAbilities.TryGetValue(behaviour, out List<Ability> behaviorAbilities) == true) {
            for (int i = 0; i < behaviorAbilities.Count; i++) {
                behaviorAbilities[i].Uneqeuip();
            }
        }
    }

    public bool GetLatestSensorTarget() {
        if (Sensor.LatestTarget == null) {
            Movement.SetTarget(null);
            return false;
        }

        Movement.SetTarget(Sensor.LatestTarget.transform);
        return true;
    }


    #region AI TRIGGER BUSINESS

    public void ReceiveStateChange(string stateName, TriggerInstance triggerInstance) {
        LastStateTrigger = triggerInstance;

        //Debug.LogWarning("Brain is recieveing a state change Trigger: " + triggerInstance.GetType());

        fsm.ChangeState(stateName);
    }

    public void ForceStateChange(string stateName) {
        fsm.ChangeState(stateName);
    }



    #endregion


}
