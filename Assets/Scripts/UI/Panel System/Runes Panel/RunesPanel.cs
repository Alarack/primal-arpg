using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunesPanel : BasePanel
{

    [Header("Template")]
    public SkillRuneEntry inventoryEntryTemplate;
    public Transform inventoryHolder;
    public Transform runeSlotHolder;

    [Header("Skill Entry Display")]
    public SkillEntry skillEntry;
    public Ability CurrentAbility { get; private set; }


    private List<SkillRuneEntry> inventoryEntries = new List<SkillRuneEntry>();
    private List<SkillRuneEntry> skillRuneEntries = new List<SkillRuneEntry>();


    private List<Item> currentSkillRunes = new List<Item>();

    protected override void Awake() {
        base.Awake();
        inventoryEntryTemplate.gameObject.SetActive(false);
        CreateEmptySlots();
    }


    public override void Open() {
        base.Open();
    }

    public void Setup(Ability ability) {
        this.CurrentAbility = ability;
        skillEntry.Setup(ability, SkillEntry.SkillEntryLocation.RunePanel);

        SetupRuneSlots();
        PopulateInventory();
    }

    private void CreateEmptySlots() {
        inventoryEntries.PopulateList(16, inventoryEntryTemplate, inventoryHolder, true);
        for (int i = 0; i < inventoryEntries.Count; i++) {
            inventoryEntries[i].Setup(null, this, ItemSlot.Inventory);
        }
    }

    private void PopulateInventory() {
        inventoryEntries.ClearList();

        for (int i = 0; i < currentSkillRunes.Count; i++) {
            if (currentSkillRunes[i].Equipped == false)
                CreateSkillRuneSlot(currentSkillRunes[i], inventoryHolder, inventoryEntries, ItemSlot.Inventory);
        }
    }

    private void SetupRuneSlots() {

        //skillRuneSlots.PopulateList(CurrentAbility.RuneSlots, inventoryEntryTemplate, runeSlotHolder, true);

        //List<Ability> currentRunes = CurrentAbility.GetRunes();

        skillRuneEntries.ClearList();
        currentSkillRunes.Clear();

        List<Item> allRunes = ((EntityPlayer)CurrentAbility.Source).Inventory.GetItems(ItemType.Rune);

        foreach (Item item in allRunes) {
            if (item.Data.runeAbilityTarget == CurrentAbility.Data.abilityName) {
                currentSkillRunes.Add(item);
                CreateSkillRuneSlot(item, runeSlotHolder, skillRuneEntries, ItemSlot.RuneSlot);
            }
        }

        if(skillRuneEntries.Count < CurrentAbility.RuneSlots) { 
           int difference = CurrentAbility.RuneSlots - skillRuneEntries.Count;
            for (int i = 0; i < difference; i++) {
                CreateSkillRuneSlot(null, runeSlotHolder, skillRuneEntries, ItemSlot.RuneSlot);
            }
        
        }

    }


    private void CreateSkillRuneSlot(Item item, Transform holder, List<SkillRuneEntry> list, ItemSlot slot) {
        SkillRuneEntry entry = Instantiate(inventoryEntryTemplate, holder);
        entry.enabled = true;
        entry.Setup(item, this, slot);
        list.Add(entry);
    }

}
