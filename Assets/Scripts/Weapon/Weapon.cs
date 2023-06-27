using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using LL.Events;

public class Weapon : MonoBehaviour {



    [Header("Projectile Fields")]
    public Transform spawnLocation;
    public WeaponData weaponData;
    public LayerMask collisionMask;

    public StatCollection Stats { get; private set; }


    private Timer weaponCooldownTimer;
    public float CooldownRatio { get { return weaponCooldownTimer.Ratio; } }
    public bool CanAttack { get; private set; } = true;

    private Entity owner;

    private List<Effect> onHitEffects = new List<Effect>();


    private LineOfSight lineOfSight;

    private void Awake() {
        lineOfSight = GetComponentInChildren<LineOfSight>();
    }


    private void Update() {
        UpdateCooldown();


        if (owner.entityType != Entity.EntityType.Player) {
            return;
        }

        if (Input.GetKeyDown(weaponData.keyBinding) == true) {
            ManualFire();
        }

    }

    public void ManualFire() {

        if (CanAttack == false)
            return;

        if (weaponData.payload == null)
            return;

        StartCoroutine(FireWithDelay());
    }

    public void Setup() {
        owner = GetComponentInParent<Entity>();


        if (weaponData == null) {
            Debug.LogError("A weapon: " + gameObject.name + " has null weapon data. You probably forgot to assign it in the inspector.");
            return;
        }

        Stats = new StatCollection(this, weaponData.statData);

        SetupTimers();
        SetupEffects();

        //AddMeToHotbar();

    }

    //private void AddMeToHotbar() {
    //    if (owner.entityType != Entity.EntityType.Player)
    //        return;

    //    HotbarPanel hotbar = FindObjectOfType<HotbarPanel>();

    //    if (hotbar != null) {
    //        hotbar.AddEntry(this);
    //    }

    //}

    private void SetupTimers() {
        weaponCooldownTimer = new Timer(Stats[StatName.Cooldown], ResetCooldown, true);
    }

    private void SetupEffects() {
        for (int i = 0; i < weaponData.effectData.Count; i++) {
            Effect newEffect = AbilityFactory.CreateEffect(weaponData.effectData[i], owner);
            onHitEffects.Add(newEffect);
        }
    }


    private bool RangeCheck() {
        if (lineOfSight != null) {
            if (lineOfSight.Hit == false)
                return false;
        }
        else {
            return true;
        }

        float weaponRange = Stats[StatName.DetectionRange];

        if (weaponRange <= 0f)
            return true;

        if (owner is NPC) {
            NPC npc = (NPC)owner;

            Entity target = npc.Brain.Sensor.LatestTarget;

            if (target != null) {
                float distance = Vector2.Distance(owner.transform.position, target.transform.position);

                if (distance > weaponRange)
                    return false;
            }
        }

        return true;
    }

    public IEnumerator FireWithDelay() {

        if (RangeCheck() == false)
            yield break;

        WaitForSeconds waiter = new WaitForSeconds(Stats[StatName.FireDelay]);

        for (int i = 0; i < Stats[StatName.ShotCount]; i++) {
            Fire();
            yield return waiter;
        }

        EventData data = new EventData();
        data.AddEntity("Owner", owner);
        data.AddWeapon("Weapon", this);

        EventManager.SendEvent(GameEvent.WeaponCooldownStarted, data);
    }

    public void ModifiyWeaponCooldown(float amount) {
        weaponCooldownTimer.ModifyDuration(amount);
    }

    private void UpdateCooldown() {
        if (weaponCooldownTimer != null && CanAttack == false) {
            weaponCooldownTimer.UpdateClock();
        }
    }

    private void ResetCooldown(EventData timerEventData) {
        CanAttack = true;

        EventData data = new EventData();

        data.AddEntity("Owner", owner);
        data.AddWeapon("Weapon", this);

        EventManager.SendEvent(GameEvent.WeaponCooldownFinished, data);
    }

    private void OverrideCooldown() {
        CanAttack = true;
        weaponCooldownTimer.ResetTimer();
    }

    public void Fire() {
        Projectile activeProjectile = Instantiate(weaponData.payload, spawnLocation.position, spawnLocation.rotation);

        activeProjectile.Setup(owner, this, onHitEffects);

        float inaccuracy = (1f - Stats[StatName.Accuracy]) * 360f;

        activeProjectile.transform.eulerAngles += new Vector3(0f, 0f, Random.Range(-inaccuracy, inaccuracy));
        CanAttack = false;
    }





}
