using LL.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;

public class EntityPlayer : Entity
{

    //public EffectDefinition fireballTest;
    public AbilityDefinition testAbility;

    private List<Effect> testEffects = new List<Effect>();
    private List<Ability> testAbilities = new List<Ability>();
    public Inventory Inventory { get; private set; }
    public bool CanAttack { get; set; } = true;

    public float CurrentDamageRoll { get { return GetDamgeRoll(); } }


    protected override void Awake() {
        base.Awake();
        Inventory = GetComponent<Inventory>();
        //Effect fireball = AbilityFactory.CreateEffect(fireballTest.effectData, this);
        //testEffects.Add(fireball);

        //Ability debugAbility = AbilityFactory.CreateAbility(testAbility.AbilityData, this);
        //debugAbility.Equip();
        //testAbilities.Add(debugAbility);

    }


    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            //testEffects[0].ReceiveStartActivationInstance(null);

            if (PanelManager.IsPanelOpen<InventoryPanel>() == true)
                return;

            if (CanAttack == false)
                return;

            //EventData eventData = new EventData();
            //eventData.AddAbility("Ability", testAbilities[0]);

            //EventManager.SendEvent(GameEvent.UserActivatedAbility, eventData);

        }

        if (Input.GetKeyDown(KeyCode.I)) {
            PanelManager.TogglePanel<InventoryPanel>();
        }

        if(Input.GetKeyDown(KeyCode.Q)) {
            PanelManager.TogglePanel<SkillsPanel>();
        }


        //if(Input.GetKeyDown(KeyCode.C)) {
        //    StatAdjustmentManager.AdjustCDR(this, 0.5f, this);
        //}

        //if (Input.GetKeyDown(KeyCode.U)) {
        //    StatAdjustmentManager.AdjustCDR(this, -0.5f, this);
        //}


    }

  

    public float GetDamgeRoll() {
        return Inventory.GetDamageRoll();
    }


    #region EVENTS
    protected override void OnHealthChanged(BaseStat stat, object source, float value) {
        if (stat.ModifiedValue <= 0f) {
            Die();
        }

    }
    #endregion


    protected override void Die() {

        base.Die();

        EntityManager.RemoveEntity(this);
        SpawnDeathVFX();

        //Show Gameover Screen PanelManager.OpenPanel<GameOverPanel>();
        //GameOverPanel panel = FindObjectOfType<GameOverPanel>();
        //panel.Open();

        gameObject.SetActive(false);

        //Destroy(gameObject);
    }

}
