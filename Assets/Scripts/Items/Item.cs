using LL.Events;
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

    protected List<Ability> abilities = new List<Ability>();
    protected List<StatModifier> statModifiers = new List<StatModifier>();


    public Item(ItemData data, Entity owner) {
        this.Data = data;
        this.Owner = owner;

        SetupStatModifiers();
        SetupAbilities();
    }

    protected void SetupAbilities() {
        AbilityUtilities.SetupAbilities(Data.abilityData, abilities, Owner);
    }

    protected void SetupStatModifiers() {
        statModifiers.Clear();

        for (int i = 0; i < Data.statModifierData.Count; i++) {
            StatModifier mod = new StatModifier(Data.statModifierData[i].value, Data.statModifierData[i].modifierType, Data.statModifierData[i].targetStat, this);
            statModifiers.Add(mod);
        }
    }


    public void Equip(ItemSlot slot) {
        Equipped = true;
        CurrentSlot = slot;

        for (int i = 0;i < abilities.Count;i++) {
            abilities[i].Equip();
        }

        for (int i = 0; i < statModifiers.Count; i++) {
            StatAdjustmentManager.ApplyStatAdjustment(Owner, statModifiers[i], Data.statModifierData[i].variantTarget, Owner);
        }

        EventData data = new EventData();
        data.AddItem("Item", this);
        EventManager.SendEvent(GameEvent.ItemEquipped, data);
    }

    public void UnEquip() {


        //Debug.Log("Unequipping: " + Data.itemName);

        for (int i = 0; i < abilities.Count; i++) {
            abilities[i].TearDown();
        }

        for (int i = 0; i < statModifiers.Count; i++) {
            StatAdjustmentManager.RemoveStatAdjustment(Owner, statModifiers[i], Data.statModifierData[i].variantTarget, Owner, true);
        }

        EventData data = new EventData();
        data.AddItem("Item", this);
        EventManager.SendEvent(GameEvent.ItemUnequipped, data);

        Equipped = false;
        CurrentSlot = ItemSlot.None;
    }

    public string GetTooltip() {
        StringBuilder builder = new StringBuilder();

        if(this is ItemWeapon) {
            ItemWeapon weapon = (ItemWeapon)this;
            builder.Append("Damage: ").Append(weapon.minDamage).Append(" - ").Append(weapon.maxDamage).AppendLine();
        }

        for (int i = 0; i < Data.statModifierData.Count; i++) {
            StatModifierData modData = Data.statModifierData[i]; 
            
            builder.Append(modData.targetStat.ToString().SplitCamelCase()).Append(": ").Append(TextHelper.FormatStat(modData.targetStat, modData.value)).AppendLine();
        }



        return builder.ToString();
    }



}