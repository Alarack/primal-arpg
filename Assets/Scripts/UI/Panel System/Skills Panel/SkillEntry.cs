using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    //public GameInput.GameButtonType keyBind;

    [Header("Slot Elements")]
    public GameObject emptySlot;
    public GameObject filledSlot;

    public Ability Ability { get; protected set; }
    public int Index { get; protected set; }

    public static SkillEntry draggedEntry;



    private Canvas canvas;

    private void Awake() {
        canvas = GetComponent<Canvas>();
    }


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

        //Debug.Log("Dropping " + draggedEntry.Skill.skillName + " onto " + Index);

        //SkillsPanel panel = PanelManager.GetPanel<SkillsPanel>();


        //SkillEntry existingSkill = panel.IsSkillInActiveMenu(draggedEntry.Skill);

        //if (existingSkill == null) { // Skill is not already on the active bar.
        //    if (Ability != null) { // Target slot is empty.
        //        EntityManager.Player.SkillManager.UnequipSkill(Skill, Index);
        //    }
        //    EntityManager.Player.SkillManager.EquipSkill(draggedEntry.Skill, Index);
        //}
        //else { // Skill is already on the active bar, so it's a movement we're doing.

        //    if (draggedEntry.location == SkillEntryLocation.KnownSkill) // We're not dragging from all skills because skill is already on the bar.
        //        return;

        //    if (Skill == null) // If the slot is empty, we can just unequip it from its old slot and equip it to the new one.
        //        EntityManager.Player.SkillManager.MoveSkill(draggedEntry.Skill, draggedEntry.Index, Index);
        //    else { // If the slot is not empty, we must perform a swap.
        //        EntityManager.Player.SkillManager.SwapEquippedSkills(Skill, Index, draggedEntry.Skill, draggedEntry.Index);
        //    }
        //}

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
        canvas.sortingOrder = 16;
    }



}
