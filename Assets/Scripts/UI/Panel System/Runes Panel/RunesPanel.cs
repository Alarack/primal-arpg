using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class RunesPanel : BasePanel {

    [Header("Template")]
    public SkillRuneEntry inventoryEntryTemplate;
    public Transform inventoryHolder;
    public Transform runeSlotHolder;
    public Transform skillEntryHolder;
    public Transform passiveSillEntryHolder;

    [Header("Rune Group Template")]
    public RuneGroupEntry runeGrouptemplate;
    public Transform runeGroupHolder;

    [Header("Text Fields")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillLevelText;
    public TextMeshProUGUI availableSkillPointsText;
    public TextMeshProUGUI headerText;

    [Header("Skill Entry Display")]
    public SkillEntry skillEntry;
    public SkillEntry passiveSkillEntry;
    public Ability CurrentAbility { get; private set; }


    private List<SkillRuneEntry> inventoryEntries = new List<SkillRuneEntry>();
    private List<SkillRuneEntry> skillRuneEntries = new List<SkillRuneEntry>();
    private List<RuneGroupEntry> runeGroupEntries = new List<RuneGroupEntry>();

    private List<SkillEntry> activeSkillEntries = new List<SkillEntry>();
    private List<SkillEntry> passiveSkillEntries = new List<SkillEntry>();

    private List<Item> currentSkillRunes = new List<Item>();

    protected override void Awake() {
        base.Awake();
        inventoryEntryTemplate.gameObject.SetActive(false);
        runeGrouptemplate.gameObject.SetActive(false);
        //CreateEmptySlots();
        skillEntry.gameObject.SetActive(false);
        passiveSkillEntry.gameObject.SetActive(false);
    }

    public override void Open() {
        base.Open();

    }

    public override void Close() {
        base.Close();

        TooltipManager.Hide();
        PanelManager.GetPanel<SkillsPanel>().CheckForUnspentSkillPoints();
    }

    public void Setup(Ability ability) {
        this.CurrentAbility = ability;
        //skillEntry.Setup(ability, SkillEntry.SkillEntryLocation.RunePanel, false);
        //skillNameText.text = CurrentAbility.Data.abilityName;
        //headerText.text = CurrentAbility.Data.abilityName + " Runes"; 

        if(ability.Data.category == AbilityCategory.KnownSkill) {
            SetupActiveSkills(ability);
            SetupPassiveSkills(null);
        }

        if(ability.Data.category == AbilityCategory.PassiveSkill) {
            SetupActiveSkills(null);
            SetupPassiveSkills(ability);
        }

        UpdateTextFields();
        //SetupRuneSlots();
        //PopulateInventory();
        CreateRuneGroups();
    }

    private void SetupActiveSkills(Ability selectedSkill) {
        List<SkillEntry> activeEntries = PanelManager.GetPanel<SkillsPanel>().GetActiveSkillEntries();

        activeSkillEntries.PopulateList(activeEntries.Count, skillEntry, skillEntryHolder, true);

        for (int i = 0; i < activeSkillEntries.Count; i++) {

            activeSkillEntries[i].Setup(activeEntries[i].Ability, SkillEntry.SkillEntryLocation.RunePanel, false);

            if (activeSkillEntries[i].Ability == selectedSkill) {
                activeSkillEntries[i].Select();
            }
        }

        if (selectedSkill == null)
            DeselectAllActiveEntries();

    }

    private void DeselectAllActiveEntries() {
        Debug.Log("Deselecting Actives");
        
        for (int i = 0; i < activeSkillEntries.Count; i++) {
            Debug.Log("Index: " + i);
            Debug.Log("Count of Active Skills: " + activeSkillEntries.Count);
            
            activeSkillEntries[i].Deselect();
        }
    }

    private void SetupPassiveSkills(Ability selectedSkill) {
        List<SkillEntry> passiveEntries = PanelManager.GetPanel<SkillsPanel>().GetActivePassiveEntries();

        passiveSkillEntries.PopulateList(passiveEntries.Count, passiveSkillEntry, passiveSillEntryHolder, true);

        for (int i = 0;i < passiveSkillEntries.Count; i++) {
            passiveSkillEntries[i].Setup(passiveEntries[i].Ability, SkillEntry.SkillEntryLocation.RunePanel, true);

            if (passiveSkillEntries[i].Ability == selectedSkill) {
                passiveSkillEntries[i].Select();
            }
        }

        if (selectedSkill == null)
            DeselectAllPassiveEntries();
    }

    private void DeselectAllPassiveEntries() {
        for (int i = 0; i < passiveSkillEntries.Count; i++) {
            passiveSkillEntries[i].Deselect();
        }
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
        availableSkillPointsText.text = EntityManager.ActivePlayer.Stats[StatName.SkillPoint].ToString();
        skillNameText.text = CurrentAbility.Data.abilityName;
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
        
        if(CurrentAbility.AbilityLevel >= CurrentAbility.Data.runeGroupData.Count) {
            return;
        }


        
        float skillPoints = EntityManager.ActivePlayer.Stats[StatName.SkillPoint];
        if(skillPoints < 1f) {
            PanelManager.OpenPanel<PopupPanel>().Setup("Insufficent Skill Points", "You don't have any Skill Points. Level up your character to gain more.");
            return;
        }

        int confirmSkillInvestment = PlayerPrefs.GetInt("ConfirmSkillInvest");

        if(confirmSkillInvestment == 0) {
            PanelManager.OpenPanel<PopupPanel>().Setup("Confirm Skill Investment", "Are you sure you want to invest a Skill Point into " + CurrentAbility.Data.abilityName + "?", ConfirmLevelUp);
        }
        else {
            ConfirmLevelUp();
        }


        //StatAdjustmentManager.AdjustSkillPoints(EntityManager.ActivePlayer, -1f);

        //CurrentAbility.LevelUp();
        //OnSkillLevelUp();
        //AudioManager.PlayAbilityLevelUp();

    }




    private void ConfirmLevelUp() {
        StatAdjustmentManager.AdjustSkillPoints(EntityManager.ActivePlayer, -1f);

        CurrentAbility.LevelUp();
        OnSkillLevelUp();
        AudioManager.PlayAbilityLevelUp();
    }

    public void ShowInfoTooltip() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Skill Points are used to unlock Skill Runes for Skills");
        builder.AppendLine();
        builder.AppendLine("You gain 1 Skill Point whenever your Character gains a Level.");
        builder.AppendLine();
        builder.AppendLine("Each time you invest a Skill Point into a Skill, you unlock a new Tier of Runes for that skill. You can choose one Rune from each Tier.");
        builder.AppendLine();
        builder.AppendLine("Left click any unlocked Rune to activate that Rune.");


        TooltipManager.Show(builder.ToString(), "Runes Info");
    }

    public void HideInfoTooltip() {
        TooltipManager.Hide();
    }

}
