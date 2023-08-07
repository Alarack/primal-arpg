using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemWeapon : Item {

    public float minDamage;
    public float maxDamage;

    public float DamageRoll { get { return RollDamage(); } }
    public float Averagedamage { get { return (minDamage + maxDamage) /2f; } }
    
    public ItemWeapon(ItemData data, Entity owner, bool display = false) : base(data, owner, display) {
        this.minDamage = data.minDamage;
        this.maxDamage = data.maxDamage;
    
    }


    private float RollDamage() {
        return Random.Range(minDamage, maxDamage);
    }
}
