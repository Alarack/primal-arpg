using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Item
{

    public ItemSlot CurrentSlot { get; protected set; } = ItemSlot.None;
    public ItemData Data {  get; protected set; } 
    public bool Equipped { get; protected set; }
    public Entity Owner { get; set; }

    public List<Ability> Abilities { get; protected set;} = new List<Ability>();
    protected List<StatModifier> activeMods = new List<StatModifier>();
    protected List<StatModifierData> modData = new List<StatModifierData>();

    public Item(ItemData data, Entity owner, bool display = false) {
        this.Data = data;
        this.Owner = owner;

        modData = new List<StatModifierData>(data.statModifierData);

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
    }

    protected void SetupStatModifiers() {
        activeMods.Clear();

        for (int i = 0; i < modData.Count; i++) {
            StatModifier mod = new StatModifier(modData[i].value, modData[i].modifierType, modData[i].targetStat, this, modData[i].variantTarget);
            activeMods.Add(mod);
        }
    }


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

        EventData data = new EventData();
        data.AddItem("Item", this);
        EventManager.SendEvent(GameEvent.ItemUnequipped, data);

        Equipped = false;
        CurrentSlot = ItemSlot.None;
    }

    public string GetTooltip() {
        StringBuilder builder = new StringBuilder();

        if(string.IsNullOrEmpty( Data.itemDescription) == false) {
            builder.Append(Data.itemDescription).AppendLine();
        } 


        if(this is ItemWeapon) {
            ItemWeapon weapon = (ItemWeapon)this;
            builder.Append("Weapon Damage: ").Append(TextHelper.ColorizeText(weapon.minDamage.ToString(), ColorDataManager.Instance["Steel Gray"])).Append(" - ").Append(TextHelper.ColorizeText( weapon.maxDamage.ToString(), ColorDataManager.Instance["Steel Gray"])).AppendLine();
        }

        for (int i = 0; i < Data.statModifierData.Count; i++) {
            StatModifierData modData = Data.statModifierData[i]; 
            
            builder.Append(modData.targetStat.ToString().SplitCamelCase()).Append(": ").Append(TextHelper.FormatStat(modData.targetStat, modData.value)).AppendLine();
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
