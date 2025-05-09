using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Michsky.MUIP;

public class CharacterChoiceEntry : MonoBehaviour {

    public Image characterImgage;
    public Image flashImage;
    public TextMeshProUGUI shortDescriptionText;
    public TextMeshProUGUI bulletPointsText;
    public TextMeshProUGUI classNameText;
    public Transform bgAnchor;
    public ButtonManager infoButton;
    public CanvasGroup fader;
    public GameObject devBlocker;


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
        bulletPointsText.text = ClassItem.itemData.secondaryDescription;
        shortDescriptionText.text = ClassItem.itemData.itemDescription;
        bgAnchor.DOMove(selectionPanel.selectedClassAnchor.position, 0.7f).SetEase(Ease.OutSine).onComplete = selectionPanel.StartInfoFadein;
        infoButton.onClick.RemoveListener(OnInfoClicked);
        infoButton.onClick.AddListener(Deselect);
        infoButton.SetText("Back");

        //Debug.Log("Selecting: " + ClassItem.itemData.itemName);

    }

    public void Deselect() {
        FlashWhite();
        bgAnchor.DOLocalMove(Vector2.zero, 0.4f).SetEase(Ease.OutSine);

        infoButton.onClick.RemoveListener(Deselect);
        infoButton.onClick.AddListener(OnInfoClicked);
        infoButton.SetText("Info");

        //Debug.Log("Deselecting: " + ClassItem.itemData.itemName);
        selectionPanel.UnhideAllEntries(this);
        new Task(selectionPanel.FadeoutInfoPanels());

    }

    public void Hide() {
        fader.DOFade(0f, 0.6f);
        //Debug.Log("Hiding: " + ClassItem.itemData.itemName);
    }

    public void Show() {
        fader.DOFade(1f, 0.6f);
        //Debug.Log("Showing: " + ClassItem.itemData.itemName);
    }

    private void SetupDisplay() {

        devBlocker.gameObject.SetActive(ClassItem.devItem);

        characterImgage.sprite = ClassItem.itemData.itemIcon;

        //shortDescriptionText.text = ClassItem.itemData.itemDescription;
        //bulletPointsText.text = ClassItem.itemData.secondaryDescription;
        classNameText.text = ClassItem.itemData.itemName;

        //SetupExampleSpells();

        weaponSelectionEntries.PopulateList(ClassItem.itemData.startingItemOptions.Count, weaponSelectionTemplate, weaponSelectionHolder, true);
        for (int i = 0; i < weaponSelectionEntries.Count; i++) {
            weaponSelectionEntries[i].Setup(ClassItem.itemData.startingItemOptions[i], this);
        }

        OnWeaponSelected(weaponSelectionEntries[0]);
    }

    //public void SetupExampleSpells() {
    //    skillPreviewEntries.PopulateList(ClassItem.itemData.classPreviewAbilities.Count, template, holder, true);
    //    for (int i = 0; i < skillPreviewEntries.Count; i++) {
    //        skillPreviewEntries[i].Setup(ClassItem.itemData.classPreviewAbilities[i]);
    //    }
    //}

    public void OnInfoClicked() {
        selectionPanel.OnClassInfoClicked(this);
        selectionPanel.SetupExampleSpells(ClassItem.itemData.classPreviewAbilities);
        //SetupExampleSpells();
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
