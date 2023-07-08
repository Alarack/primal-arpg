using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IAbilityTargetable {

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

    //Weapon / Ability / Skill manager
    //Health Manager

    public string entityName;
    public string EntityName { get { return string.IsNullOrEmpty(entityName) == false ? entityName : gameObject.name; } }
    public EntityType entityType;
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
    //public List<AbilityData> baseAbilities = new List<AbilityData>();

    //protected List<Ability> abilities = new List<Ability>();

    protected virtual void Awake() {
        Stats = new StatCollection(this, statDefinitions);

        if(Stats.Contains(StatName.Health) && Stats[StatName.Health] < 1) {
            Debug.LogError(EntityName + " has 0 starting health. You probably forgot to set the range curren value to the max");
        }

        Movement = GetComponent<EntityMovement>();
        AbilityManager = GetComponent<AbilityManager>();
    }

    protected virtual void Start() {
        if (entityType != EntityType.Projectile && entityType != EntityType.EffectZone) {
            EntityManager.RegisterEntity(this);
        }
    }

    protected virtual void OnEnable() {
        RegisterStatListeners();
        SpawnEntranceEffect();
    }

    protected virtual void OnDisable() {
        if (Stats.Contains(StatName.Health) == true) {
            Stats.RemoveStatListener(StatName.Health, OnHealthChanged);
        }
    }

    #region ABILIITES

    //protected virtual void SetupAbilities() {
    //    AbilityUtilities.SetupAbilities(baseAbilities, abilities, this);
    //}

    public Ability GetAbilityByName(string name, AbilityCategory category) {
        return AbilityManager.GetAbilityByName(name, category);
    }

    public void ActivateFirstAbility() {
        AbilityManager.ActivateFirstAbility();
    }

    public void ActivateAbilityByName(string name, AbilityCategory category) {
        AbilityManager.ActivateAbilityByName(name, category);
    }

    #endregion

    #region EVENTS

    protected virtual void RegisterStatListeners() {
        if(Stats.Contains(StatName.Health) == true) {
            Stats.AddStatListener(StatName.Health, OnHealthChanged);
        }
    }


    protected virtual void OnHealthChanged(BaseStat stat, object source, float value) {
        //if (stat.ModifiedValue <= 0f) {
        //    Die();
        //}
    }





    #endregion


    protected virtual void Die() {
        EventData data = new EventData();
        data.AddEntity("Entity", this);
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
