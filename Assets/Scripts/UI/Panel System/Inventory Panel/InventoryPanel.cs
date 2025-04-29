using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LL.Events;
using TMPro;
using System.Linq;
using System.Text;

public class InventoryPanel : BasePanel {

    public List<InventoryItemEntry> paperDollEntries = new List<InventoryItemEntry>();
    private List<InventoryItemEntry> inventoryEntries = new List<InventoryItemEntry>();


    [Header("Affix Template")]
    public InventoryItemEntry forgeSlot;
    public ItemAffixEntry affixTemplate;
    public Transform affixHolder;

    private List<ItemAffixEntry> itemAffixEntries = new List<ItemAffixEntry>();

    [Header("Affix Slot Template")]
    public ItemAffixSlotEntry affixSlotTemplate;
    public Transform affixSlotHolder;

    [Header("VFX")]
    public ParticleSystem forgeSelectionVFX;

    private List<ItemAffixSlotEntry> itemAffixSlots = new List<ItemAffixSlotEntry>();
    private ItemAffixSlotEntry selectedSlot;



    [Header("Template")]
    public int slotCount = 60;
    public int defaultForgeCost = 25;
    public Transform inventoryHolder;
    public InventoryItemEntry inventoryEntryTemplate;

    public GameObject dropZone;

    [Header("Stat Display")]
    public StatDisplayEntry statDisplayTemplate;
    public Transform statDisplayHolder;

    private List<StatDisplayEntry> statDisplayEntries = new List<StatDisplayEntry>();

    private Task createAffixTask;
    private Task selectAffixTask;

    [Header("Text Fields")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI forgeCostText;
    //[Header("Testing Debug Things")]
    //public TextMeshProUGUI cdrText;

    [Header("Images")]
    public Image characterImage;

    protected override void Awake() {
        base.Awake();
        CreateEmptySlots();
        SetupPaperDollSlots();
        
        statDisplayTemplate.gameObject.SetActive(false);
        inventoryEntryTemplate.gameObject.SetActive(false);
        affixTemplate.gameObject.SetActive(false);
        affixSlotTemplate.gameObject.SetActive(false);
        forgeSlot.Setup(null, this);
    }

    protected override void OnEnable() {
        base.OnEnable();
        EventManager.RegisterListener(GameEvent.ItemAquired, OnItemAquired);
        EventManager.RegisterListener(GameEvent.ItemUnequipped, OnItemUnequipped);
        EventManager.RegisterListener(GameEvent.ItemEquipped, OnItemEquipped);
        EventManager.RegisterListener(GameEvent.UnitStatAdjusted, OnStatChanged);
    }

    protected override void OnDisable() {
        base.OnDisable();

        EventManager.RemoveMyListeners(this);
    }

    public override void Open() {
        base.Open();

        if (EntityManager.ActivePlayer == null)
            return;

        PanelManager.ClosePanel<SkillsPanel>();
        PanelManager.ClosePanel<RunesPanel>();

        PopulateInventory();
        SetStatValues();
        UpdateGoldText();
        UpdateForgeCost();

        //if(characterImage.sprite != null) {
        //    characterImage.sprite = EntityManager.ActivePlayer.Inventory.GetItems(ItemType.ClassSelection)[0].Data.itemIcon;
        //}
    }

    private void UpdateForgeCost() {
        forgeCostText.text = defaultForgeCost + ": ";
    }

    public override void Close() {
        if (InventoryBaseEntry.DraggedInventoryItem != null)
            return;
        
        base.Close();

        TooltipManager.Hide();
    }

    private void OnStatChanged(EventData data) {
        Entity target = data.GetEntity("Target");
        StatName stat = (StatName)data.GetInt("Stat");

        if (target != EntityManager.ActivePlayer)
            return;


        SetStatValues(stat);

        //if(stat == StatName.CooldownReduction) {
        //    SetStatValues();
        //}

    }

    private void SetStatValues(StatName stat = StatName.Vitality) {

        if(EntityManager.ActivePlayer == null) {
            return;
        }

        List<StatName> exceptions = new List<StatName> {
            StatName.DashSpeed,
            StatName.DashDuration,
            StatName.MoveSpeed,
            StatName.Health,
            StatName.Essence,
            StatName.Experience,
            StatName.StatReroll,
            StatName.EssenceShield,
            StatName.SkillPoint,
            StatName.HeathPotions,
        };

        if(exceptions.Contains(stat)) {
            return;
        }

        Dictionary<StatName, string> allStatDisplays = EntityManager.ActivePlayer.Stats.GetStatDisplays(exceptions);
        //List<StatName> relevantStats = EntityManager.ActivePlayer.Stats.GetListOfStatNames(exceptions);

        allStatDisplays = allStatDisplays.OrderBy(s => s.Key.ToString()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        //relevantStats.Sort();

        statDisplayEntries.PopulateList(allStatDisplays.Count, statDisplayTemplate, statDisplayHolder, true);

        int count = 0;
        foreach (var statDisplay in allStatDisplays) {
            string displayText = TextHelper.PretifyStatName(statDisplay.Key) + ": " + statDisplay.Value;
            statDisplayEntries[count].Setup(displayText, statDisplay.Key);
            count++;
        }


        //cdrText.text = "Cooldown Reduction: " + TextHelper.FormatStat(StatName.CooldownReduction, EntityManager.ActivePlayer.Stats[StatName.CooldownReduction]);

    }

    private void CreateEmptySlots() {
        inventoryEntries.PopulateList(slotCount, inventoryEntryTemplate, inventoryHolder, true);
        for (int i = 0; i < inventoryEntries.Count; i++) {
            inventoryEntries[i].Setup(null, this);
        }
    }

    private void SetupPaperDollSlots() {
        for (int i = 0; i < paperDollEntries.Count; i++) {
            paperDollEntries[i].Setup(null, this);
        }
    }

    public void AddToFirstEmptySlot(Item item) {

        InventoryItemEntry emptySlot = GetEmptyInventorySlot();
        if (emptySlot != null) {
            emptySlot.Add(item);
        }
        else {
            Debug.LogWarning("Inventory is full");
            //TODO: Drop item;
        }
    }

    public void CheckForDupeEquips(InventoryItemEntry entry) {
        for (int i = 0;i < paperDollEntries.Count;i++) {
            if (paperDollEntries[i] == entry) {
                continue;
            }

            if (paperDollEntries[i].MyItem == entry.MyItem) {
                Debug.Log("Duplicate Equip Detected: " + entry.MyItem.Data.itemName);
                paperDollEntries[i].Remove();
            }
        }
    }

    private void OnItemAquired(EventData data) {
        Item item = data.GetItem("Item");

        if(item.Data.Type == ItemType.Equipment)
            AddToFirstEmptySlot(item);

        if(item.Data.Type == ItemType.ClassSelection) {
            characterImage.sprite = EntityManager.ActivePlayer.Inventory.GetItems(ItemType.ClassSelection)[0].Data.itemIcon;
        }
    }

    private void OnItemEquipped(EventData data) {
        Item item = data.GetItem("Item");

        if (item == null)
            return;

        if (EntityManager.ActivePlayer.Inventory.ItemOwned(item) == true) {

            if (item.Data.Type == ItemType.Equipment)
                GetPaperDollSlot(item.CurrentSlot).Add(item);
        }
    }

    private void OnItemUnequipped(EventData data) {
        Item item = data.GetItem("Item");

        if (item == null)
            return;

        if (EntityManager.ActivePlayer.Inventory.ItemOwned(item) == true) {
            
            if(IsItemInInventory(item) == false && item.Data.Type == ItemType.Equipment)
                GetEmptyInventorySlot().Add(item);
        }
    }

    private InventoryItemEntry GetEmptyInventorySlot() {
        for (int i = 0; i < inventoryEntries.Count; i++) {
            if (inventoryEntries[i].MyItem == null) {
                return inventoryEntries[i];
            }
        }

        return null;
    }

    private InventoryItemEntry GetPaperDollSlot(ItemSlot slot) {
        for (int i = 0; i < paperDollEntries.Count; i++) {
            if (paperDollEntries[i].slot == slot) {
                return paperDollEntries[i];
            }
        }
        return null;
    }

    private bool IsItemInInventory(Item item) {
        for (int i = 0; i < inventoryEntries.Count; i++) {
            if (inventoryEntries[i].MyItem == item) 
                return true;
        }

        return false;
    }

    public void HighlightValidSLots() {
        if (InventoryBaseEntry.DraggedInventoryItem == null)
            return;

        for (int i = 0; i < paperDollEntries.Count; i++) {
            if (InventoryBaseEntry.DraggedInventoryItem.MyItem == null)
                continue;
            
            if (InventoryBaseEntry.DraggedInventoryItem.MyItem.Data.validSlots.Contains(paperDollEntries[i].slot)) {
                paperDollEntries[i].ShowHighlight();
            }
            else {
                paperDollEntries[i].HideHighlight();
            }
        }

    }

    public void HideAllHighlights() {
        for (int i = 0; i < paperDollEntries.Count; i++) {
            paperDollEntries[i].HideHighlight();
        }
    }

    private void PopulateInventory() {
        ClearInventory();
        List<Item> items = EntityManager.ActivePlayer.Inventory.GetInventoryItems();
        for (int i = 0; i < items.Count; i++) {
            inventoryEntries[i].Add(items[i]);
        }
    }

    private void ClearInventory() {
        for (int i = 0; i < inventoryEntries.Count; i++) {
            inventoryEntries[i].Remove();
        }
    }



    public void SetupItemAffixSlots() {
        if (forgeSlot.MyItem == null)
            return;

        itemAffixSlots.PopulateList(forgeSlot.MyItem.AffixSlots, affixSlotTemplate, affixSlotHolder, true);

        for (int i = 0; i < itemAffixSlots.Count; i++) {
            itemAffixSlots[i].Setup(this, forgeSlot.MyItem, null);

            UpdateAffixSlot(itemAffixSlots[i]);

            //int activeAffixCount = forgeSlot.MyItem.Affixes.Count;

            //if(i < activeAffixCount) {
            //    itemAffixSlots[i].UpdateAffix(forgeSlot.MyItem.Affixes.Keys.ElementAt(i));
            //}

        }

        OnAffixSlotSelected(itemAffixSlots[0]);

        //for (int i = 0; i < forgeSlot.MyItem.Affixes.Keys.Count; i++) {
        //    ItemData affixData = forgeSlot.MyItem.Affixes.Keys.ElementAt(i);

        //    itemAffixSlots[i].UpdateAffix(affixData);
        //}
    }

    private void UpdateGoldText() {
        float currentCoins = EntityManager.ActivePlayer != null ? EntityManager.ActivePlayer.Inventory.GetCurrencyAmount() : 0f;
        
        if(currentCoins >= 25f) {
            goldText.color = Color.white;
        }
        else {
            goldText.color = ColorDataManager.Instance["Stat Penalty Color"];
        }

        goldText.text = currentCoins.ToString();
    }

    public void UpdateAffixSlot(ItemAffixSlotEntry slot) {

        int index = itemAffixSlots.IndexOf(slot);

        if(index < forgeSlot.MyItem.Affixes.Count) {
            slot.UpdateAffix(forgeSlot.MyItem.Affixes.Keys.ElementAt(index));
        }
    }

    //public void UpdateAllAffixSlots() {
    //    for (int i = 0; i < itemAffixSlots.Count; i++) {
    //        UpdateAffixSlot(itemAffixSlots[i]);
    //    }
    //}

    public void ResetForge() {
        itemAffixEntries.ClearList();
        itemAffixSlots.ClearList();
        forgeSlot.Remove();
    }

    public void OnForgeClicked() {
        if (forgeSlot.MyItem == null)
            return;

        if(EntityManager.ActivePlayer.Inventory.TrySpendCoins(25f, "Forge") == false) {
            Debug.Log("Not enough money");
            return;
        }

        if (createAffixTask != null && createAffixTask.Running == true)
            return;

        AudioManager.PlayForgeSound();

        createAffixTask = new Task(CreateAffixEntries());
        UpdateGoldText();

    }

    private IEnumerator CreateAffixEntries() {
        WaitForSeconds waiter = new WaitForSeconds(0.2f);
        
        List<ItemData> affixData = ItemSpawner.CreateItemAffixSet(5);

        itemAffixEntries.PopulateList(affixData.Count, affixTemplate, affixHolder, false);

        for (int i = 0; i < itemAffixEntries.Count; i++) {
            itemAffixEntries[i].Setup(this, forgeSlot.MyItem, affixData[i]);
            itemAffixEntries[i].gameObject.SetActive(true);
            yield return waiter;
            //Debug.Log("Created an Affix: " + affixData[i].affixStatTarget + " " + TextHelper.FormatStat(affixData[i].affixStatTarget, affixData[i].statModifierData[0].value) + " Tier: " + affixData[i].tier);
        }
    }

    public void OnAffixSelected(ItemData affixdata, ItemAffixEntry entry) {
        if(forgeSlot.MyItem == null) {
            itemAffixEntries.ClearList();
            return;
        }

        if (createAffixTask != null && createAffixTask.Running == true)
            return;

        if (selectAffixTask != null && selectAffixTask.Running == true)
            return;

        if (selectedSlot == null) {
            Debug.LogError("No item affix slot is selected");
        }

        if(selectedSlot.AffixData == null) {
            forgeSlot.MyItem.AddAffix(affixdata);
        }
        else {
            forgeSlot.MyItem.ReplaceAffix(selectedSlot.AffixData, affixdata);
        }

        selectedSlot.UpdateAffix(affixdata);

        selectAffixTask = new Task(ShowSelectionEffect(entry));
    }

    private IEnumerator ShowSelectionEffect(ItemAffixEntry entry) {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();

        entry.ShowSelectionEffects();
        forgeSelectionVFX.Play();
        AudioManager.PlayForgeSelect();

        yield return waiter;    

        while(entry.selectionEffect.particleCount > 0) {
            yield return waiter;
        }

        itemAffixEntries.ClearList();
    }


    public void OnAffixSlotSelected(ItemAffixSlotEntry slotEntry) {
        selectedSlot = slotEntry;
        selectedSlot.Select();
        for (int i = 0; i < itemAffixSlots.Count; i++) {
            if (itemAffixSlots[i] != selectedSlot) {
                itemAffixSlots[i].Deselect();
            }
        }
    }

    public void RemoveAllItems() {
        for (int i = 0; i < paperDollEntries.Count; i++) {
            paperDollEntries[i].Remove();
        }
        for (int i = 0; i < inventoryEntries.Count; i++) {
            inventoryEntries[i].Remove();
        }
    }




    public void ShowForgeInfo() {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Drag an item into the Forge Slot to begin.");
        builder.AppendLine();
        builder.AppendLine("Choose an enhancment slot above the Forge Button and click Forge.");
        builder.AppendLine();
        builder.AppendLine("If you can afford the gold cost, you can forge any number of times in the same slot to re-roll for a desired affix.");
        builder.AppendLine();
        builder.AppendLine("Click on a desired Affix from the list to attach it to the current item.");
        builder.AppendLine();
        builder.AppendLine("You can reforge previously chosen Affixes.");

        TooltipManager.Show(builder.ToString(), "Forging");
    }

    public void HideForgeInfo() {
        TooltipManager.Hide();
    }

}
