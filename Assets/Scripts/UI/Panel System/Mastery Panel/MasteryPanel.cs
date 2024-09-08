using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasteryPanel : BasePanel
{

    [Header("Template")]
    public MasteryEntry template;
    public Transform holder;


    private List<MasteryEntry> entries = new List<MasteryEntry>();


    protected override void Awake() {
        base.Awake();
        template.gameObject.SetActive(false);
    }

    public override void Open() {
        base.Open();

        SetupDisplay();
    }

    private void SetupDisplay() {
        PopulateMasteries();
    }

    private void PopulateMasteries() {
        entries.PopulateList(GameManager.Instance.masteryDatabase.masteryData.Count, template, holder, true);
        for (int i = 0; i < entries.Count; i++) {
            entries[i].Setup(GameManager.Instance.masteryDatabase.masteryData[i]);
        }
    }
}
