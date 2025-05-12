using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using static AffixDatabase;
using System;
using static UnityEngine.GraphicsBuffer;

public class NPC : Entity
{

    public AIBrain Brain { get; protected set; }

    public Timer spawnInTimer;
    public float spawnDelayTime = 0.5f;
    private Collider2D myCollider;

    public List<NPCEliteAffixData> currentEliteAffixes = new List<NPCEliteAffixData>();

    public Entity MinionMaster { get; set; }

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

    public void BecomeElite(EliteAffixType type) {
        NPCEliteAffixData eliteData = AffixDataManager.GetEliteAffixDataByType(type);
        currentEliteAffixes.Add(eliteData);

        for (int i = 0; i < eliteData.abilities.Count; i++) {
            Brain.AddAbility(eliteData.abilities[i]);
        }

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
