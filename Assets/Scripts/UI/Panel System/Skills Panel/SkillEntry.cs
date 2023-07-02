using LL.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using GameButtonType = InputHelper.GameButtonType;

public class SkillEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler {

    public enum SkillEntryLocation {
        ActiveSkill,
        KnownSkill,
        Hotbar
    }


    [Header("Images & Text")]
    public Image icon;
    public Image dimmer;
    public Image buttonPromptImage;
    public TextMeshProUGUI buttonPromptText;
    public SkillEntryLocation location;
    public GameButtonType keybind;
    //public GameInput.GameButtonType keyBind;

    //[Header("Slot Elements")]
    //public GameObject emptySlot;
    //public GameObject filledSlot;

    public Ability Ability { get; protected set; }
    public int Index { get; protected set; }

    public static SkillEntry draggedEntry;



    private Canvas canvas;
    private int baseLayer;

    private void Awake() {
        canvas = GetComponent<Canvas>();
        baseLayer = canvas.sortingOrder;
    }

    public void Setup(Ability ability, SkillEntryLocation location, GameButtonType keyBind = GameButtonType.None, int index = -1) {
        this.Ability = ability;
        this.keybind = keyBind;
        SetupAbilityIcon(ability);

        this.location = location;
        if ((location == SkillEntryLocation.Hotbar || location == SkillEntryLocation.ActiveSkill) && index > 0)
            Index = index;
    }

    private void SetupAbilityIcon(Ability ability) {

        if (ability != null) {
            this.Ability = ability;
            icon.gameObject.SetActive(true);
            icon.sprite = ability.Data.abilityIcon;
        }
        else {
            icon.gameObject.SetActive(false);
        }
    }

    public void AssignNewAbility(Ability ability) {
        this.Ability = ability;
        SetupAbilityIcon(ability);
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

        if (eventData.button == PointerEventData.InputButton.Right) {

            //PanelManager.OpenPanel<SkillEditPanel>(this);

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
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (CancelDrag() == true)
            return;

        SetCanvasLayerOnTop();
        draggedEntry = this;
    }
    public void OnDrag(PointerEventData eventData) {
        if (CancelDrag() == true)
            return;
        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        icon.transform.position = target;

    }
    public void OnEndDrag(PointerEventData eventData) {
        if (CancelDrag() == true)
            return;

        icon.transform.localPosition = Vector2.zero;
        ResetCanvasLayer();
    }
    public void OnDrop(PointerEventData eventData) {

        if (draggedEntry.Ability == null)
            return;

        if (location != SkillEntryLocation.ActiveSkill)
            return;

        Debug.Log("Dropping " + draggedEntry.Ability.Data.abilityName + " onto " + Index);

        SkillsPanel panel = PanelManager.GetPanel<SkillsPanel>();


        SkillEntry existingSkill = panel.IsAbilityInActiveList(draggedEntry.Ability);

        if (existingSkill == null) { // Skill is not already on the active bar.
            if (Ability != null) { // Target slot is not empty.
                EntityManager.ActivePlayer.AbilityManager.UnequipAbility(Ability, Index);
            }
            EntityManager.ActivePlayer.AbilityManager.EquipAbility(draggedEntry.Ability, Index);
        }
        else { // Skill is already on the active bar, so it's a movement we're doing.

            //if (draggedEntry.location == SkillEntryLocation.KnownSkill) // We're not dragging from all skills because skill is already on the bar.
            //    return; //Pretty sure this is never true

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

        if (location == SkillEntryLocation.Hotbar)
            return true;

        return false;
    }

    private void SetCanvasLayerOnTop() {
        if (canvas.sortingOrder == 100)
            return;

        canvas.sortingOrder = 100;
    }

    private void ResetCanvasLayer() {
        canvas.sortingOrder = baseLayer;
    }


    #endregion
}
