using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour {

    public enum EntityType {
        Player,
        Enemy,
        Projectile,
        EffectZone
    }


    public enum EntitySubtype {
        Dragon,
        Goblin,
        Elemental,
    }

    public enum EntityClass {
        None,
        SiegeMage,
        SpellstormMage,
        Berserker
    }

    //Weapon / Ability / Skill manager
    //Health Manager

    public string entityName;
    public string EntityName { get { return string.IsNullOrEmpty(entityName) == false ? entityName : gameObject.name; } }
    public EntityType entityType;
    public EntityClass CurrentClass { get; protected set; }
    public OwnerConstraintType ownerType;
    public List<EntitySubtype> subtypes = new List<EntitySubtype>();
    

    [Header("Stat Definitions")]
    public StatDataGroup statDefinitions;

    [Header("VFX")]
    public GameObject deathEffectPrefab;
    public GameObject spawnEffectPrefab;

    public EntityMovement Movement { get; private set; }
    public AbilityManager AbilityManager { get; private set; }
    public StatCollection Stats { get; private set; }

    public List<Status> ActiveStatuses { get; private set; } = new List<Status>();

    public bool IsDead { get; protected set; }

    public Ability ActivelyCastingAbility { get; set; }

    protected virtual void Awake() {
        Stats = new StatCollection(this, statDefinitions);

        if(Stats.Contains(StatName.Health) && Stats[StatName.Health] < 1) {
            Debug.LogError(EntityName + " has 0 starting health. You probably forgot to set the range curren value to the max");
        }

        Movement = GetComponent<EntityMovement>();
        AbilityManager = GetComponent<AbilityManager>();

        //if (entityType != EntityType.Projectile && entityType != EntityType.EffectZone) {
        //    EntityManager.RegisterEntity(this);
        //}

        
    }

    protected virtual void Start() {
        if (entityType != EntityType.Projectile && entityType != EntityType.EffectZone) {
            EntityManager.RegisterEntity(this);
        }

        if (AbilityManager != null)
            AbilityManager.Setup();
    }

    protected virtual void OnEnable() {
        RegisterStatListeners();
        SpawnEntranceEffect();
    }

    protected virtual void OnDisable() {
        //if (Stats.Contains(StatName.Health) == true) {
        //    Stats.RemoveStatListener(StatName.Health, OnHealthChanged);
        //}

        //StopAllCoroutines();

        EventManager.RemoveMyListeners(this);
    }

    protected virtual void OnCollisionEnter2D(Collision2D other) {

        //Debug.Log(EntityName + " Collided with: " + other.gameObject.name);

    }

    #region ABILIITES

    public Ability GetAbilityByName(string name, AbilityCategory category) {
        return AbilityManager.GetAbilityByName(name, category);
    }

    public void ActivateFirstAbility() {
        AbilityManager.ActivateFirstAbility();
    }

    public void ActivateAbilityByName(string name, AbilityCategory category) {
        AbilityManager.ActivateAbilityByName(name, category);
    }

    //public virtual Ability IsAbilityActivelyCasting() {

    //    Debug.LogError("Base Is Ability Active Casting. Overrwite this");
    //    return null;
    //}

    #endregion

    #region ENTITY CLASS

    public void SetEntityClass(EntityClass targetClass) {
        this.CurrentClass = targetClass;
    }

    #endregion  


    #region EVENTS

    protected virtual void RegisterStatListeners() {
        //if(Stats.Contains(StatName.Health) == true) {
        //    Stats.AddStatListener(StatName.Health, OnHealthChanged);
        //}

        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatChanged);
    }


    protected virtual void OnStatChanged(EventData data) {
        StatName stat = (StatName)data.GetInt("Stat");

        Entity target = data.GetEntity("Target");

        if (target != this)
            return;

        if (stat != StatName.Health)
            return;

        Ability sourceAbility = data.GetAbility("Ability");
        Entity cause = data.GetEntity("Source");

        if (Stats[StatName.Health] <= 0) {
            Die(cause, sourceAbility);
        }


    }

    //protected virtual void OnHealthChanged(BaseStat stat, object source, float value) {
    //    //if (stat.ModifiedValue <= 0f) {
    //    //    Die();
    //    //}
    //}

    #endregion

    #region STATUSES

    public void AddStatus(Status status) {
        ActiveStatuses.Add(status);
    }

    public void RemoveStatus(Status status) {
        ActiveStatuses.Remove(status);
    }

    public bool HasStatus(Status.StatusName status) {
        for (int i = 0; i < ActiveStatuses.Count; i++) {
            if (ActiveStatuses[i].statusName == status) {
                return true;
            }
        }

        return false;
    }


    #endregion


    protected virtual void Die(Entity source, Ability sourceAbility = null) {
        if(IsDead == true)
            return;

        IsDead = true;
        
        EventData data = new EventData();
        data.AddEntity("Victim", this);
        data.AddEntity("Killer", source);
        data.AddAbility("Ability Cause", sourceAbility);


        EventManager.SendEvent(GameEvent.UnitDied, data);
    }



    #region VFX

    protected void SpawnDeathVFX() {
        VFXUtility.SpawnVFX(deathEffectPrefab, transform.position, Quaternion.identity, 2f);
    }

    protected void SpawnEntranceEffect() {
        VFXUtility.SpawnVFX(spawnEffectPrefab, transform.position, Quaternion.identity, 2f);
    }


    #endregion
}
