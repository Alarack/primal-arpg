using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Item
{

    public ItemSlot CurrentSlot { get; protected set; } = ItemSlot.None;
    public ItemData Data {  get; protected set; } 
    public bool Equipped { get; protected set; }
    public Entity Owner { get; set; }

    public int AffixSlots { get; set; }

    public List<Ability> Abilities { get; protected set;} = new List<Ability>();
    protected List<StatModifier> activeMods = new List<StatModifier>();
    protected List<StatModifierData> modData = new List<StatModifierData>();

    //protected List<ItemData> itemAffixData = new List<ItemData>();
    //protected List<StatModifier> affixModifiers = new List<StatModifier>();

    public Dictionary<ItemData, List<StatModifier>> Affixes { get; protected set; } = new Dictionary<ItemData, List<StatModifier>>();

    public Item(ItemData data, Entity owner, bool display = false) {
        this.Data = data;
        this.Owner = owner;
        AffixSlots = data.baseAffixSlots;

        modData = new List<StatModifierData>(data.statModifierData);

        //for (int i = 0; i < data.itemAffixes.Count; i++) {
        //    modData.AddRange(data.itemAffixes[i].statModifierData);
        //}

        for (int i = 0; i < modData.Count; i++) {
            modData[i].SetupStats();
        }

        if(display == true) {
            SetupStatModifiers();
            SetupAbilities();
        }

        EventManager.RegisterListener(GameEvent.ItemAquired, OnItemAquired);
        EventManager.RegisterListener(GameEvent.ItemDropped, OnItemDropped);
    }



    private void OnItemAquired(EventData data) {
        Item item = data.GetItem("Item");
        if (item != this) {
            return;
        }

        SetupAbilities();

        if(Data.Type == ItemType.ClassSelection) {
            Owner.SetEntityClass(Data.entityClass);
            Equip(ItemSlot.Class);
        }

        RollInitialAffixes();
    }

    private void RollInitialAffixes() {

        if (Data.Type != ItemType.Equipment)
            return;

        float affixRoll = UnityEngine.Random.Range(0f, 1f);

        if (affixRoll < 0.5f && affixRoll > 0.25f) {
            AddAffix(ItemSpawner.CreateItemAffixSet(1, Data.validSlots[0])[0]);
        }
        else if (affixRoll < 0.25f) {
            AddAffix(ItemSpawner.CreateItemAffixSet(1, Data.validSlots[0])[0]);
            AddAffix(ItemSpawner.CreateItemAffixSet(1, Data.validSlots[0])[0]);
        }
    }

    private void OnItemDropped(EventData data) {
        Item item = data.GetItem("Item");
        if (item != this) {
            return;
        }

        EventManager.RemoveMyListeners(this);
    }

    protected void SetupAbilities(bool autoEquip = false, bool registerWithPlayer = false) {
        Abilities.Clear();
        AbilityUtilities.SetupAbilities(Data.abilityDefinitions, Abilities, Owner, autoEquip, registerWithPlayer);
        //Debug.Log("Setting up abilities for: " + Data.itemName);
    }

    protected void SetupStatModifiers() {
        activeMods.Clear();

        for (int i = 0; i < modData.Count; i++) {
            StatModifier mod = new StatModifier(modData[i].value, modData[i].modifierType, modData[i].targetStat, this, modData[i].variantTarget);
            activeMods.Add(mod);
        }
    }

    #region AFFIXES

    public void AddAffix(ItemData affixData) {
        if(Affixes.ContainsKey(affixData) == true) {
            Affixes[affixData].AddRange(affixData.CreateStatModifiers(Owner));
        }
        else {
            Affixes.Add(affixData, affixData.CreateStatModifiers(Owner));
        }

        if (Equipped == true) {
            ApplyAffixMods(Affixes[affixData]);
        }
    }

    public void RemoveAffix(ItemData affixData) {
        if(Affixes.ContainsKey(affixData) == true) {
            if(Equipped == true) {
                RemoveAffixMods(Affixes[affixData]);
            }
            Affixes.Remove(affixData);
        }
    }

    public void ReplaceAffix(ItemData oldAffix, ItemData newAffix) {
        if (Affixes.ContainsKey(oldAffix) == true) {
            RemoveAffix(oldAffix);
            AddAffix(newAffix);
        }
    }

    protected void ApplyAllAffixMods() {
        foreach (var affix in Affixes) {
            ApplyAffixMods(Affixes[affix.Key]);
        }
    }

    protected void RemoveAllAffixMods() {
        foreach (var affix in Affixes) {
            RemoveAffixMods(Affixes[affix.Key]);
        }
    }

    protected void ApplyAffixMods(List<StatModifier> affixMods) {
        foreach (StatModifier mod in affixMods) {
            StatAdjustmentManager.ApplyStatAdjustment(Owner, mod, mod.TargetStat, mod.VariantTarget, null, 1, true);
        }
    }

    protected void RemoveAffixMods(List<StatModifier> affixMods) {
        foreach (StatModifier mod in affixMods) {
            StatAdjustmentManager.RemoveStatAdjustment(Owner, mod, mod.VariantTarget, Owner, null);
        }
    }

    #endregion

    public void Equip(ItemSlot slot) {


        //Debug.Log("Equipping: " + Data.itemName);

        Equipped = true;
        CurrentSlot = slot;

        SetupStatModifiers();
        SetupAbilities(false, true);

        for (int i = 0;i < Abilities.Count;i++) {
            Abilities[i].Equip();
        }

        for (int i = 0; i < activeMods.Count; i++) {
            StatAdjustmentManager.ApplyStatAdjustment(Owner, activeMods[i], activeMods[i].VariantTarget, Owner, null);
        }

        ApplyAllAffixMods();

        EventData data = new EventData();
        data.AddItem("Item", this);
        EventManager.SendEvent(GameEvent.ItemEquipped, data);
    }

    public void DeactivateEquippedRunes() {
        if(Data.Type == ItemType.Rune) {
            for (int i = 0; i < Abilities.Count; i++) {
                if (Abilities[i].IsEquipped == true)
                    Abilities[i].Uneqeuip();
            }
        }
    }

    public void ReactivateEquippedRunes() {
        if (Data.Type == ItemType.Rune) {
            for (int i = 0; i < Abilities.Count; i++) {
                if (Abilities[i].IsEquipped == false)
                    Abilities[i].Equip();
            }
        }
    }

    public void UnEquip() {


        //Debug.Log("Unequipping: " + Data.itemName);

        for (int i = 0; i < Abilities.Count; i++) {
            Abilities[i].Uneqeuip();
        }

        for (int i = 0; i < activeMods.Count; i++) {
            StatAdjustmentManager.RemoveStatAdjustment(Owner, activeMods[i], activeMods[i].VariantTarget, Owner, null, true);
        }

        RemoveAllAffixMods();

        EventData data = new EventData();
        data.AddItem("Item", this);
        EventManager.SendEvent(GameEvent.ItemUnequipped, data);

        Equipped = false;
        CurrentSlot = ItemSlot.None;
    }

    public string GetTooltip() {
        StringBuilder builder = new StringBuilder();

        if(string.IsNullOrEmpty( Data.itemDescription) == false) {
            builder.Append("<i>" +Data.itemDescription +"</i>").AppendLine().AppendLine();
        } 


        if(this is ItemWeapon) {
            ItemWeapon weapon = (ItemWeapon)this;
            builder.Append("Weapon Damage: ").Append(TextHelper.ColorizeText(weapon.minDamage.ToString(), ColorDataManager.Instance["Steel Gray"])).Append(" - ").Append(TextHelper.ColorizeText( weapon.maxDamage.ToString(), ColorDataManager.Instance["Steel Gray"])).AppendLine();
        }

        for (int i = 0; i < Data.statModifierData.Count; i++) {
            StatModifierData modData = Data.statModifierData[i];

            string maxPrefix = Data.statModifierData[0].targetStat == StatName.Essence || Data.statModifierData[0].targetStat == StatName.Health ? "Max " : "";

            string statName = maxPrefix + TextHelper.PretifyStatName(modData.targetStat);

            builder.Append(statName).Append(": ").Append(TextHelper.FormatStat(modData.targetStat, modData.value, modData.displayAsPercent)).AppendLine();
        }

        if(Affixes.Count > 0) {
            builder.AppendLine("Affixes: ");

            foreach (var affix in Affixes) {
                builder.Append(affix.Key.GetAffixTooltip());
            }

            builder.AppendLine();
        }
       

        for (int i = 0; i < Abilities.Count; i++) {

            string abilityTooltip = Abilities[i].GetTooltip();
            if(string.IsNullOrEmpty(abilityTooltip) == false) {
                builder.Append(abilityTooltip);

                if ( i != Abilities.Count - 1) {
                    builder.AppendLine();
                }
            }
                
        }


        if(Data.Type == ItemType.Rune) {

            if(Abilities.Count > 0) {
                builder.Append(Ability.GetRunesTooltip(Abilities));
            }
            else {
                Debug.LogError("A Rune: " + Data.itemName + " has no abilities");
            }
        }
        return builder.ToString();
    }



}
