using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    
    public ItemData Data {  get; protected set; } 
    public bool Equipped { get; protected set; }
    public Entity Owner { get; protected set; }

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


    public void Equip() {
        Equipped = true;

        for (int i = 0;i < abilities.Count;i++) {
            abilities[i].Equip();
        }

        for (int i = 0; i < statModifiers.Count; i++) {
            StatAdjustmentManager.ApplyStatAdjustment(Owner, statModifiers[i], Data.statModifierData[i].variantTarget, Owner);
        }
    }

    public void UnEquip() { 
        Equipped = false;

        for (int i = 0; i < abilities.Count; i++) {
            abilities[i].TearDown();
        }

        for (int i = 0; i < statModifiers.Count; i++) {
            StatAdjustmentManager.RemoveStatAdjustment(Owner, statModifiers[i], Data.statModifierData[i].variantTarget, Owner);
        }
    }



}
