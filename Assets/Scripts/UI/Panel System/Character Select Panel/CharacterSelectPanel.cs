using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CharacterSelectPanel : BasePanel
{

    [Header("Template")]
    public CharacterChoiceEntry template;
    public Transform holder;
    public SkillPreviewEntry skillPreviewTemplate;
    public Transform skillPreviewHolder;

    [Header("Anchors & Faders")]
    public Transform selectedClassAnchor;
    public CanvasGroup bulletPointsFader;
    public CanvasGroup exampleSpellsFader;

    [Header("Class Data")]
    public List<ItemDefinition> classItems = new List<ItemDefinition>();

    private List<CharacterChoiceEntry> entries = new List<CharacterChoiceEntry>();


    private CharacterChoiceEntry selectedClass;

    private List<SkillPreviewEntry> skillPreviewEntries = new List<SkillPreviewEntry>();

    protected override void Awake() {
        base.Awake();
        template.gameObject.SetActive(false);
    }
    
    public override void Open() {
        base.Open();

        new Task(PopulateClassOptions());
        bulletPointsFader.alpha = 0f;
        exampleSpellsFader.alpha = 0f;  

        //entries.PopulateList(classItems.Count, template, holder, true);
        //for (int i = 0; i < entries.Count; i++) {
        //    entries[i].Setup(classItems[i], this);
        //}
    }

    public override void Close() {
        base.Close();

        TooltipManager.Hide();
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


    public void OnClassSelected(CharacterChoiceEntry entry) {
        ItemSpawner.SpawnItem(entry.ClassItem, transform.position, true);
        ItemSpawner.SpawnItem(entry.ChosenItem, transform.position, true);
        Close();

        RoomManager.SpawnRoomPortals();

        EntityManager.ActivePlayer.Inventory.AddEXP(25f);
        PanelManager.OpenPanel<HotbarPanel>();
        PanelManager.OpenPanel<HUDPanel>();
        
        PanelManager.OpenPanel<LevelUpPanel>();
    }


}
