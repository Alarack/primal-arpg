using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Michsky.MUIP;

public class CharacterChoiceEntry : MonoBehaviour
{

    public Image characterImgage;
    public Image flashImage;
    public TextMeshProUGUI shortDescriptionText;
    public TextMeshProUGUI bulletPointsText;
    public TextMeshProUGUI classNameText;
    public Transform bgAnchor;
    public ButtonManager infoButton;

    [Header("Template")]
    public SkillPreviewEntry template;
    public Transform holder;
    public WeaponSelectionEntry weaponSelectionTemplate;
    public Transform weaponSelectionHolder;

    public ItemDefinition ClassItem { get; private set; }

    private List<SkillPreviewEntry> skillPreviewEntries = new List<SkillPreviewEntry>();
    private List<WeaponSelectionEntry> weaponSelectionEntries = new List<WeaponSelectionEntry>();
    private CharacterSelectPanel selectionPanel;
    private WeaponSelectionEntry chosenWeapon;

    public ItemData ChosenItem { get { return chosenWeapon != null ? chosenWeapon.MyItem.itemData : null; } }

    private void Awake() {
        template.gameObject.SetActive(false);
        weaponSelectionTemplate.gameObject.SetActive(false);
    }

    public void Setup(ItemDefinition item, CharacterSelectPanel selectionPanel) {
        ClassItem = item;
        this.selectionPanel = selectionPanel;
        infoButton.onClick.AddListener(OnInfoClicked);
        SetupDisplay();

        FlashWhite();
        bgAnchor.DOLocalMove(Vector2.zero, 0.4f).SetEase(Ease.OutSine);
    }

    private void FlashWhite() {
        flashImage.color = Color.white;
        flashImage.DOColor(Color.clear, 1f);
    }

    public void Select() {
        FlashWhite();
        bgAnchor.DOMove(selectionPanel.selectedClassAnchor.position, 0.7f).SetEase(Ease.OutSine);
        infoButton.onClick.RemoveListener(OnInfoClicked);
        infoButton.onClick.AddListener(Deselect);
        infoButton.SetText("Back");

        Debug.Log("Selecting: " + ClassItem.itemData.itemName);
    }

    public void Deselect() {
        FlashWhite();
        bgAnchor.DOLocalMove(Vector2.zero, 0.4f).SetEase(Ease.OutSine);

        infoButton.onClick.RemoveListener(Deselect);
        infoButton.onClick.AddListener(OnInfoClicked);
        infoButton.SetText("Info");

        Debug.Log("Deselecting: " + ClassItem.itemData.itemName);

    }

    private void SetupDisplay() {
        characterImgage.sprite = ClassItem.itemData.itemIcon;

        shortDescriptionText.text = ClassItem.itemData.itemDescription;
        bulletPointsText.text = ClassItem.itemData.secondaryDescription;
        classNameText.text = ClassItem.itemData.itemName;

        skillPreviewEntries.PopulateList(ClassItem.itemData.classPreviewAbilities.Count, template, holder, true);
        for (int i = 0; i < skillPreviewEntries.Count; i++) {
            skillPreviewEntries[i].Setup(ClassItem.itemData.classPreviewAbilities[i]);
        }

        weaponSelectionEntries.PopulateList(ClassItem.itemData.startingItemOptions.Count, weaponSelectionTemplate, weaponSelectionHolder, true);
        for (int i = 0; i < weaponSelectionEntries.Count; i++) {
            weaponSelectionEntries[i].Setup(ClassItem.itemData.startingItemOptions[i], this);
        }

        OnWeaponSelected(weaponSelectionEntries[0]);
    }

    public void OnInfoClicked() {
        selectionPanel.OnClassInfoClicked(this);
    }

    public void OnSelectClicked() {
        selectionPanel.OnClassSelected(this);
    }

    public void OnWeaponSelected(WeaponSelectionEntry weapon) {
        chosenWeapon = weapon;
        for (int i = 0; i < weaponSelectionEntries.Count; i++) {
            weaponSelectionEntries[i].Deselect();
        }

        chosenWeapon.Select();
    }

}
