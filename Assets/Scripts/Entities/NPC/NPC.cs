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

    protected override void OnHealthChanged(BaseStat stat, object source, float value)
    {
        if(stat.ModifiedValue <= 0f)
        {
            Die();
        }
    }


    protected override void Die()
    {
        //Play Death animation
        //Drop Loot
        //Award exp
        base.Die();

        EntityManager.RemoveEntity(this);
        //EffectManager.RemoveTarget(this);
        SpawnDeathVFX();
        Destroy(gameObject);
    }





}
