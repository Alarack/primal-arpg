using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RunesPanel : BasePanel {

    [Header("Template")]
    public SkillRuneEntry inventoryEntryTemplate;
    public Transform inventoryHolder;
    public Transform runeSlotHolder;

    [Header("Rune Group Template")]
    public RuneGroupEntry runeGrouptemplate;
    public Transform runeGroupHolder;

    [Header("Text Fields")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillLevelText;
    public TextMeshProUGUI availableSkillPointsText;

    [Header("Skill Entry Display")]
    public SkillEntry skillEntry;
    public Ability CurrentAbility { get; private set; }


    private List<SkillRuneEntry> inventoryEntries = new List<SkillRuneEntry>();
    private List<SkillRuneEntry> skillRuneEntries = new List<SkillRuneEntry>();
    private List<RuneGroupEntry> runeGroupEntries = new List<RuneGroupEntry>();


    private List<Item> currentSkillRunes = new List<Item>();

    protected override void Awake() {
        base.Awake();
        inventoryEntryTemplate.gameObject.SetActive(false);
        runeGrouptemplate.gameObject.SetActive(false);
        //CreateEmptySlots();
    }

    public override void Open() {
        base.Open();

    }

    public override void Close() {
        base.Close();

        TooltipManager.Hide();
    }

    public void Setup(Ability ability) {
        this.CurrentAbility = ability;
        skillEntry.Setup(ability, SkillEntry.SkillEntryLocation.RunePanel, false);
        skillNameText.text = CurrentAbility.Data.abilityName;
        UpdateTextFields();
        SetupRuneSlots();
        PopulateInventory();
        CreateRuneGroups();
    }

    public void ResetRunes() {
        for (int i = 0; i < runeGroupEntries.Count; i++) {
            runeGroupEntries[i].ResetEntries();
        }
    }

    private void CreateEmptySlots() {
        inventoryEntries.PopulateList(16, inventoryEntryTemplate, inventoryHolder, true);
        for (int i = 0; i < inventoryEntries.Count; i++) {
            inventoryEntries[i].Setup(null, this, ItemSlot.Inventory);
            //Debug.Log("Creating a rune inventory Slot");
        }
    }

    private void PopulateInventory() {
        CreateEmptySlots();

        //Debug.Log("Current Runes: " + currentSkillRunes.Count);

        for (int i = 0; i < currentSkillRunes.Count; i++) {
            if (currentSkillRunes[i].Equipped == false) {
                inventoryEntries[i].Setup(currentSkillRunes[i], this, ItemSlot.Inventory);
            }
            //CreateSkillRuneSlot(currentSkillRunes[i], inventoryHolder, inventoryEntries, ItemSlot.Inventory);
        }
    }

    private void UpdateTextFields() {
        string levelText = CurrentAbility.AbilityLevel < 3 ? "Level - " + CurrentAbility.AbilityLevel.ToString() : "Level - MAX";


        skillLevelText.text = levelText;
        availableSkillPointsText.text = "Available Skill Point: " + EntityManager.ActivePlayer.Stats[StatName.SkillPoint];

    }

    private void OnSkillLevelUp() {
        UpdateTextFields();
        
        for (int i = 0; i < runeGroupEntries.Count; i++) {
            runeGroupEntries[i].UpdateLockout();
        }
    }

    private void CreateRuneGroups() {

        runeGroupEntries.PopulateList(CurrentAbility.Data.runeGroupData.Count,  runeGrouptemplate, runeGroupHolder, true);
        
        for (int i = 0; i < runeGroupEntries.Count; i++) {
            runeGroupEntries[i].Setup(this, CurrentAbility.Data.runeGroupData[i]);
        }
    }

    private void SetupRuneSlots() {

        //skillRuneSlots.PopulateList(CurrentAbility.RuneSlots, inventoryEntryTemplate, runeSlotHolder, true);

        //List<Ability> currentRunes = CurrentAbility.GetRunes();

        skillRuneEntries.ClearList();
        currentSkillRunes.Clear();

        List<Item> allRunes = ((EntityPlayer)CurrentAbility.Source).Inventory.GetItems(ItemType.Rune, false);

        //Debug.Log(allRunes.Count + " runes found");

        foreach (Item item in allRunes) {

            //Debug.Log("Rune Target: " + item.Data.runeAbilityTarget + ". Current Abilit: " + CurrentAbility.Data.abilityName);

            if (item.Data.runeAbilityTarget == CurrentAbility.Data.abilityName || item.Data.runeAbilityTarget == "") {

                if (item.Equipped == true && CurrentAbility.equippedRunes.Contains(item)) {
                    //Debug.Log("Creating Rune Slot for: " + item.Data.itemName);
                    CreateSkillRuneSlot(item, runeSlotHolder, skillRuneEntries, ItemSlot.RuneSlot);
                }
                //else {
                //    Debug.Log("Rune is not equipped: " + item.Data.itemName);
                //}

                currentSkillRunes.Add(item);
            }
        }

        if(skillRuneEntries.Count > CurrentAbility.RuneSlots) {
            Debug.LogWarning(CurrentAbility.Data.abilityName + " is Rune Overloaded!");
        }

        if (skillRuneEntries.Count < CurrentAbility.RuneSlots) {
            int difference = CurrentAbility.RuneSlots - skillRuneEntries.Count;
            for (int i = 0; i < difference; i++) {
                CreateSkillRuneSlot(null, runeSlotHolder, skillRuneEntries, ItemSlot.RuneSlot);
            }

        }

    }


    private void CreateSkillRuneSlot(Item item, Transform holder, List<SkillRuneEntry> list, ItemSlot slot) {
        SkillRuneEntry entry = Instantiate(inventoryEntryTemplate, holder);
        entry.gameObject.SetActive(true);
        entry.Setup(item, this, slot);
        list.Add(entry);
    }



    public void OnLevelUpClicked() {
        
        if(CurrentAbility.AbilityLevel == 3) {
            return;
        }
        
        float skillPoints = EntityManager.ActivePlayer.Stats[StatName.SkillPoint];
        if(skillPoints < 1f) {
            return;
        }
        StatAdjustmentManager.AdjustSkillPoints(EntityManager.ActivePlayer, -1f);

        CurrentAbility.LevelUp();
        OnSkillLevelUp();

    }

}
