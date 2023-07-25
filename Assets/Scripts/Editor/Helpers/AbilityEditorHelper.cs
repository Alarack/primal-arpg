using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class AbilityEditorHelper 
{
    public const string abilityHeader = "Ability Header";
    public const string triggerHeader = "Trigger Header";
    public const string effectHeader = "Effect Header";
    public const string errorLabel = "Error Label";


    public static AbilityData DrawAbilityData(AbilityData entry) {
        EditorGUILayout.Separator();

        string placeholderName = string.IsNullOrEmpty(entry.abilityName) == true ? "New Ability" : entry.abilityName;
        EditorGUILayout.LabelField(placeholderName, EditorHelper2.LoadStyle(abilityHeader));

        EditorGUILayout.Separator();
        entry.abilityIcon = EditorHelper.ObjectField("Sprite", entry.abilityIcon);
        entry.category = EditorHelper.EnumPopup("Category", entry.category);
        entry.tags = EditorHelper.DrawList("Tags", "Tag", entry.tags, AbilityTag.None, EditorHelper.DrawListOfEnums);
        
        if (entry.category == AbilityCategory.Rune) {
            entry.runeAbilityTarget = EditorGUILayout.TextField("Ability Target", entry.runeAbilityTarget);
        }

        entry.abilityName = EditorGUILayout.TextField("Ability Name", entry.abilityName);
        entry.abilityDescription = EditorGUILayout.TextField("Description", entry.abilityDescription);
        entry.resourceCost = EditorGUILayout.FloatField("Resource Cost", entry.resourceCost);
        entry.baseRuneSlots = EditorGUILayout.IntField("Rune Slots", entry.baseRuneSlots);

        entry.includeEffectsInTooltip = EditorGUILayout.Toggle("Include Effect Toolip", entry.includeEffectsInTooltip);
        entry.ignoreTooltip = EditorGUILayout.Toggle("Ignore Tooltip", entry.ignoreTooltip);
        entry.autoFire = EditorGUILayout.Toggle("Autofire", entry.autoFire);

        entry.suspend = EditorGUILayout.Toggle("Suspend?", entry.suspend);

       
        
        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Child Abilities: ", EditorStyles.boldLabel);

        entry.childAbilities = EditorHelper.DrawList(null, entry.childAbilities, null, DrawAbilityDefinitionList);

        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Activation Triggers: ", EditorStyles.boldLabel);
        DrawTriggerCounterData(entry.counterData);

        entry.activationTriggerData = EditorHelper.DrawExtendedList(entry.activationTriggerData, "Trigger", DrawTriggerData);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Effects: ", EditorStyles.boldLabel);

        entry.effectDefinitions = EditorHelper.DrawList(null, entry.effectDefinitions, null, DrawEffectDefinitionList);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("End Triggers: ", EditorStyles.boldLabel);
        DrawTriggerCounterData(entry.endCounterData);

        entry.endTriggerData = EditorHelper.DrawExtendedList(entry.endTriggerData, "Trigger", DrawTriggerData);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Recoveries: ", EditorStyles.boldLabel);
        entry.startingRecoveryCharges = EditorGUILayout.IntField("Starting Charges", entry.startingRecoveryCharges);

        entry.recoveryData = EditorHelper.DrawExtendedList(entry.recoveryData, "Recovery", DrawRecoveryData);
        EditorGUILayout.EndVertical();
        return entry;
    }



    public static EffectDefinition DrawEffectDefinitionList(List<EffectDefinition> list, int index) {
        EffectDefinition result = EditorHelper.ObjectField(list[index]);

        return result;
    }

    public static AbilityDefinition DrawAbilityDefinitionList(List<AbilityDefinition> list, int index) {
        AbilityDefinition result = EditorHelper.ObjectField(list[index]);

        return result;
    }


    public static TriggerData DrawTriggerData(TriggerData entry) {

        string placeholderTriggerName = "Start of Trigger: " + ObjectNames.NicifyVariableName(entry.type.ToString()) + " section";
        EditorGUILayout.LabelField(placeholderTriggerName, EditorHelper2.LoadStyle(triggerHeader));

        entry.type = EditorHelper.EnumPopup("Trigger Type", entry.type);

        if (entry.type == TriggerType.Rider) {
            entry.riderAbilityName = EditorGUILayout.TextField("Target Ability", entry.riderAbilityName);
            entry.riderEffectName = EditorGUILayout.TextField("Target Effect", entry.riderEffectName);
        }

        EditorGUILayout.LabelField("Constraints", EditorStyles.boldLabel);
        DrawTriggerConstrains(entry);

        foreach (ConstraintDataFocus dataList in entry.allConstraints) {
            DrawConstraintDataFocus(dataList);
        }

        EditorGUILayout.LabelField("End of Trigger: " + ObjectNames.NicifyVariableName(entry.type.ToString()) + " section", EditorHelper2.LoadStyle(triggerHeader) /*triggerHeaderStyle*/);


        return entry;
    }

    public static void DrawTriggerCounterData(TriggerActivationCounterData entry) {
        entry.limitedNumberOfTriggers = EditorGUILayout.Toggle("Limit Activations?", entry.limitedNumberOfTriggers);
        entry.requireMultipleTriggers = EditorGUILayout.Toggle("Require Multiple Activations?", entry.requireMultipleTriggers);

        if (entry.requireMultipleTriggers == true)
            entry.minTriggerCount = EditorGUILayout.IntField("Activations Required", entry.minTriggerCount);

        if (entry.limitedNumberOfTriggers == true)
            entry.maxTriggerCount = EditorGUILayout.IntField("Maximum Activations", entry.maxTriggerCount);

        if (entry.requireMultipleTriggers == true || entry.limitedNumberOfTriggers == true) {
            entry.useCustomRefreshTrigger = EditorGUILayout.Toggle("Override Refresh Mode?", entry.useCustomRefreshTrigger);

            if (entry.useCustomRefreshTrigger == true) {
                DrawTriggerData(entry.customRefreshTriggerData);
                EditorGUILayout.Separator();
            }
        }

        if (entry.limitedNumberOfTriggers == true && entry.useCustomRefreshTrigger == false) {
            EditorGUILayout.LabelField("Enable override refresh mode to allow this counter to reset.", EditorHelper2.LoadStyle(errorLabel));

        }

        if (entry.limitedNumberOfTriggers == true && entry.requireMultipleTriggers == true) {
            EditorGUILayout.LabelField("The System does not currently support both min and max trigger limits", EditorHelper2.LoadStyle(errorLabel));
        }

    }

    public static void DrawTriggerConstrains(TriggerData data) {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Entity Triggers")) {
            if (data.HasConstraintListOfType(ConstraintFocus.Trigger) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.Trigger);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.Trigger));
            }
        }

        if (GUILayout.Button("Entity Sources")) {
            if (data.HasConstraintListOfType(ConstraintFocus.Source) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.Source);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.Source));
            }
        }

        if (GUILayout.Button("Entity Causes")) {
            if (data.HasConstraintListOfType(ConstraintFocus.Cause) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.Cause);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.Cause));
            }
        }

        if (GUILayout.Button("Ability Triggers")) {
            if (data.HasConstraintListOfType(ConstraintFocus.AbilityTrigger) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.AbilityTrigger);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.AbilityTrigger));
            }
        }

        if (GUILayout.Button("Ability Sources")) {
            if (data.HasConstraintListOfType(ConstraintFocus.AbilitySource) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.AbilitySource);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.AbilitySource));
            }
        }

        if (GUILayout.Button("Ability Causes")) {
            if (data.HasConstraintListOfType(ConstraintFocus.AbiityCause) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.AbiityCause);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.AbiityCause));
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    public static void DrawConstraintDataFocus(ConstraintDataFocus entry) {
        EditorGUILayout.LabelField(entry.focus.ToString() + " Constraints", EditorStyles.boldLabel);
        entry.constraintData = EditorHelper.DrawExtendedList(entry.constraintData, entry.focus.ToString() + " Constraint", DrawConstraintData);
    }

    public static ConstraintData DrawConstraintData(ConstraintData entry) {

        entry.type = EditorHelper.EnumPopup("Constraint Type", entry.type);

        switch (entry.type) {
            case ConstraintType.Owner:
                entry.ownerTarget = EditorHelper.EnumPopup("Owner", entry.ownerTarget);
                break;
            case ConstraintType.PrimaryType:
                entry.targetPrimaryType = EditorHelper.EnumPopup("Primary Type", entry.targetPrimaryType);
                break;
            case ConstraintType.Subtype:
                entry.targetSubtype = EditorHelper.EnumPopup("Subtype", entry.targetSubtype);
                break;
            case ConstraintType.StatMinimum:
                entry.minStatTarget = EditorHelper.EnumPopup("Stat Name", entry.minStatTarget);
                entry.minStatValue = EditorGUILayout.FloatField("Min Value", entry.minStatValue);
                break;
            case ConstraintType.StatMaximum:
                entry.maxStatTarget = EditorHelper.EnumPopup("Stat Name", entry.maxStatTarget);
                entry.maxStatValue = EditorGUILayout.FloatField("Min Value", entry.maxStatValue);
                break;
            case ConstraintType.SourceOnly:
                break;
            case ConstraintType.StatChanged:
            //case ConstraintType.UnitStatDecreased:
            //case ConstraintType.UnitStatIncreased:
                entry.statChangeTarget = EditorHelper.EnumPopup("Target Stat", entry.statChangeTarget);
                entry.changeDirection = EditorHelper.EnumPopup("Change Direction", entry.changeDirection);

                break;
            case ConstraintType.Range:
                entry.rangeToWhat = EditorHelper.EnumPopup("Range to What?", entry.rangeToWhat);
                entry.minRange = EditorGUILayout.FloatField("Min Range", entry.minRange);
                entry.maxRange = EditorGUILayout.FloatField("Max Range", entry.maxRange);
                break;
            case ConstraintType.UnitDamaged:
                EditorGUILayout.LabelField("Not Implemented", EditorHelper2.LoadStyle(errorLabel));
                break;
            case ConstraintType.MostStat:
                entry.mostStatTarget = EditorHelper.EnumPopup("Stat Name", entry.mostStatTarget);
                entry.mostStatAbilityName = EditorGUILayout.TextField("Gather Ability Name", entry.mostStatAbilityName);
                entry.mostStatEffectName = EditorGUILayout.TextField("Gather Effect Name", entry.mostStatEffectName);
                break;
            case ConstraintType.LeastStat:
                entry.leastStatTarget = EditorHelper.EnumPopup("Stat Type", entry.leastStatTarget);
                entry.leastStatAbilityName = EditorGUILayout.TextField("Gather Ability Name", entry.leastStatAbilityName);
                entry.leastStatEffectName = EditorGUILayout.TextField("Gather Effect Name", entry.leastStatEffectName);
                break;
            case ConstraintType.AbilityActive:
                entry.targetActiveAbilityName = EditorGUILayout.TextField("Target Ability Name", entry.targetActiveAbilityName);
                break;
            case ConstraintType.EffectApplied:
                entry.appliedEffectType = EditorHelper.EnumPopup("Effect Type", entry.appliedEffectType);
                //entry.onlyTargetedEffects = EditorGUILayout.Toggle("Only Targeted", entry.onlyTargetedEffects);
                //entry.constraintByEffectName = EditorGUILayout.Toggle("Constrain by Name", entry.constraintByEffectName);
                //if (entry.constraintByEffectName == true) {
                //    entry.appliedEffectName = EditorGUILayout.TextField("Effect Name", entry.appliedEffectName);

                //}
                break;
            case ConstraintType.EntityName:
                entry.targetEntityName = EditorGUILayout.TextField("Card Name", entry.targetEntityName);
                break;
            //case ConstraintType.RiderHasTargets:
            //    break;
            //case ConstraintType.IsCause:
            //    break;
            //case ConstraintType.IsDead:
            case ConstraintType.EffectName:
                entry.targetEffectName = EditorGUILayout.TextField("Effect Name", entry.targetEffectName);
                break;

            case ConstraintType.AbilityName:
                entry.targetAbiltyName = EditorGUILayout.TextField("Ability Name", entry.targetAbiltyName);
                break;

            case ConstraintType.EffectDesignation:
                entry.effectDesigantion = EditorHelper.EnumPopup("Designation", entry.effectDesigantion);
                break;

            case ConstraintType.HasStatus:
                entry.targetStatus = EditorHelper.EnumPopup("Status", entry.targetStatus);
                break;

            case ConstraintType.AbilityTag:
                entry.targetAbilityTag = EditorHelper.EnumPopup("Tag", entry.targetAbilityTag);
                break;
            default:
                break;
        }


        entry.inverse = EditorGUILayout.Toggle("Inverse?", entry.inverse);


        return entry;
    }

    public static RecoveryData DrawRecoveryData(RecoveryData entry) {

        entry.type = EditorHelper.EnumPopup("Type", entry.type);

        if(entry.type == RecoveryType.Timed) {
            entry.cooldown = EditorGUILayout.FloatField("Cooldown", entry.cooldown);
        }
        else {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Recovery Triggers: ", EditorStyles.boldLabel);
            DrawTriggerCounterData(entry.counterData);

            entry.recoveryTriggers = EditorHelper.DrawExtendedList(entry.recoveryTriggers, "Trigger", DrawTriggerData);

            EditorGUILayout.EndVertical();
        }

        

        return entry;
    }

}
