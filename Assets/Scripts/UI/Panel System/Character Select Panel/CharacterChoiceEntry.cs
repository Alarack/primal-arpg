using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterChoiceEntry : MonoBehaviour
{

    public Image characterImgage;
    public TextMeshProUGUI shortDescriptionText;
    public TextMeshProUGUI bulletPointsText;
    public TextMeshProUGUI classNameText;

    [Header("Template")]
    public SkillPreviewEntry template;
    public Transform holder;


    public ItemDefinition ClassItem { get; private set; }

    private List<SkillPreviewEntry> skillPreviewEntries = new List<SkillPreviewEntry>();
    private CharacterSelectPanel selectionPanel;

    private void Awake() {
        template.gameObject.SetActive(false);
    }

    public void Setup(ItemDefinition item, CharacterSelectPanel selectionPanel) {
        ClassItem = item;
        this.selectionPanel = selectionPanel;
        SetupDisplay();
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
    }

    public void OnSelectClicked() {
        selectionPanel.OnClassSelected(this);
    }

}
