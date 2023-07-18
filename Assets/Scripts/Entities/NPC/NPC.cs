using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL.Events;

public class NPC : Entity
{

    public AIBrain Brain { get; protected set; }

    protected override void Awake() {
        base.Awake();
        Brain = GetComponent<AIBrain>();
    }


    protected override void Die(Entity source, Ability sourceAbility = null)
    {
        //Play Death animation
        //Drop Loot
        //Award exp
        base.Die(source, sourceAbility);

        EntityManager.RemoveEntity(this);
        //EffectManager.RemoveTarget(this);
        SpawnDeathVFX();
        Destroy(gameObject);
    }





}
