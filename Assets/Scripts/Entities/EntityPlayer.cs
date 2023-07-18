using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;

public class EntityPlayer : Entity
{

    public Inventory Inventory { get; private set; }
    //public bool CanAttack { get; set; } = true;

    public float CurrentDamageRoll { get { return GetDamgeRoll(); } }



    protected override void Awake() {
        base.Awake();
        Inventory = GetComponent<Inventory>();
    }


    private void Update() {


    }

 
    public float GetDamgeRoll() {
        return Inventory.GetDamageRoll();
    }


    #region EVENTS

    #endregion


    protected override void Die(Entity source, Ability sourceAbility = null) {

        base.Die(source, sourceAbility);

        EntityManager.RemoveEntity(this);
        SpawnDeathVFX();

        //Show Gameover Screen PanelManager.OpenPanel<GameOverPanel>();
        //GameOverPanel panel = FindObjectOfType<GameOverPanel>();
        //panel.Open();

        gameObject.SetActive(false);

        //Destroy(gameObject);
    }

}
