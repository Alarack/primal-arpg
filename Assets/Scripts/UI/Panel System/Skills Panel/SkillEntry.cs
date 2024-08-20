using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using GameButtonType = InputHelper.GameButtonType;

public class SkillEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler {

    public enum SkillEntryLocation {
        ActiveSkill,
        KnownSkill,
        Hotbar,
        RunePanel,
        ActivePassive,
        KnownPassive,
        ClassFeatureSkill
    }


    [Header("Images & Text")]
    public Image icon;
    public Image dimmer;
    public Image buttonPromptImage;
    public TextMeshProUGUI buttonPromptText;
    public TextMeshProUGUI chargesText;
    public SkillEntryLocation location;
    public GameButtonType keybind;

    [Header("Holders")]
    public GameObject activeHolder;
    public GameObject passiveHolder;

    [Header("Passive Variant")]
    public Image passiveIcon;
    public Image selecteFrame;

    [Header("Rune Pips")]
    public Color equippedColor;
    public Color unequippedColor;
    public Image runePipImageTemplate;
    public Transform runePipHolder;

    private List<Image> runePipEntries = new List<Image>();

    //public GameInput.GameButtonType keyBind;

    //[Header("Slot Elements")]
    //public GameObject emptySlot;
    //public GameObject filledSlot;

    public Ability Ability { get; protected set; }
    public int Index { get; protected set; }

    public static SkillEntry draggedEntry;

    public bool IsPassive { get; protected set; }

    private Canvas canvas;
    private int baseLayer;

    private void Awake() {
        canvas = GetComponent<Canvas>();
        baseLayer = canvas.sortingOrder;
        dimmer.fillAmount = 0f;

        runePipImageTemplate.gameObject.SetActive(false);
    }

    private void OnDisable() {
        if (Ability == null || location != SkillEntryLocation.Hotbar || Ability.MaxCharges < 2)
            return;

        Ability.RemoveChargesChangedListener(OnAbilityChargesChanges);

    }

    private void Update() {
        ShowCooldownDimmer();
    }

    public void Setup(Ability ability, SkillEntryLocation location, bool passive, GameButtonType keyBind = GameButtonType.None, int index = -1) {
        this.Ability = ability;
        this.keybind = keyBind;
        SetupAbilityIcon(ability);

        this.location = location;
        if ((location == SkillEntryLocation.Hotbar || location == SkillEntryLocation.ActiveSkill) && index > 0)
            Index = index;

        SetupCharges();

        IsPassive = passive;

        if(passive == true) {
            passiveHolder.SetActive(true);
            activeHolder.SetActive(false);
        }
        else {
            passiveHolder.SetActive(false);
            activeHolder.SetActive(true);
        }

        SetupRunePips();
    }

    public void SetupRunePips() {
        if (location != SkillEntryLocation.ActiveSkill && location != SkillEntryLocation.ActivePassive) {
            runePipEntries.ClearList();
            return;
        }
            

        if (Ability == null) {
            runePipEntries.ClearList();
            return;
        }
            

        runePipEntries.PopulateList(Ability.GetMaxRunes(), runePipImageTemplate, runePipHolder, true);

        for (int i = 0; i < Ability.GetEquippedRuneCount(); i++) {
            runePipEntries[i].color = equippedColor;
        }

        for (int i = 0; i < runePipEntries.Count; i++) {
            if (i < Ability.GetEquippedRuneCount()) {
                runePipEntries[i].color = equippedColor;
            }
            else {
                runePipEntries[i].color = unequippedColor;
            }
        }
    }

    private void SetupCharges() {
        if (Ability == null || location != SkillEntryLocation.Hotbar || Ability.MaxCharges < 2) {
            chargesText.gameObject.SetActive(false);
            //Debug.Log("NO Charges");
            return;
        }

        chargesText.gameObject.SetActive(true);
        chargesText.text = Ability.MaxCharges.ToString();

        Ability.AddChargesChangedListener(OnAbilityChargesChanges);

    }

    private void OnAbilityChargesChanges(BaseStat stat, object source, float value) {
        if (Ability == null) {
            chargesText.gameObject.SetActive(false);
            return;
        }

        chargesText.text = Ability.Charges.ToString();
    }

    private void SetupAbilityIcon(Ability ability) {

        if (ability != null) {
            this.Ability = ability;
            icon.gameObject.SetActive(true);
            icon.sprite = ability.Data.abilityIcon;
            passiveIcon.gameObject.SetActive(true);
            passiveIcon.sprite = ability.Data.abilityIcon;
        }
        else {
            icon.gameObject.SetActive(false);
            passiveIcon.gameObject.SetActive(false);

        }
    }

    public void AssignNewAbility(Ability ability) {
        this.Ability = ability;
        SetupAbilityIcon(ability);
        SetupCharges();
        SetupRunePips();
    }

    private void ShowCooldownDimmer() {
        if (Ability == null)
            return;

        if (location != SkillEntryLocation.Hotbar)
            return;

        dimmer.fillAmount = Mathf.Abs(Ability.GetCooldownRatio() - 1);

        if (Ability.IsReady == true && dimmer.fillAmount != 0)
            dimmer.fillAmount = 0;
    }

    public void SelectPassive() {
        selecteFrame.gameObject.SetActive(true);
    }

    public void DeselectPassive() {
        selecteFrame.gameObject.SetActive(false);
    }

    #region UI CALLBACKS


    public void OnPointerEnter(PointerEventData eventData) {
        if (Ability == null) {
            return;
        }

        TooltipManager.Show(Ability.GetTooltip(), Ability.Data.abilityName);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Hide();
    }

    public void OnPointerClick(PointerEventData eventData) {

        if (location == SkillEntryLocation.RunePanel)
            return;


        if (eventData.button == PointerEventData.InputButton.Right) {

            if (Ability != null && Ability.IsEquipped == true)
                PanelManager.OpenPanel<RunesPanel>().Setup(Ability);

            //SkillModifier embiggen = SkillModifierFactory.MakeProjectileBigger();
            //Debug.Log("Modifying " + Skill.skillName + " " + embiggen.sizeMod);
            //Skill.AddModifier(embiggen);
        }

        if (location == SkillEntryLocation.Hotbar && eventData.button == PointerEventData.InputButton.Left) {
            if (Ability == null)
                return;

            EventData data = new EventData();
            data.AddAbility("Ability", Ability);
            EventManager.SendEvent(GameEvent.UserActivatedAbility, data);
        }


        if(eventData.button == PointerEventData.InputButton.Left) {

            if(location == SkillEntryLocation.ActivePassive || location == SkillEntryLocation.KnownPassive) {
                SkillsPanel skillsPanel = PanelManager.GetPanel<SkillsPanel>();

                if(location == SkillEntryLocation.KnownPassive) {
                    skillsPanel.OnKnownPassiveSelected(this);
                }

                if(location == SkillEntryLocation.ActivePassive) {
                    skillsPanel.OnPassiveSlotClicked(this);
                }

            }

          
        }

        //if (location == SkillEntryLocation.ActivePassive && eventData.button == PointerEventData.InputButton.Left) {
        //    SkillsPanel skillsPanel = PanelManager.GetPanel<SkillsPanel>();


        //    if (Ability == null || Ability.IsEquipped == true) {
        //      skillsPanel.OnPassiveSlotClicked(this);
        //    }

        //    if(Ability != null && Ability.IsEquipped == false) {
        //        skillsPanel.OnKnownPassiveSelected(this);
        //    }
        //}
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (CancelDrag() == true)
            return;

        //SetCanvasLayerOnTop();
        UIHelper.SetCanvasLayerOnTop(canvas);
        draggedEntry = this;
    }
    public void OnDrag(PointerEventData eventData) {
        if (CancelDrag() == true)
            return;
        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        icon.transform.position = target;

    }
    public void OnEndDrag(PointerEventData eventData) {
        //ResetCanvasLayer();
        UIHelper.ResetCanvasLayer(canvas, baseLayer);

        if (CancelDrag() == true)
            return;

        icon.transform.localPosition = Vector2.zero;
    }
    public void OnDrop(PointerEventData eventData) {

        if (draggedEntry == null)
            return;

        if (draggedEntry.Ability == null)
            return;

        if (location != SkillEntryLocation.ActiveSkill)
            return;

        //Debug.Log("Dropping " + draggedEntry.Ability.Data.abilityName + " onto " + Index);

        SkillsPanel panel = PanelManager.GetPanel<SkillsPanel>();


        SkillEntry existingSkill = panel.IsAbilityInActiveList(draggedEntry.Ability);

        if (existingSkill == null) { // Skill is not already on the active bar.
            if (Ability != null) { // Target slot is not empty.
                EntityManager.ActivePlayer.AbilityManager.UnequipAbility(Ability, Index);
            }
            EntityManager.ActivePlayer.AbilityManager.EquipAbility(draggedEntry.Ability, Index);
        }
        else { // Skill is already on the active bar, so it's a movement we're doing.

            if (draggedEntry.location == SkillEntryLocation.KnownSkill) // We're not dragging from all skills because skill is already on the bar.
                return; //Pretty sure this is never true

            if (Ability == null) // If the slot is empty, we can just unequip it from its old slot and equip it to the new one.
                EntityManager.ActivePlayer.AbilityManager.MoveAbilitySlot(draggedEntry.Ability, draggedEntry.Index, Index);
            else { // If the slot is not empty, we must perform a swap.
                EntityManager.ActivePlayer.AbilityManager.SwapEquippedAbilities(Ability, Index, draggedEntry.Ability, draggedEntry.Index);
            }
        }

        draggedEntry.icon.transform.localPosition = Vector2.zero;
        draggedEntry = null;
    }


    private bool CancelDrag() {
        if (Ability == null)
            return true;

        if (location == SkillEntryLocation.Hotbar || 
            location == SkillEntryLocation.RunePanel ||
            location == SkillEntryLocation.ClassFeatureSkill)
            return true;

        return false;
    }

    private void SetCanvasLayerOnTop() {
        if (canvas.overrideSorting == true && canvas.sortingOrder == 100)
            return;

        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
    }

    private void ResetCanvasLayer() {
        canvas.sortingOrder = baseLayer;
        canvas.overrideSorting = false;
    }


    #endregion
}
