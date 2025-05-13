using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LL.Events;

public class AbilityManager : MonoBehaviour {

    public List<AbilityDefinition> preloadedAbilities = new List<AbilityDefinition>();
    public List<AbilityDefinition> preloadedPassives = new List<AbilityDefinition>();

    public List<Ability> this[AbilityCategory category] { get { return GetAbilitiesByCategory(category); } }

    public List<Ability> ClassFeatures { get { return GetClassFeatures(); } }
    public List<Ability> PassiveAbilities { get { return this[AbilityCategory.PassiveSkill]; } }
    public List<Ability> ActiveAbilities { get { return this[AbilityCategory.ActiveSkill]; } }
    public List<Ability> KnownAbilities { get { return this[AbilityCategory.KnownSkill]; } }
    public List<Ability> RuneAbilities { get { return this[AbilityCategory.Rune]; } }

    public Dictionary<AbilityCategory, List<Ability>> Abilities { get; private set; } = new Dictionary<AbilityCategory, List<Ability>>();

    public Dictionary<string, Ability> AbilitiesByName { get; protected set; } = new Dictionary<string, Ability>();

    public List<ElementMastery> Masteries { get; protected set; } = new List<ElementMastery>();

    public Action<Ability, int> onAbilityEquipped;
    public Action<Ability, int> onAbilityUnequipped;
    //public Action<Ability> onAbilityLearned;
    //public Action<Ability> onAbilityUnlearned;
    public Action<Ability, int, Ability, int> onAbilitySwapped;


    private List<Ability> learnedUseableAbilitiesFromItems = new List<Ability>();

    public Entity Owner { get; private set; }

    private void Awake() {
        Owner = GetComponent<Entity>();
        SetupAbilityDict();

    }

    public void Setup() {
        SetupPreloadedAbilities();
    }

    public void ResetAbilities() {

        foreach (var entry in Abilities) {
            for (int i = 0; i < entry.Value.Count; i++) {
                if (entry.Value[i].Tags.Contains(AbilityTag.Mastery))
                    continue;

                entry.Value[i].ResetLevel();
                entry.Value[i].ResetRunes();

                if (entry.Value[i].Data.startingAbility == false)
                    entry.Value[i].Locked = true;
            }
        }

        //PanelManager.GetPanel<RunesPanel>().ResetRunes();

        //List<Ability> actives = ActiveAbilities;

        //Debug.Log("Count of Actives: " + ActiveAbilities.Count);

        for (int i = ActiveAbilities.Count - 1; i >= 0; i--) {
            int currentSlot = PanelManager.GetPanel<HotbarPanel>().GetAbilitySlotIndex(ActiveAbilities[i]);

            //Debug.Log("Slot for: " + ActiveAbilities[i].Data.abilityName + " :: " + currentSlot);

            if (currentSlot > -1) {
                UnequipAbility(ActiveAbilities[i], currentSlot);
            }
        }

        for (int i = PassiveAbilities.Count - 1; i >= 0; i--) {
            if (PassiveAbilities[i].Tags.Contains(AbilityTag.Mastery))
                continue;

            if (PassiveAbilities[i].IsEquipped == true)
                PassiveAbilities[i].Uneqeuip();
        }

        new Task(AutoEquipStartingSkill());

    }

    private void OnEnable() {
        EventManager.RegisterListener(GameEvent.ItemAquired, OnItemAquired);
        EventManager.RegisterListener(GameEvent.AbilityStatAdjusted, OnAbilityStatChanged);

        EventManager.RegisterListener(GameEvent.ItemEquipped, OnItemEquipped);
        EventManager.RegisterListener(GameEvent.ItemUnequipped, OnItemUnequipped);
    }

    private void OnDisable() {
        EventManager.RemoveMyListeners(this);
    }

    private void OnItemAquired(EventData data) {
        Item item = data.GetItem("Item");

        if (item.Data.Type == ItemType.Skill) {
            //for (int i = 0; i < item.Data.learnableAbilities.Count; i++) {
            //    LearnAbility(item.Data.learnableAbilities[i].AbilityData);
            //}
            //LearnItemAbilities(item);

            Debug.Log("Item aquired: " + item.Data.itemName);
            LearnSkillFromScroll(item);
        }
    }

    private void OnItemEquipped(EventData data) {
        Item item = data.GetItem("Item");

        LearnUsableItemAbilities(item);

        //if (item is ItemWeapon) {
        //    LearnUsableItemAbilities(item);
            
        //    //List<Ability> newWeaponAbilities = LearnItemAbilities(item);

        //    //if (newWeaponAbilities.Count > 0)
        //    //    EquipWeaponAbility(newWeaponAbilities[0]);
        //}
    }

    public void OnItemUnequipped(EventData data) {
        Item item = data.GetItem("Item");

        if (item is ItemWeapon) {
            if (item.Data.learnableAbilities.Count > 0)
                UnlearnAbility(item.Data.learnableAbilities[0]);
        }
    }

    private void OnAbilityStatChanged(EventData data) {
        Ability ability = data.GetAbility("Ability");
        StatName stat = (StatName)data.GetInt("Stat");

        if (stat != StatName.AbilityRuneSlots)
            return;

        if (KnownAbilities.Contains(ability) == true) {

            if (ability.equippedRunes.Count > ability.RuneSlots) {
                Debug.LogWarning(ability.Data.abilityName + " is Overloaded!");
            }

        }
    }

    private void SetupAbilityDict() {

        AbilityCategory[] categories = Enum.GetValues(typeof(AbilityCategory)) as AbilityCategory[];
        foreach (AbilityCategory category in categories) {
            Abilities.Add(category, new List<Ability>());
        }
    }

    public List<Ability> GetAbilitiesByCategory(AbilityCategory category) {
        if (Abilities.TryGetValue(category, out List<Ability> results) == true)
            return results;

        return null;
    }

    private void SetupPreloadedAbilities() {
        AbilityUtilities.SetupAbilities(preloadedAbilities, KnownAbilities, Owner, false, true);

        AbilityUtilities.SetupAbilities(preloadedPassives, PassiveAbilities, Owner, false, true);

        new Task(AutoEquipStartingSkill());
    }

    private IEnumerator AutoEquipStartingSkill() {

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < KnownAbilities.Count; i++) {
            if (KnownAbilities[i].Data.startingAbility == true) {
                AutoEquipAbilityToHotbar(KnownAbilities[i], 4);
                break;
            }
        }


    }


    #region LEARNING AND EQUIPPING


    private void LearnUsableItemAbilities(Item item) {
        for (int i = 0; i < item.Data.learnableAbilities.Count; i++) {

            Ability existingAbility = GetAbilityByName(item.Data.learnableAbilities[i].AbilityData.abilityName);

            if (existingAbility != null) {
                Debug.LogError("Ability from Item " + item.Data.itemName + " already exists: " + item.Data.learnableAbilities[i].AbilityData.abilityName);
                continue;
            }

            Ability newItemAbility = CreateAndLearnAbility(item.Data.learnableAbilities[i].AbilityData);
            learnedUseableAbilitiesFromItems.Add(newItemAbility);
            if(item is ItemWeapon) {
                EquipActiveWeaponAbility(newItemAbility);
            }
        }
    }

    //private List<Ability> LearnItemAbilities(Item item) {
    //    List<Ability> newItemAbilities = new List<Ability>();

    //    for (int i = 0; i < item.Data.learnableAbilities.Count; i++) {
    //        Ability learnedAbility = LearnAbility(item.Data.learnableAbilities[i].AbilityData);

    //        if (learnedAbility != null) {
    //            newItemAbilities.Add(learnedAbility);
    //            AutoEquipToFirstEmptySlot(learnedAbility);
    //        }
    //    }

    //    learnedAbilities.AddRange(newItemAbilities);

    //    return newItemAbilities;
    //}

    private void LearnSkillFromScroll(Item item) {
        for (int i = 0; i < item.Data.learnableAbilities.Count; i++) {
            Ability existingAbility = GetAbilityByName(item.Data.learnableAbilities[i].AbilityData.abilityName);
            if (existingAbility != null) {
                UnlockAbility(existingAbility);
            }
            else {
                Debug.LogError("A skill: " + item.Data.learnableAbilities[i].AbilityData.abilityName + " did not exist.");
            }
        }
    }

    public void LearnAbility(Ability ability, AbilityCategory category, bool autoEquip = false) {


        Abilities[category].AddUnique(ability);

        if (AbilitiesByName.ContainsKey(ability.Data.abilityName) == false) {
            AbilitiesByName.Add(ability.Data.abilityName, ability);
        }

        if (autoEquip == true)
            ability.Equip();

        ability.Locked = false;

        EventData data = new EventData();
        data.AddAbility("Ability", ability);
        EventManager.SendEvent(GameEvent.AbilityLearned, data);
    }

    private void UnlockAbility(Ability ability) {


        if (ability.Tags.Contains(AbilityTag.Mastery)) {
            if(ability.IsEquipped == false)
                ability.Equip();
            
            return;
        }


        ability.Locked = false;

        if (ability.Data.category == AbilityCategory.KnownSkill)
            AutoEquipToFirstEmptySlot(ability);
        if (ability.Data.category == AbilityCategory.PassiveSkill)
            PanelManager.GetPanel<SkillsPanel>().AutoEquipPassiveToFirstEmptySlot(ability);
    }


    public void LearnAndEquipAbility(AbilityData abilityData, int startingLevel) {
        Ability existingAbility = GetAbilityByName(abilityData.abilityName);
        
        if (existingAbility != null) 
            UnlockAbility(existingAbility);
        else {
            CreateAndLearnAbility(abilityData, true, startingLevel);
        }

    }

    public Ability CreateAndLearnAbility(AbilityData abilityData, bool autoEquip = false, int startingLevel = 1) {

        Ability existingAbility = GetAbilityByName(abilityData.abilityName);

        if(existingAbility != null) {
            Debug.LogError("An ability: " + abilityData.abilityName + " already existed on: " + Owner.EntityName);
            return existingAbility;
        }

        Ability newAbility = AbilityFactory.CreateAbility(abilityData, Owner);
        newAbility.SetLevel(startingLevel);
        LearnAbility(newAbility, abilityData.category, autoEquip);

        return newAbility;
    }


    //public Ability LearnAbility(AbilityData abilityData, bool autoEquip = false, int startingLevel = 1) {
    //    Ability existingAbility = GetAbilityByName(abilityData.abilityName, AbilityCategory.Any);
    //    if (existingAbility != null) {
    //        existingAbility.Locked = false;

    //        if (abilityData.category == AbilityCategory.KnownSkill)
    //            AutoEquipToFirstEmptySlot(existingAbility);
    //        if (abilityData.category == AbilityCategory.PassiveSkill)
    //            PanelManager.GetPanel<SkillsPanel>().AutoEquipPassiveToFirstEmptySlot(existingAbility);


    //        return null;
    //    }

    //    //Debug.LogWarning("An Ability: " + abilityData.abilityName + " doesn't exist, so we have to create it fresh");


    //    Ability newAbility = AbilityFactory.CreateAbility(abilityData, Owner);
    //    newAbility.SetLevel(startingLevel);
    //    LearnAbility(newAbility, abilityData.category, autoEquip);

    //    return newAbility;
    //}


    public void UnlearnAbility(AbilityDefinition abilityDef) {

        for (int i = learnedUseableAbilitiesFromItems.Count - 1; i >= 0; i--) {
            if (learnedUseableAbilitiesFromItems[i].Data.abilityName == abilityDef.AbilityData.abilityName) {

                //Debug.Log("Unearling a weapon ability: " + learnedAbilities[i].Data.abilityName);

                Abilities[AbilityCategory.KnownSkill].RemoveIfContains(learnedUseableAbilitiesFromItems[i]);

                int currentSlot = PanelManager.GetPanel<HotbarPanel>().GetAbilitySlotIndex(learnedUseableAbilitiesFromItems[i]);

                if (currentSlot > -1) {
                    UnequipAbility(learnedUseableAbilitiesFromItems[i], currentSlot);
                }

                AbilitiesByName.Remove(learnedUseableAbilitiesFromItems[i].Data.abilityName);
                learnedUseableAbilitiesFromItems.Remove(learnedUseableAbilitiesFromItems[i]);
            }
        }
    }

    public void UnlearnAbility(Ability ability) {
        if (KnownAbilities.RemoveIfContains(ability) == true) {
            //onAbilityUnlearned?.Invoke(ability);
        }
    }

    public void EquipAbility(Ability ability, int index) {
        if (KnownAbilities.Contains(ability)) {
            if (ActiveAbilities.AddUnique(ability) == true) {
                ability.Equip();
                onAbilityEquipped?.Invoke(ability, index);
            }
            else
                Debug.LogError(ability.Data.abilityName + " is already equipped.");
        }
        else
            Debug.LogError("Tried to equip a skill you didn't know: " + ability.Data.abilityName);
    }

    public void EquipActiveWeaponAbility(Ability ability) {
        Ability currentWeaponAbility = PanelManager.GetPanel<HotbarPanel>().GetActiveAbilityBySlot(4);

        if (currentWeaponAbility == null) {
            EquipAbility(ability, 4);
        }
    }

    public void AutoEquipToFirstEmptySlot(Ability ability) {

        if (ability.Data.category == AbilityCategory.PassiveSkill) {
            Debug.LogError("A passive Ability: " + ability.Data.abilityName + " was passed to auto equip to first empty slot");
            return;
        }


        int firstEmptySlot = PanelManager.GetPanel<HotbarPanel>().GetFirstEmptySlot();

        if (firstEmptySlot > -1) {
            EquipAbility(ability, firstEmptySlot);
        }
    }

    public void AutoEquipAbilityToHotbar(Ability ability, int slot) {
        Ability existingAbility = PanelManager.GetPanel<HotbarPanel>().GetActiveAbilityBySlot(slot);

        if (existingAbility == null) {
            EquipAbility(ability, slot);
        }
    }

    public void UnequipAbility(Ability ability, int index) {
        if (ActiveAbilities.RemoveIfContains(ability) == true) {
            ability.Uneqeuip();
            onAbilityUnequipped?.Invoke(ability, index);
            Debug.Log("Unequipping: " + ability.Data.abilityName);
        }
        else
            Debug.LogError(ability.Data.abilityName + " is not equipped, so we can't unequip it");
    }

    public void MoveAbilitySlot(Ability ability, int fromIndex, int toIndex) {
        UnequipAbility(ability, fromIndex);
        EquipAbility(ability, toIndex);

        //onAbilityUnequipped?.Invoke(ability, fromIndex);
        //onAbilityEquipped?.Invoke(ability, toIndex);
    }

    public void SwapEquippedAbilities(Ability firstAbility, int firstAbilityIndex, Ability secondAbility, int secondAbilityIndex) {
        onAbilitySwapped?.Invoke(firstAbility, firstAbilityIndex, secondAbility, secondAbilityIndex);
    }

    #endregion

    #region GETTING AND ACTIVATION

    public List<Ability> GetAllAbilities() {
        List<Ability> results = new List<Ability>();

        foreach (var entry in Abilities) {
            for (int i = 0; i < entry.Value.Count; i++) {
                results.AddUnique(entry.Value[i]);
            }
        }
        return results;
    }

    public List<Ability> GetLockedAbilities(AbilityCategory category) {
        List<Ability> targetAbilities = GetAbilitiesByCategory(category);

        List<Ability> results = new List<Ability>();

        for (int i = 0; i < targetAbilities.Count; i++) {
            if (targetAbilities[i].Locked == true && targetAbilities[i].Tags.Contains(AbilityTag.Mastery) == false)
                results.Add(targetAbilities[i]);
        }

        return results;
    }

    public void UnlockAbility(AbilityDefinition ability) {
        UnlockAbility(ability.AbilityData.abilityName);
    }

    public void UnlockAbility(string abilityName) {
        Ability target = GetAbilityByName(abilityName);

        if (target != null) {
            target.Locked = false;
            if (target.Data.category == AbilityCategory.KnownSkill)
                AutoEquipToFirstEmptySlot(target);
            if (target.Data.category == AbilityCategory.PassiveSkill) {
                PanelManager.GetPanel<SkillsPanel>().AutoEquipPassiveToFirstEmptySlot(target);
            }
        }
        else {
            Debug.LogWarning("Null ability when unlocking: " + abilityName);
        }
    }

    public List<Ability> GetClassFeatures() {
        List<Ability> results = new List<Ability>();

        foreach (var entry in Abilities) {
            for (int i = 0; i < entry.Value.Count; i++) {
                if (entry.Value[i].Tags.Contains(AbilityTag.ClassFeature))
                    results.AddUnique(entry.Value[i]);
            }
        }
        return results;
    }

    public List<Effect> GetAllEffects() {
        List<Ability> allAbiliites = GetAllAbilities();
        List<Effect> results = new List<Effect>();

        foreach (var entry in allAbiliites) {

            List<Effect> effects = entry.GetAllEffects();

            for (int i = 0; i < effects.Count; i++) {
                results.AddUnique(effects[i]);
            }

            //results.AddRange(entry.GetAllEffects());
        }

        return results;
    }

    public List<Ability> GetAbilitiesByTag(AbilityTag tag, AbilityCategory category) {
        List<Ability> results = new List<Ability>();

        for (int i = 0; i < Abilities[category].Count; i++) {
            if (Abilities[category][i].Tags.Contains(tag) == true) {
                results.Add(Abilities[category][i]);
            }
        }


        return results;
    }

    public List<AbilityTag> GetRelevantTags() {
        List<AbilityTag> results = new List<AbilityTag>();

        for (int i = 0; i < KnownAbilities.Count; i++) {
            for (int j = 0; j < KnownAbilities[i].Tags.Count; j++) {
                if (KnownAbilities[i].Locked == false)
                    results.AddUnique(KnownAbilities[i].Tags[j]);
            }
        }

        return results;
    }


    public Ability GetAbilityByName(string name) {
        if(AbilitiesByName.TryGetValue(name, out Ability ability)) 
            return ability;

        return null;
    }


    public Ability GetAbilityByName(string name, AbilityCategory category) {

        if (category == AbilityCategory.Any) {
            foreach (var entry in Abilities) {
                List<Ability> abilities = entry.Value;
                for (int i = 0; i < abilities.Count; i++) {
                    if (abilities[i].Data.abilityName == name) {
                        return abilities[i];
                    }
                }
            }
        }

        for (int i = 0; i < this[category].Count; i++) {
            if (this[category][i].Data.abilityName == name)
                return this[category][i];
        }

        return null;
    }

    public List<Ability> GetRuneAbilities(string abilityName) {
        List<Ability> results = new List<Ability>();

        for (int i = 0; i < RuneAbilities.Count; i++) {
            if (RuneAbilities[i].Data.runeAbilityTarget == abilityName)
                results.Add(RuneAbilities[i]);
        }

        return results;
    }

    public void ActivateFirstAbility() {
        if (KnownAbilities.Count == 0) {
            Debug.LogError(Owner.EntityName + " has no abiliites and was told to force active an ability");
            return;
        }

        KnownAbilities[0].ForceActivate();
    }

    public void ActivateAbilityByName(string name, AbilityCategory category) {
        Ability targetAbility = GetAbilityByName(name);
        if (targetAbility == null) {
            Debug.LogError("An abiity: " + name + " could not be found on: " + Owner.EntityName + " and it was told to activate");
            return;
        }

        targetAbility.ForceActivate();
    }

    public bool HasAbility(AbilityDefinition ability) {
        Ability target = GetAbilityByName(ability.AbilityData.abilityName);

        //if (target != null && target.Locked == false) {
        //    Debug.Log(target.Data.abilityName + " is not locked");
        //}

        //if(target != null  && target.Locked == true) {
        //    Debug.Log(target.Data.abilityName + " is locked");
        //}

        //if(target == null) {
        //    Debug.Log(ability.AbilityData.abilityName + " is not found");
        //}

        return target != null && target.Locked == false;
    }

    public bool HasAbility(string abilityName) {
        Ability target = GetAbilityByName(abilityName);

        return target != null && target.Locked == false;
    }

    public bool IsAbilityOnHotbar(Ability ability) {
        return ActiveAbilities.Contains(ability);
    }

    public bool IsAbilityInSlot(Ability ability, int slot) {
        Ability targetAbility = PanelManager.GetPanel<HotbarPanel>().GetActiveAbilityBySlot(slot);

        return targetAbility != null && targetAbility == ability;
    }

    #endregion

}
