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


    private FSM fsm;
    private List<StateChanger> stateChangers = new List<StateChanger>();

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
        fsm.ManagedUpdate();
    }

    private void FixedUpdate() {
        fsm.ManagedFixedUpdate();
    }

    public void FireAllWeapons() {
        WeaponManager.FireAllWeapons();
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





    #endregion


}
