using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.UI;



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
        Minion,
        Elite,
        Boss,
        MiniBoss,
        Orbital,
        Obstical
    }

    public enum EntityClass {
        None,
        SiegeMage,
        SpellstormMage,
        Berserker
    }

    //Weapon / Ability / Skill manager
    //Health Manager
    [SerializeField]
    private string entityName;
    public string EntityName { get { return string.IsNullOrEmpty(entityName) == false ? entityName : gameObject.name; } }
    public EntityType entityType;
    public int entityLevel = 1;
    public int levelsStored = 0;
    public EntityClass CurrentClass { get; protected set; }
    public OwnerConstraintType ownerType;
    public List<EntitySubtype> subtypes = new List<EntitySubtype>();
    public SpriteRenderer innerSprite;
    public SpriteRenderer mainSprite;

    [Header("Stat Definitions")]
    public StatDataGroup statDefinitions;

    [Header("VFX")]
    public float vfxScalar = 1f;
    public GameObject deathEffectPrefab;
    public GameObject spawnEffectPrefab;
    public GameObject facingIndicator;
    public Transform castingVFXPosition;
    public Transform statusVFXPosition;

    [Header("SFX")]
    public float spawnSFXVolume = 1f;
    public List<AudioClip> spawnSFX = new List<AudioClip>();
    public float hurtSFXVolume = 1f;
    public List<AudioClip> hurtSFX = new List<AudioClip>();
    public float deathSFXVolume = 1f;
    public List<AudioClip> deathSFX = new List<AudioClip>();

    [SerializedDictionary("Health", "Sprite")]
    public SerializedDictionary<float, Sprite> spriteProgression = new SerializedDictionary<float, Sprite>();

    [Header("Health Bar")]
    public Slider healthBar;

    public EntityMovement Movement { get; private set; }
    public float IsMoving { get { return Movement.IsMoving(); } }
    public Quaternion FacingRotation { get { return GetFacingRotation(); } }
    public AbilityManager AbilityManager { get; private set; }
    public StatCollection Stats { get; private set; }

    public List<Status> ActiveStatuses { get; private set; } = new List<Status>();

    public bool IsDead { get; set; }

    public bool Invincible { get; protected set; }

    public Ability ActivelyCastingAbility { get; set; }
    public Ability ActiveChannelingAbility { get; set; }

    public Ability SpawningAbility { get; set; }

    public AnimHelper AnimHelper { get; protected set; }

    protected Timer essenceRegenTimer;
    protected Timer healthRegenTimer;

    protected virtual void Awake() {
        //Debug.Log(EntityName + " is waking");
        Stats = new StatCollection(this, statDefinitions);

        if (Stats.Contains(StatName.Health) && Stats[StatName.Health] < 1) {
            Debug.LogError(EntityName + " has 0 starting health. You probably forgot to set the range curren value to the max");
        }

        if((entityType == EntityType.Enemy || entityType == EntityType.Player) && Stats.Contains(StatName.Armor) == false) {
            //Debug.Log("Adding base 0 armor to: " + EntityName);
            Stats.AddStat(new SimpleStat(StatName.Armor, 0f));
        }

        if ((entityType == EntityType.Enemy || entityType == EntityType.Player) && Stats.Contains(StatName.CastingMoveSpeedModifier) == false) {
            //Debug.Log("Adding base 0 armor to: " + EntityName);
            Stats.AddStat(new SimpleStat(StatName.CastingMoveSpeedModifier, -0.5f));
        }

        Movement = GetComponent<EntityMovement>();
        AbilityManager = GetComponent<AbilityManager>();
        AnimHelper = GetComponentInChildren<AnimHelper>();
        healthBar = GetComponentInChildren<Slider>();

        //if (entityType != EntityType.Projectile && entityType != EntityType.EffectZone) {
        //    EntityManager.RegisterEntity(this);
        //}


    }

    protected virtual void Start() {
        if (entityType != EntityType.Projectile 
            && entityType != EntityType.EffectZone
            && subtypes.Contains(EntitySubtype.Obstical) == false) {
            
            EntityManager.RegisterEntity(this);
        }

        if (AbilityManager != null)
            AbilityManager.Setup();

        if(Stats.Contains(StatName.EssenceRegenerationRate) == true)
            essenceRegenTimer = new Timer(CalculateEssenceRegenRate(), RegenEssence, true);

        if(Stats.Contains(StatName.HealthRegenerationRate) == true) {
            healthRegenTimer = new Timer(CalculateHealthRegenRate(), RegenHealth, true);
        }

        SendEntitySpawnEvent();
    }

    protected virtual void Update() {
        //Debug.Log("Moving: " + IsMoving);
    }

    protected virtual void OnEnable() {
        RegisterStatListeners();
        SpawnEntranceEffect();

        if (Stats.Contains(StatName.EssenceRegenerationRate) == true)
            Stats.AddStatListener(StatName.EssenceRegenerationRate, OnEssenceRegenChanged);

    }

    protected virtual void OnDisable() {
        //if (Stats.Contains(StatName.Health) == true) {
        //    Stats.RemoveStatListener(StatName.Health, OnHealthChanged);
        //}

        //StopAllCoroutines();
        if (Stats.Contains(StatName.EssenceRegenerationRate) == true)
            Stats.RemoveStatListener(StatName.EssenceRegenerationRate, OnEssenceRegenChanged);


        EventManager.RemoveMyListeners(this);
    }

    protected virtual void OnCollisionEnter2D(Collision2D other) {

        //Debug.Log(EntityName + " Collided with: " + other.gameObject.name);

    }

    #region ESSENCE & HEALTH

    public bool TrySpendEssence(float value, Ability cause = null) {
        float difference = Stats[StatName.Essence] - value;

        if (difference < 0)
            return false;

        //Debug.Log("Spending: " + value + " Essence");

        Stats.AdjustStatRangeCurrentValue(StatName.Essence, -value, StatModType.Flat, this);
        SendEssenceChangedEvent(-value, cause);

        return true;
    }

    public bool HasEnoughEssence(float value) {
        float difference = Stats[StatName.Essence] - value;

        if (difference < 0)
            return false;

        return true;
    }

    public void SpendAllEssence() {
        float allMana = Stats[StatName.Essence];


        Stats.AdjustStatRangeCurrentValue(StatName.Essence, -allMana, StatModType.Flat, this);
        SendEssenceChangedEvent(-allMana);
    }

    public float HandleManaShield(float incomingDamage, float conversionRate) {

        if (incomingDamage > 0f)
            return 0f;

        float convertedDamage = incomingDamage * -1;

        //Debug.Log("Incoming damage: " + convertedDamage);

        float manaCost = convertedDamage * conversionRate;


        if (Stats[StatName.Essence] < conversionRate) {
            //Debug.Log("Not enough mana to shield: " + conversionRate);
            return incomingDamage;
        }


        if (Stats[StatName.Essence] > manaCost) {
            TrySpendEssence(manaCost);
            //Debug.LogWarning("Blocked all damage: " + manaCost);
            return 0f;
        }

        float leftOver = manaCost - Stats[StatName.Essence];
        SpendAllEssence();
        return -leftOver;

    }

    protected void OnEssenceRegenChanged(BaseStat stat, object source, float value) {
        //Debug.Log("Essence regen changed: " + value + " :: " + Stats[StatName.EssenceRegenerationRate]);
        essenceRegenTimer.SetDuration(CalculateEssenceRegenRate());
    }

    protected void RegenEssence(EventData data) {
        //Debug.Log("Regening: " + Stats[StatName.EssenceRegenerationValue] + "% of max essence. CurrentValue: " + Stats[StatName.Essence]);
        Stats.AdjustStatRangeByPercentOfMaxValue(StatName.Essence, Stats[StatName.EssenceRegenerationValue], this);
        SendEssenceChangedEvent(Stats[StatName.EssenceRegenerationValue]);
    }

    protected void OnHealthRegenChange(BaseStat stat, object source, float value) {
        if (healthRegenTimer == null)
            return;

        healthRegenTimer.SetDuration(CalculateHealthRegenRate());
    }

    protected void RegenHealth(EventData data) {
        Stats.AdjustStatRangeByPercentOfMaxValue(StatName.Health, Stats[StatName.HealthRegenerationValue], this);
        SendHealthChangedEvent(Stats[StatName.HealthRegenerationValue]);
    }

    public float CalculateEssenceRegenRate() {
        float statValue = Stats[StatName.EssenceRegenerationRate];

        float convertedValue = 1 / statValue;

        return convertedValue;
    }

    protected float CalculateHealthRegenRate() {
        float statValue = Stats[StatName.HealthRegenerationRate];

        float convertedValue = 1 / statValue;

        return convertedValue;
    }

    #endregion

    #region ABILIITES

    public Ability GetAbilityByName(string name) {
        return AbilityManager.GetAbilityByName(name);
    }

    public void ActivateFirstAbility() {
        AbilityManager.ActivateFirstAbility();
    }

    public void ActivateAbilityByName(string name, AbilityCategory category) {
        AbilityManager.ActivateAbilityByName(name, category);
    }

    public virtual bool HasAbilityOfTag(AbilityTag tag) {
        Debug.LogError(EntityName + " is running the base HasAbilityOfTag Method. Overwrite this");
        return false;
    }

    public virtual void AddAbility(Ability ability) {

    }

    public virtual void RemoveAbility(Ability ability) {

    }

    //public virtual Ability IsAbilityActivelyCasting() {

    //    Debug.LogError("Base Is Ability Active Casting. Overrwite this");
    //    return null;
    //}

    public virtual bool IsCasting() {
        return ActivelyCastingAbility != null;
    }

    public virtual bool IsChanneling() {
        return ActiveChannelingAbility != null;
    }

    #endregion

    #region ENTITY CLASS

    public void SetEntityClass(EntityClass targetClass) {
        this.CurrentClass = targetClass;
    }

    #endregion  


    #region EVENTS

    private void SendEntitySpawnEvent() {
        EventData data = new EventData();
        data.AddEntity("Entity", this);
        data.AddAbility("Cause", SpawningAbility);

        EventManager.SendEvent(GameEvent.EntitySpawned, data);
    }

    protected void SendEssenceChangedEvent(float value, Ability cause = null) {
        EventData data = new EventData();
        data.AddEntity("Target", this);
        data.AddEntity("Source", this);
        data.AddFloat("Value", value);
        data.AddInt("Stat", (int)StatName.Essence);
        data.AddAbility("Ability", cause);

        EventManager.SendEvent(GameEvent.UnitStatAdjusted, data);
    }

    protected void SendHealthChangedEvent(float value, Ability cause = null) {
        EventData data = new EventData();
        data.AddEntity("Target", this);
        data.AddEntity("Source", this);
        data.AddFloat("Value", value);
        data.AddInt("Stat", (int)StatName.Health);
        data.AddAbility("Ability", cause);

        EventManager.SendEvent(GameEvent.UnitStatAdjusted, data);
    }


    protected virtual void RegisterStatListeners() {
        //if(Stats.Contains(StatName.Health) == true) {
        //    Stats.AddStatListener(StatName.Health, OnHealthChanged);
        //}

        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatChanged);
    }


    protected virtual void OnStatChanged(EventData data) {
        StatName stat = (StatName)data.GetInt("Stat");
        float value = data.GetFloat("Value");

        Entity target = data.GetEntity("Target");
        Ability ability = data.GetAbility("Ability");

        if (target != this)
            return;

        if (stat != StatName.Health)
            return;

        Ability sourceAbility = data.GetAbility("Ability");
        Entity cause = data.GetEntity("Source");

        HandleHealthSpriteChange();
        UpdateHealthBar();

        Status stun = GetStatus(Status.StatusName.Stunned);
        if (stun != null && value < 0f) {
            stun.Remove();
        }


        if (Stats[StatName.Health] <= 0) {
            
            if(ability != null && ability.Data.nonLethal) {
                float difference = 1 - Stats[StatName.Health];

                Stats.AddStatRangeCurrentModifier(StatName.Health, new StatModifier(difference, StatModType.Flat, StatName.Health, this, StatModifierData.StatVariantTarget.RangeCurrent));
                return;
            }
            
            Die(cause, sourceAbility);
        }
    }

    protected virtual void UpdateHealthBar() {
        if (healthBar == null)
            return;

        healthBar.value = Stats.GetStatRangeRatio(StatName.Health);
    }

    private void HandleHealthSpriteChange() {
        if (spriteProgression.Count == 0 || innerSprite == null)
            return;

        float currentHealthRatio = Stats.GetStatRangeRatio(StatName.Health);

        Sprite target = null;
        if (currentHealthRatio < 0.75f && currentHealthRatio > 0.5f) {
            target = spriteProgression[0.75f];
        }

        if (currentHealthRatio < 0.5f && currentHealthRatio > 0.25f) {
            target = spriteProgression[0.5f];
        }

        if (currentHealthRatio < 0.25f) {
            target = spriteProgression[0.25f];
        }

        if(innerSprite.sprite != target && target != null) {
            SpawnDeathVFX();
            innerSprite.sprite = target;
        }



        //float highestThreshold = .75f;
        //foreach (var item in spriteProgression) {
        //    if(currentHealthRatio < item.Key) {
                
        //        if(highestThreshold > item.Key)
        //            highestThreshold = item.Key;


        //        if (innerSprite.sprite != target) {
        //            Debug.Log("Heath: " + currentHealthRatio + " threshold: " + highestThreshold);

        //            SpawnDeathVFX();
        //            innerSprite.sprite = target;
        //        }

        //    }
        //}

    }

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

    public Status GetStatus(Status.StatusName status) {
        for (int i = 0; i < ActiveStatuses.Count; i++) {
            if (ActiveStatuses[i].statusName == status) {
                return ActiveStatuses[i];
            }
        }

        return null;
    }

    public List<Status> GetDots() {
        List<Status> results = new List<Status>();

        for (int i = 0; i < ActiveStatuses.Count; i++) {
            if (ActiveStatuses[i].IsDot) {
                results.Add(ActiveStatuses[i]);
            }
        }

        return results;
    }

    public bool HasDot() {
        if (ActiveStatuses.Count == 0)
            return false;

        for (int i = 0; i < ActiveStatuses.Count; i++) {
            if (ActiveStatuses[i].IsDot) 
                return true;
        }

        return false;
    }


    #endregion

    public virtual void LevelUp() {
        entityLevel++;
        levelsStored++;
        Stats.AddMaxValueModifier(StatName.Experience, 0.1f, StatModType.PercentMult, this);
        Stats.EmptyStatRange(StatName.Experience, this);
        StatAdjustmentManager.AdjustSkillPoints(this, 1f);

        EventData data = new EventData();
        data.AddEntity("Target", this);
        data.AddInt("Level", entityLevel);

        EventManager.SendEvent(GameEvent.EntityLeveled, data);

    }

    public virtual void EndGameCleanUp() {
        //entityLevel = 0;
        //levelsStored = 0;
        //Stats.GetStat<StatRange>(StatName.Experience).RemoveAllMaxModifiersFromSource(this);
        //Stats.EmptyStatRange(StatName.Experience, this);
        //Stats.RemoveAllModifiersFromSource(StatName.SkillPoint, this);

        SpawnDeathVFX();
        Destroy(gameObject);
    }

    public virtual void ResetLevel() {
        entityLevel = 0;
        levelsStored = 0;
        Stats.GetStat<StatRange>(StatName.Experience).RemoveAllMaxModifiersFromSource(this);
        Stats.EmptyStatRange(StatName.Experience, this);
        Stats.RemoveAllModifiersFromSource(StatName.SkillPoint, this);

        EventData data = new EventData();
        data.AddEntity("Target", this);
        data.AddInt("Level", entityLevel);

        EventManager.SendEvent(GameEvent.EntityLeveled, data);
    }

    public virtual void RemoveAllStatuses() {

        for (int i = ActiveStatuses.Count -1; i >=0; i--) {
            ActiveStatuses[i].Remove(true);
        }
    }

    public virtual void ForceDie(Entity source, Ability sourceAbility = null) {
        Die(source, sourceAbility);
    }

    protected virtual void Die(Entity source, Ability sourceAbility = null) {
        if (IsDead == true)
            return;

        IsDead = true;

        if (subtypes.Contains(EntitySubtype.Obstical)) {
            VFXUtility.SpawnDebirs(UnityEngine.Random.Range(8, 15), transform.position);
        }

        EventData data = new EventData();
        data.AddEntity("Victim", this);
        data.AddEntity("Killer", source);
        data.AddAbility("Ability Cause", sourceAbility);


        EventManager.SendEvent(GameEvent.UnitDied, data);
    }

    #region HELPERS

    protected Quaternion GetFacingRotation() {
        return facingIndicator == null ? transform.rotation : facingIndicator.transform.rotation;
    }

    public Transform GetOriginPoint() {
        return facingIndicator == null ? transform : facingIndicator.transform;
    }

    public Transform GetCastingVFXPosition() {
        return castingVFXPosition == null ? transform : castingVFXPosition.transform;
    }

    public Transform GetStatusVFXPosition() {
        return statusVFXPosition == null ? transform : statusVFXPosition.transform;
    }

    #endregion

    #region VFX

    protected void SpawnDeathVFX(float scale = 1f) {
        float desiredScale = vfxScalar;

        if(scale != 1f) {
            desiredScale = scale;
        }

        AudioManager.PlayRandomClip(deathSFX, transform.position, deathSFXVolume);

        VFXUtility.SpawnVFX(deathEffectPrefab, transform.position, Quaternion.identity, null, 2f, desiredScale);
    }

    protected void SpawnEntranceEffect(float scale = 1f) {
        float desiredScale = vfxScalar;

        if (scale != 1f) {
            desiredScale = scale;
        }

        //if(spawnEffectPrefab != null)
        //    Debug.LogWarning("Spawing Entrance Effect for: " + EntityName + " : " + spawnEffectPrefab.name);
        AudioManager.PlayRandomClip(spawnSFX, transform.position, spawnSFXVolume);

        VFXUtility.SpawnVFX(spawnEffectPrefab, transform.position, Quaternion.identity, null, 2f, desiredScale);
    }


    #endregion
}
