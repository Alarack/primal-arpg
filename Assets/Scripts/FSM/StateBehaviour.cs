using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LL.FSM {

    public enum ExecutionMode {
        Enter,
        Exit,
        Update,
        FixedUpdate
    }



    public abstract class StateBehaviour {


        public StateBehaviourData Data { get; protected set; }
        public abstract StateBehaviourType Type { get; }
        public ExecutionMode Mode { get; protected set; }


        protected AIBrain brain;
        protected AISensor sensor;

        public StateBehaviour(StateBehaviourData data, AIBrain brain, AISensor sensor) {
            this.brain = brain;
            this.sensor = sensor;
            this.Mode = data.mode;
            this.Data = data;
        }


        public virtual void ManagedUpdate() {

        }

        public virtual void OnEnter() {

        }

        public virtual void OnExit() { 
        
        }

        public abstract void Execute();


    }

    public class AntiFlockBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.Flee;

        private LayerMask mask;
        private Collider2D[] nearbyEntities;

        public AntiFlockBehaviour(StateBehaviourData data, AIBrain brain, AISensor seonsor) : base(data, brain, seonsor) {

            mask = LayerTools.AddToMask(mask, brain.Owner.gameObject.layer);
        }

        public override void ManagedUpdate() {
            base.ManagedUpdate();

            nearbyEntities = Physics2D.OverlapCircleAll(brain.Owner.transform.position, 10f, mask);
        }

        public override void Execute() {

            if (nearbyEntities == null || nearbyEntities.Length < 1) {
                //Debug.Log("Nothing nearby");
                return;
            }
              

            for (int i = 0; i < nearbyEntities.Length; i++) {

                if (nearbyEntities[i] == null) {
                    continue;
                }
                
                float distance = Vector2.Distance(brain.Owner.transform.position, nearbyEntities[i].transform.position);

                if(distance < Data.minFlockDistance) {
                    brain.Movement.MoveAwayFromPoint(nearbyEntities[i].transform.position, 0.5f);
                }

            }
        }
    }

    public class StrafeBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.Strafe;

        private bool hasTarget;

        public StrafeBehaviour(StateBehaviourData data, AIBrain brain, AISensor seonsor) : base(data, brain, seonsor) {

        }

        public override void ManagedUpdate() {
            base.ManagedUpdate();

            hasTarget = brain.GetLatestSensorTarget();
        }

        public override void Execute() {

            if (hasTarget == false)
                return;

            
                brain.Movement.StrafeTarget();
        }
    }

    public class FleeBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.Flee;

        private bool hasTarget;

        public FleeBehaviour(StateBehaviourData data, AIBrain brain, AISensor seonsor) : base(data, brain, seonsor) {

        }

        public override void ManagedUpdate() {
            base.ManagedUpdate();

            hasTarget = brain.GetLatestSensorTarget();
        }

        public override void Execute() {

            if (hasTarget == false)
                return;

            float distance = sensor.GetDistanceToTarget();

            if (distance < 0f)
                return;

            if (distance < Data.fleeDistance)
                brain.Movement.MoveAwayFromTarget();
        }
    }

    public class ChaseBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.Chase;

        private bool hasTarget;

        public ChaseBehaviour(StateBehaviourData data, AIBrain brain, AISensor seonsor) : base(data, brain, seonsor) {

        }

        public override void ManagedUpdate() {
            base.ManagedUpdate();

            hasTarget = brain.GetLatestSensorTarget();
        }

        public override void Execute() {

            if (hasTarget == false)
                return;

            float distance = sensor.GetDistanceToTarget();

            if (distance < 0f)
                return;

            if (distance > Data.chaseDistance)
                brain.Movement.MoveTowardTarget();
        }
    }

    public class RotateTowardTargetBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.RotateToward;
        private bool hasTarget;

        public RotateTowardTargetBehaviour(StateBehaviourData data, AIBrain brain, AISensor sensor) : base(data, brain, sensor) {

        }

        public override void ManagedUpdate() {
            base.ManagedUpdate();

            hasTarget = brain.GetLatestSensorTarget();
        }

        public override void Execute() {
            brain.Movement.RotateTowardTarget();
        }
    }

    public class AttackBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.Attack;

        public AttackBehaviour(StateBehaviourData data, AIBrain brain, AISensor sensor) : base(data, brain, sensor) {

        }

        public override void Execute() {
            brain.FireAllWeapons();
        }
    }

    public class AbilityBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.AbilityContainer;

        private bool hasTarget;

        //private List<Ability> activeAbilities = new List<Ability>();

        public AbilityBehaviour(StateBehaviourData data, AIBrain brain, AISensor sensor) : base(data, brain, sensor) {
            AddAbiliites();
        }

        private void AddAbiliites() {
            brain.AddAbilitiesFromBehavior(Data.abilities, this);
        }

        public override void ManagedUpdate() {
            base.ManagedUpdate();

            hasTarget = brain.GetLatestSensorTarget();
        }

        public override void OnEnter() {
            brain.EquipBehaviourAbilities(this);
        }

        public override void OnExit() {
            brain.UnequipBehaviourAbilities(this);
        }

        public override void Execute() {

            if (hasTarget == false)
                return;

            brain.ActivateBehaviourAbilities(this);
        }
    }

    public class SpawnObjectBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.SpawnObject;

        public SpawnObjectBehaviour(StateBehaviourData data, AIBrain brain, AISensor sensor) : base(data, brain, sensor) {

        }

        public override void Execute() {
            Vector3 brainPos = brain.transform.position;
            Vector3 offset = Data.spawnOffset;

            Vector3 newPos = brainPos + offset;

            GameObject.Instantiate(Data.spawn, newPos, Quaternion.identity);
        }
    }

    public class WaitBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.Wait;

        public Timer waitTimer;

        public WaitBehaviour(StateBehaviourData data, AIBrain brain, AISensor sensor) : base(data, brain, sensor) {

            EventData timerEventData = new EventData();
            timerEventData.AddEntity("Owner", brain.Owner);
            waitTimer = new Timer(data.waitTime, OnTimerComplete, true, timerEventData);
        
        }

        public override void ManagedUpdate() {
            base.ManagedUpdate();

            if (waitTimer != null)
                waitTimer.UpdateClock();
        }


        public override void Execute() {

        }

        private void OnTimerComplete(EventData data) {
            EventManager.SendEvent(GameEvent.TriggerTimerCompleted, data);
        }
    }

    public class WanderBehaviour : StateBehaviour {

        public override StateBehaviourType Type => StateBehaviourType.Wander;

        private Timer wanderTimer;
        private bool wandering = false;
        private bool waiting = false;
        private bool hasTarget;
        private Vector2 wanderPoint;

        public WanderBehaviour(StateBehaviourData data, AIBrain brain, AISensor sensor) : base(data, brain, sensor) {
            wanderTimer = new Timer(data.wanderIdleTime, ResetWanderTimer, true);
        }

        public override void ManagedUpdate() {
            base.ManagedUpdate();

            hasTarget = brain.GetLatestSensorTarget();

            if (wanderTimer != null && waiting == true)
                wanderTimer.UpdateClock();
        }


        public override void Execute() {
            if (wandering == false) {
                PickDirection();
            }

            if (waiting == true)
                return;

            float distance = Vector2.Distance(brain.transform.position, wanderPoint);

            if (distance > 0.1f) {
               
                if(hasTarget == false)
                    brain.Movement.RotateTowardPoint(wanderPoint);
                 
                brain.Movement.MoveTowardPoint(wanderPoint);
            }
            else {
                waiting = true;
            }

        }

        private void PickDirection() {
            
            Vector2 startPoint = Data.leashToOrigin == false ? (Vector2)brain.transform.position : Vector2.zero;
            
            wanderPoint = startPoint + (Random.insideUnitCircle * Data.wanderMaxDistance);
            wandering = true;
        }

        private void ResetWanderTimer(EventData timerEventData) {
            wandering = false;
            waiting = false;
        }
    }

}
