using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using static AffixDatabase;
//using System;


public class NPC : Entity
{

    public AIBrain Brain { get; protected set; }

    public Timer spawnInTimer;
    public float spawnDelayTime = 0.5f;
    private Collider2D myCollider;

    public List<NPCEliteAffixData> currentEliteAffixes = new List<NPCEliteAffixData>();

    public Entity MinionMaster { get; set; }
    public bool IsElite { get { return currentEliteAffixes.Count > 0; } }

    public bool active;

    private Timer selfDestructTimer;

    protected override void Awake() {
        base.Awake();
        Brain = GetComponent<AIBrain>();
        myCollider = GetComponent<Collider2D>();
        
        if(myCollider != null )
            myCollider.enabled = false;

        if (spawnDelayTime > 0f)
            spawnInTimer = new Timer(spawnDelayTime, OnSpawnInComplete, false);
        else
            OnSpawnInComplete(null);

        if(Stats.Contains(StatName.NPCLifetime)) {
            selfDestructTimer = new Timer(Stats[StatName.NPCLifetime], SelfDestruct, false);
        }

        if(entityType == EntityType.Enemy) {
            AdjustStatsByDifficulty();
        }
    }



    protected override void Update() {
        base.Update();

        if(spawnInTimer != null && active == false) {
            spawnInTimer.UpdateClock();
        }

        if(selfDestructTimer != null) {
            selfDestructTimer.UpdateClock();
        }
    }


    public Ability AddAbility(AbilityData data) {
        Ability newAbility = AbilityFactory.CreateAbility(data, this);
        AddAbility(newAbility);

        return newAbility;
    }


    public override void AddAbility(Ability ability) {
        base.AddAbility(ability);
        Brain.AddAbility(ability);
    }

    public override void RemoveAbility(Ability ability) {
        base.RemoveAbility(ability);
        Brain.RemoveAbility(ability);
    }

    private void OnSpawnInComplete(EventData data) {
        CheckHandicap();
        CheckElite();
        active = true;
        if(myCollider != null) 
            myCollider.enabled = true;
    }

    private void CheckHandicap() {
        if (ownerType != OwnerConstraintType.Enemy)
            return;

        float enemySpeed = PlayerPrefs.GetFloat("EnemySpeed");
        if (enemySpeed > 0) {
            StatModifier handiCapSpeed = new StatModifier(enemySpeed -1f, StatModType.PercentMult, StatName.MoveSpeed, this, StatModifierData.StatVariantTarget.Simple);
            StatModifier handiCapCooldown = new StatModifier(enemySpeed -1f, StatModType.PercentMult, StatName.CooldownReduction, this, StatModifierData.StatVariantTarget.Simple);

            if(Stats.Contains(StatName.CooldownReduction) == false) {
                Stats.AddStat(new SimpleStat(StatName.CooldownReduction, 0f));
            }

            StatAdjustmentManager.ApplyStatAdjustment(this, handiCapSpeed, handiCapSpeed.VariantTarget, this, null);
            StatAdjustmentManager.ApplyStatAdjustment(this, handiCapCooldown, handiCapCooldown.VariantTarget, this, null);
        }

    }

    private void AdjustStatsByDifficulty() {
        float difficulty = RoomManager.CurrentDifficulty;

        float damageModifier = (difficulty - 5f) / 50f;
        float healthModifier = (difficulty - 5f) / 30f;


        //Debug.Log("Adjusting Damage for: " + EntityName + " by " + damageModifier);

        StatAdjustmentManager.ApplyStatAdjustment(this, damageModifier, StatName.GlobalDamageModifier, StatModType.Flat,
            StatModifierData.StatVariantTarget.Simple, this, null);

        //Debug.Log("Adjusting health for: " + EntityName + " by " + healthModifier);

        StatAdjustmentManager.AdjustMaxValuePercentAdd(this, StatName.Health, healthModifier, this, null);
        StatAdjustmentManager.RefreshStat(this, StatName.Health, this);

    }

    private void CheckElite() {

        if (Brain == null)
            return;

        if(entityType != EntityType.Enemy) 
            return;
        
        float eliteRoll = Random.Range(0f, 1f);

        if(subtypes.Contains(EntitySubtype.Minion) == true) {
            if(eliteRoll <= 0.02f)
                BecomeElite(EliteAffixType.Overcharged);

            return;
        }

        if(eliteRoll <= 0.1f) {
            BecomeElite(EliteAffixType.Overcharged);
        }
    }

    public void BecomeElite(EliteAffixType type) {
        NPCEliteAffixData eliteData = AffixDataManager.GetEliteAffixDataByType(type);
        currentEliteAffixes.Add(eliteData);
        subtypes.Add(EntitySubtype.Elite);

        for (int i = 0; i < eliteData.abilities.Count; i++) {
            Brain.AddAbility(eliteData.abilities[i]);
        }
        StatModifier mod = new StatModifier(2f, StatModType.PercentMult, StatName.Health, this, StatModifierData.StatVariantTarget.RangeMax);
        StatAdjustmentManager.ApplyStatAdjustment(this, mod, mod.VariantTarget, this, null);
        Stats.Refresh(StatName.Health);

        transform.localScale *= 1.2f;

        VFXUtility.SpawnVFX(eliteData.vfxPrefab, transform, 0f, 2f);
    }

    private void SelfDestruct(EventData data) {
        ForceDie(this);
    }

    protected override void Die(Entity source, Ability sourceAbility = null)
    {

        //Debug.Log(EntityName + " is dying");

        base.Die(source, sourceAbility);

        if(Brain != null) 
            Brain.TearDownAbilities();
        EntityManager.RemoveEntity(this);

        int threat = (int)NPCDataManager.GetThreatLevel(EntityName);
        GameManager.Instance.totalThreatFromKilledEnemies += threat;

        SpawnDeathVFX();
        Destroy(gameObject, 0.1f);
    }





}
