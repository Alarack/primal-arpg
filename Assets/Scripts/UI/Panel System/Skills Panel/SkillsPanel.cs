using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsPanel : BasePanel
{

    public Transform activeSkillsHolder;
    public Transform knownSkillsHolder;
    public SkillEntry skillEntryTemplate;

    private List<SkillEntry> activeSkillEntries = new List<SkillEntry>(); 
    private List<SkillEntry> knownSkillEntries = new List<SkillEntry>();

    public SkillEntry this[int i] { get { return activeSkillEntries[i]; } }


    protected override void Start() {
        base.Start();

        CreateEmptySlots();
    }

    public override void Open() {
        base.Open();

        AbilityUtilities.PopulateSkillEntryList(ref knownSkillEntries, skillEntryTemplate, knownSkillsHolder, SkillEntry.SkillEntryLocation.KnownSkill);
    }

    public override void Close() {
        base.Close();
        TooltipManager.Hide();
    }


    protected override void OnEnable() {
        base.OnEnable();
        EntityManager.ActivePlayer.AbilityManager.onAbilityEquipped += OnAbilityEquipped;
        EntityManager.ActivePlayer.AbilityManager.onAbilityUnequipped += OnAbilityUnequipped;
        EntityManager.ActivePlayer.AbilityManager.onAbilitySwapped += OnAbilitySwapped;
    }

    protected override void OnDisable() {
        base.OnDisable();
        EntityManager.ActivePlayer.AbilityManager.onAbilityEquipped -= OnAbilityEquipped;
        EntityManager.ActivePlayer.AbilityManager.onAbilityUnequipped -= OnAbilityUnequipped;
        EntityManager.ActivePlayer.AbilityManager.onAbilitySwapped -= OnAbilitySwapped;
    }

    private void CreateEmptySlots() {
        AbilityUtilities.CreateEmptySkillEntries(ref activeSkillEntries, 6, skillEntryTemplate, activeSkillsHolder, SkillEntry.SkillEntryLocation.ActiveSkill);
    }

    public SkillEntry IsAbilityInActiveList(Ability ability) {
        return AbilityUtilities.GetSkillEntryByAbility(activeSkillEntries, ability);
    }


    #region EVENTS

    private void OnAbilityEquipped(Ability ability, int index) {
        activeSkillEntries[index].AssignNewAbility(ability);
    }

    private void OnAbilityUnequipped(Ability ability, int index) {
        activeSkillEntries[index].AssignNewAbility(null);
    }

    private void OnAbilitySwapped(Ability first, int firstIndex, Ability second, int secondIndex) {
        //activeSkillEntries[firstIndex].AssignNewAbility(null);
        //activeSkillEntries[secondIndex].AssignNewAbility(null);
        activeSkillEntries[firstIndex].AssignNewAbility(second);
        activeSkillEntries[secondIndex].AssignNewAbility(first);
    }
    #endregion
}
