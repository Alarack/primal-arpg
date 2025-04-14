using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CharacterSelectPanel : BasePanel {

    [Header("Template")]
    public CharacterChoiceEntry template;
    public Transform holder;
    public SkillPreviewEntry skillPreviewTemplate;
    public Transform skillPreviewHolder;
    public CharacterStarterPackageEntry starterPackageTemplate;
    public Transform starterPackageHolder;

    [Header("Anchors & Faders")]
    public Transform selectedClassAnchor;
    public CanvasGroup bulletPointsFader;
    public CanvasGroup exampleSpellsFader;

    [Header("Ray Blocker")]
    public GameObject rayBlocker;

    [Header("Class Data")]
    public List<ItemDefinition> classItems = new List<ItemDefinition>();

    private List<CharacterChoiceEntry> entries = new List<CharacterChoiceEntry>();


    private CharacterChoiceEntry selectedClass;

    private List<SkillPreviewEntry> skillPreviewEntries = new List<SkillPreviewEntry>();
    private List<CharacterStarterPackageEntry> starterPackageEntries = new List<CharacterStarterPackageEntry>();

    protected override void Awake() {
        base.Awake();
        template.gameObject.SetActive(false);
    }

    public override void Open() {
        base.Open();

        new Task(PopulateClassOptions());
        bulletPointsFader.alpha = 0f;
        exampleSpellsFader.alpha = 0f;
        rayBlocker.SetActive(false);
        //entries.PopulateList(classItems.Count, template, holder, true);
        //for (int i = 0; i < entries.Count; i++) {
        //    entries[i].Setup(classItems[i], this);
        //}
    }

    public override void Close() {
        base.Close();

        TooltipManager.Hide();
        starterPackageEntries.ClearList();
    }


    private IEnumerator PopulateClassOptions() {
        WaitForSeconds waiter = new WaitForSeconds(0.2f);

        entries.PopulateList(classItems.Count, template, holder, true);
        for (int i = 0; i < entries.Count; i++) {
            entries[i].Setup(classItems[i], this);
            yield return waiter;
        }

    }

    public void SetupExampleSpells(List<AbilityDefinition> skills) {
        skillPreviewEntries.PopulateList(skills.Count, skillPreviewTemplate, skillPreviewHolder, true);
        for (int i = 0; i < skillPreviewEntries.Count; i++) {
            skillPreviewEntries[i].Setup(skills[i]);
        }
    }




    public void StartInfoFadein() {
        new Task(FadeInInfoPanels());
    }

    public IEnumerator FadeInInfoPanels() {
        WaitForSeconds waiter = new WaitForSeconds(0.2f);

        bulletPointsFader.DOFade(1f, 0.2f);
        yield return waiter;
        exampleSpellsFader.DOFade(1f, 0.2f);

    }

    public IEnumerator FadeoutInfoPanels() {
        WaitForSeconds waiter = new WaitForSeconds(0.1f);

        bulletPointsFader.DOFade(0f, 0.1f);
        yield return waiter;
        exampleSpellsFader.DOFade(0f, 0.1f);

    }


    public void OnBackClicked() {
        PanelManager.OpenPanel<MainMenuPanel>();
        Close();
    }

    public void OnClassInfoClicked(CharacterChoiceEntry entry) {
        selectedClass = entry;
        selectedClass.Select();
        selectedClass.Show();

        for (int i = 0; i < entries.Count; i++) {
            if (entries[i] != entry)
                entries[i].Hide();
        }

    }

    public void UnhideAllEntries(CharacterChoiceEntry entry) {
        for (int i = 0; i < entries.Count; i++) {
            if (entries[i] != entry)
                entries[i].Show();
        }
    }

    public void HideAllEntries() {
        for (int i = 0; i < entries.Count; i++) {
            entries[i].Hide();
        }
    }


    public void OnClassSelected(CharacterChoiceEntry entry) {
        selectedClass = entry;

        HideAllEntries();
        new Task(FadeoutInfoPanels());
        ShowStarterPackages();

        //ItemSpawner.SpawnItem(entry.ClassItem, transform.position, true);
        //ItemSpawner.SpawnItem(entry.ChosenItem, transform.position, true);
        //Close();

        //RoomManager.SpawnRoomPortals();

        //EntityManager.ActivePlayer.Inventory.AddEXP(25f);
        //PanelManager.OpenPanel<HotbarPanel>();
        //PanelManager.OpenPanel<HUDPanel>();

        //PanelManager.OpenPanel<LevelUpPanel>();
    }

    public void StartGame() {
        Close();
        MasteryManager.Instance.LoadSavedMasteries();
        RoomManager.SpawnRoomPortals();

        EntityManager.ActivePlayer.Inventory.AddEXP(25f);
        PanelManager.OpenPanel<HotbarPanel>();
        PanelManager.OpenPanel<HUDPanel>();

        PanelManager.OpenPanel<LevelUpPanel>();
    }



    private void ShowStarterPackages() {
        //WaitForSeconds waiter = new WaitForSeconds(0.3f);
        rayBlocker.SetActive(true);
        starterPackageEntries.ClearList();

        List<ItemDefinition> starterSkills = new List<ItemDefinition>(ItemSpawner.Instance.lootDatabase.GetRandomSkillsByTag(selectedClass.ClassItem.itemData.abilityTags));
        List<ItemDefinition> starterWeapons = new List<ItemDefinition>(selectedClass.ClassItem.itemData.startingItemOptions);
        starterWeapons.Shuffle();
        starterSkills.Shuffle();

        if (starterWeapons.Count < 4) {
            Debug.LogError("Less than 4 starter Weapons Found on " + selectedClass.ClassItem.itemData.itemName);
            //yield return null;
            return;
        }

        if (starterSkills.Count < 4) {
            Debug.LogError("Less than 4 starter skills Found on " + selectedClass.ClassItem.itemData.itemName);
            //yield return null;
            return;
        }


        for (int i = 0; i < 4; i++) {
            CreateStarterPackage(starterWeapons[i], starterSkills[i]);
        }

        new Task(FadeInPackages());

    }

    private IEnumerator FadeInPackages() {
        WaitForSeconds waiter = new WaitForSeconds(0.2f);

        for (int i = 0; i < starterPackageEntries.Count; i++) {
            UIHelper.FadeInObject(starterPackageEntries[i].gameObject, 1, 0.4f);
            starterPackageEntries[i].ShowShimmer();
            yield return waiter;
            
        }
    }

    private void CreateStarterPackage(params ItemDefinition[] items) {
        CharacterStarterPackageEntry starterPackage = Instantiate(starterPackageTemplate, starterPackageHolder);
        starterPackage.gameObject.SetActive(true);
        starterPackage.Setup(this, items);
        starterPackageEntries.Add(starterPackage);
    }
}
