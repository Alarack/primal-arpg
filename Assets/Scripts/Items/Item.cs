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
    protected List<StatModifier> activeMods = new List<StatModifier>();
    protected List<StatModifierData> modData = new List<StatModifierData>();

    public Item(ItemData data, Entity owner) {
        this.Data = data;
        this.Owner = owner;

        modData = new List<StatModifierData>(data.statModifierData);

        for (int i = 0; i < modData.Count; i++) {
            modData[i].SetupStats();
        }
        //SetupStatModifiers();
        //SetupAbilities();
    }

    protected void SetupAbilities() {
        abilities.Clear();
        AbilityUtilities.SetupAbilities(Data.abilityDefinitions, abilities, Owner);
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
        SetupAbilities();

        for (int i = 0;i < abilities.Count;i++) {
            abilities[i].Equip();
        }

        for (int i = 0; i < activeMods.Count; i++) {
            StatAdjustmentManager.ApplyStatAdjustment(Owner, activeMods[i], activeMods[i].VariantTarget, Owner);
        }

        EventData data = new EventData();
        data.AddItem("Item", this);
        EventManager.SendEvent(GameEvent.ItemEquipped, data);
    }

    public void UnEquip() {


        //Debug.Log("Unequipping: " + Data.itemName);

        for (int i = 0; i < abilities.Count; i++) {
            abilities[i].Uneqeuip();
        }

        for (int i = 0; i < activeMods.Count; i++) {
            StatAdjustmentManager.RemoveStatAdjustment(Owner, activeMods[i], activeMods[i].VariantTarget, Owner, true);
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
