using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;
using static AffixDatabase;

public class NPC : Entity
{

    public AIBrain Brain { get; protected set; }

    public Timer spawnInTimer;
    public float spawnDelayTime = 0.5f;
    private Collider2D myCollider;

    public List<NPCEliteAffixData> currentEliteAffixes = new List<NPCEliteAffixData>();

    public bool active;

    protected override void Awake() {
        base.Awake();
        Brain = GetComponent<AIBrain>();
        myCollider = GetComponent<Collider2D>();
        myCollider.enabled = false;

        spawnInTimer = new Timer(spawnDelayTime, OnSpawnInComplete, false);
    }

    protected override void Update() {
        base.Update();

        if(spawnInTimer != null && active == false) {
            spawnInTimer.UpdateClock();
        }
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
        active = true;
        myCollider.enabled = true;
    }

    public void BecomeElite(EliteAffixType type) {
        NPCEliteAffixData eliteData = AffixDataManager.GetEliteAffixDataByType(type);
        currentEliteAffixes.Add(eliteData);

        for (int i = 0; i < eliteData.abilities.Count; i++) {
            Brain.AddAbility(eliteData.abilities[i]);
        }

        VFXUtility.SpawnVFX(eliteData.vfxPrefab, transform, 0f, 2f);
    }

    protected override void Die(Entity source, Ability sourceAbility = null)
    {
        //Play Death animation
        //Drop Loot
        //Award exp
        base.Die(source, sourceAbility);

        EntityManager.RemoveEntity(this);
        //EffectManager.RemoveTarget(this);
        SpawnDeathVFX();
        Destroy(gameObject, 0.1f);
    }





}
