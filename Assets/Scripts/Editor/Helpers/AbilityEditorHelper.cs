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
            entry.runeAbilityTarget = EditorGUILayout.TextField("Rune Target Ability Name", entry.runeAbilityTarget);
        }

        entry.abilityName = EditorGUILayout.TextField("Ability Name", entry.abilityName);
        entry.abilityDescription = EditorGUILayout.TextField("Description", entry.abilityDescription);
        //entry.resourceCost = EditorGUILayout.FloatField("Resource Cost", entry.resourceCost);
        //entry.baseRuneSlots = EditorGUILayout.IntField("Rune Slots", entry.baseRuneSlots);

        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("Stats", EditorHelper2.LoadStyle(abilityHeader));
        entry.abilityStatData = EditorHelper.DrawExtendedList(entry.abilityStatData, "Stat", DrawStatData);
        EditorGUI.indentLevel--;

        if(entry.ContainsStat(StatName.AbilityWindupTime)) {
            entry.windupVFX = EditorHelper.ObjectField("WindupVFX", entry.windupVFX);
        }


        entry.includeEffectsInTooltip = EditorGUILayout.Toggle("Include Effect Toolip", entry.includeEffectsInTooltip);
        entry.ignoreTooltip = EditorGUILayout.Toggle("Ignore Tooltip", entry.ignoreTooltip);
        entry.autoFire = EditorGUILayout.Toggle("Autofire", entry.autoFire);
        entry.ignoreOtherCasting = EditorGUILayout.Toggle("Ignore Other Casting", entry.ignoreOtherCasting);

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





    public static T DrawListOfScriptableObjects<T>(List<T> list, int index) where T : ScriptableObject {
        T result = EditorHelper.ObjectField(list[index]);

        return result;
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

        string nullCheck = entry == null ? "" : entry.type.ToString();

        string placeholderTriggerName = "Start of Trigger: " + ObjectNames.NicifyVariableName(nullCheck + " section");
        EditorGUILayout.LabelField(placeholderTriggerName, EditorHelper2.LoadStyle(triggerHeader));

        entry.type = EditorHelper.EnumPopup("Trigger Type", entry.type);
        entry.delay1Frame = EditorGUILayout.Toggle("Delay 1 Frame", entry.delay1Frame);
        if (entry.type == TriggerType.Rider) {
            entry.riderAbilityName = EditorGUILayout.TextField("Target Ability", entry.riderAbilityName);
            entry.riderEffectName = EditorGUILayout.TextField("Target Effect", entry.riderEffectName);
        }

        if(entry.type == TriggerType.Timed) {
            entry.triggerTimerDuration = EditorGUILayout.FloatField("Duration", entry.triggerTimerDuration);
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
        if (GUILayout.Button("E Triggers")) {
            if (data.HasConstraintListOfType(ConstraintFocus.Trigger) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.Trigger);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.Trigger));
            }
        }

        if (GUILayout.Button("E Sources")) {
            if (data.HasConstraintListOfType(ConstraintFocus.Source) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.Source);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.Source));
            }
        }

        if (GUILayout.Button("E Causes")) {
            if (data.HasConstraintListOfType(ConstraintFocus.Cause) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.Cause);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.Cause));
            }
        }

        if (GUILayout.Button("A Triggers")) {
            if (data.HasConstraintListOfType(ConstraintFocus.AbilityTrigger) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.AbilityTrigger);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.AbilityTrigger));
            }
        }

        if (GUILayout.Button("A Sources")) {
            if (data.HasConstraintListOfType(ConstraintFocus.AbilitySource) == false) {
                ConstraintDataFocus newList = new ConstraintDataFocus(ConstraintFocus.AbilitySource);
                data.allConstraints.Add(newList);
            }
            else {
                data.allConstraints.Remove(data.GetListByType(ConstraintFocus.AbilitySource));
            }
        }

        if (GUILayout.Button("A Causes")) {
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
        EditorGUILayout.LabelField(ObjectNames.NicifyVariableName( entry.focus.ToString()) + " Constraints", EditorStyles.boldLabel);
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

            case ConstraintType.EffectType:
                entry.targetEffectType = EditorHelper.EnumPopup("Effect Type", entry.targetEffectType);
                break;

            case ConstraintType.StatRatio:
                entry.statRatioTarget = EditorHelper.EnumPopup("Target Stat", entry.statRatioTarget);
                entry.targetRatio = EditorGUILayout.FloatField("Ratio", entry.targetRatio);
                break;

            case ConstraintType.UnitIsMoving:
                entry.movementMagnitudeLimit = EditorGUILayout.FloatField("Magnitude Limit", entry.movementMagnitudeLimit);
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

    public static EffectData DrawEffectData(EffectData entry) {
        EditorGUILayout.Separator();
        string placeholderName = string.IsNullOrEmpty(entry.effectName) == true ? "New Effect" : entry.effectName;
        EditorGUILayout.LabelField(placeholderName, EditorHelper2.LoadStyle(effectHeader));
        entry.type = EditorHelper.EnumPopup("Effect Type", entry.type);
        entry.effectName = EditorGUILayout.TextField("Effect Name", entry.effectName);
        entry.effectDescription = EditorGUILayout.TextField("Effect Description", entry.effectDescription);
        entry.effectDesignation = EditorHelper.EnumPopup("Effect Designation", entry.effectDesignation);
        entry.floatingTextColor = EditorGUILayout.GradientField("Floating Text Color", entry.floatingTextColor);
        entry.canOverload = EditorGUILayout.Toggle("Can Overload?", entry.canOverload);
        entry.canAffectDeadTargets = EditorGUILayout.Toggle("Can Affect Dead", entry.canAffectDeadTargets);
        
        if(entry.canOverload == true) {
            entry.overloadFloatingTextColor = EditorGUILayout.GradientField("Overload Floating Text Color", entry.overloadFloatingTextColor);

        }

        EditorGUILayout.Separator();

        entry.targeting = EditorHelper.EnumPopup("Targeting", entry.targeting);
        entry.subTarget = EditorHelper.EnumPopup("Sub Target", entry.subTarget);
        entry.maskTargeting = EditorHelper.EnumPopup("Mask Targeting", entry.maskTargeting);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Rider Effects", EditorHelper2.LoadStyle(effectHeader));
        entry.riderEffects = EditorHelper.DrawList("Riders", entry.riderEffects, null, DrawEffectDefinitionList);

        EditorGUILayout.Separator();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Target Constarints", EditorHelper2.LoadStyle(triggerHeader));
        entry.targetConstraints = EditorHelper.DrawExtendedList(entry.targetConstraints, "Constraint", DrawConstraintData);

        EditorGUILayout.Separator();

        if (entry.targeting != EffectTarget.PayloadDelivered) {
            entry.numberOfTargets = EditorGUILayout.IntField("Number of Targets", entry.numberOfTargets);
            entry.deliveryPayloadToTarget = EditorGUILayout.Toggle("Use Payload?", entry.deliveryPayloadToTarget);
            
        }
        else {
            EditorGUILayout.LabelField("Payload: ", EditorStyles.boldLabel);
            entry.projectileHitMask = EditorHelper.LayerMaskField("Hit Mask", entry.projectileHitMask);
            entry.payloadPrefab = EditorHelper.ObjectField("Payload Prefab", entry.payloadPrefab);
            entry.payloadCount = EditorGUILayout.IntField("Shot Count", entry.payloadCount);
            entry.shotDelay = EditorGUILayout.FloatField("Shot Delay", entry.shotDelay);
        }

        entry.spawnLocation = EditorHelper.EnumPopup("Spawn Location", entry.spawnLocation);

        if(entry.spawnLocation == DeliverySpawnLocation.ViewportPosition) {
            entry.minViewportValues = EditorGUILayout.Vector2Field("Min Values", entry.minViewportValues);
            entry.maxViewportValues = EditorGUILayout.Vector2Field("Max Values", entry.maxViewportValues);
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Effect Zone: ", EditorStyles.boldLabel);
        
        EditorGUI.indentLevel++;
        entry.effectZoneInfo = DrawEffectZoneInfo(entry.effectZoneInfo);
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Separator();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Stats: ", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        entry.payloadStatData = EditorHelper.DrawExtendedList(entry.payloadStatData, "Stat", DrawStatData);
        EditorGUI.indentLevel--;

        for (int i = 0; i < entry.payloadStatData.Count; i++) {
            if (AreStatsDuplicated(entry.payloadStatData[i], entry.payloadStatData) == true) {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(entry.payloadStatData[i].statName.ToString()) + " is duplicated in this data set.", EditorHelper2.LoadStyle(errorLabel));
            }
        }

        EditorGUILayout.Separator();

        switch (entry.type) {
            case EffectType.None:
                break;
            case EffectType.StatAdjustment:
                EditorGUILayout.LabelField("Mod Data: ", EditorStyles.boldLabel);
                
                EditorGUI.indentLevel++;
                entry.modData = EditorHelper.DrawExtendedList(entry.modData, "Mod", DrawStatModifierData);
                EditorGUI.indentLevel--;

                break;
            case EffectType.SpawnProjectile:
                EditorGUILayout.LabelField("Not Yet Implemented: ", errorLabel);
                break;
            case EffectType.AddStatus:
                EditorGUILayout.LabelField("Statuses: ", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                entry.statusToAdd = EditorHelper.DrawExtendedList(entry.statusToAdd, "Status", DrawStatusData);
                EditorGUI.indentLevel--;
                break;
            case EffectType.RemoveStatus:
                EditorGUILayout.LabelField("Not Yet Implemented: ", errorLabel);
                break;
            case EffectType.Movement:
                //EditorGUILayout.LabelField("Not Yet Implemented: ", errorLabel);
                entry.targetDestination = EditorHelper.EnumPopup("Move Direction", entry.targetDestination);
                entry.moveForce = EditorGUILayout.FloatField("Force", entry.moveForce);

                break;
            case EffectType.AddChildAbility:
                EditorGUILayout.LabelField("Abilities to Add: ", EditorStyles.boldLabel);
                entry.abilitiesToAdd = EditorHelper.DrawList("Child Abilities", entry.abilitiesToAdd, null, DrawAbilityDefinitionList);
                break;
            case EffectType.ApplyOtherEffect:
                EditorGUILayout.LabelField("Apply Other Effect: ", EditorStyles.boldLabel);
                entry.applyTriggeringEffect = EditorGUILayout.Toggle("Apply Triggering Effect", entry.applyTriggeringEffect);

                if(entry.applyTriggeringEffect == false) {
                    entry.targetOtherEffectName = EditorGUILayout.TextField("Target Effect", entry.targetOtherEffectName);
                    entry.targetOtherEffectParentAbilityName = EditorGUILayout.TextField("Target Parent Ability", entry.targetOtherEffectParentAbilityName);

                }
                break;
            case EffectType.AddStatScaler:
                EditorGUILayout.LabelField("Scalers to Add: ", EditorStyles.boldLabel);
                entry.statScalersToAdd = EditorHelper.DrawExtendedList(entry.statScalersToAdd, "Scaler", DrawStatScaler);
                break;

            case EffectType.SpawnEntity:
                entry.spawnType = EditorHelper.EnumPopup("Spawn Type", entry.spawnType);
                if(entry.spawnType == EntitySpawnType.Manual) {
                    entry.entityPrefab = EditorHelper.ObjectField("Prefab", entry.entityPrefab);
                }
                entry.percentOfPlayerDamage = EditorGUILayout.FloatField("Percent of Player Damage", entry.percentOfPlayerDamage);
                break;

            case EffectType.Teleport:
                entry.teleportDestination = EditorHelper.EnumPopup("Teleport Destination", entry.teleportDestination);
                entry.teleportVFX = EditorHelper.ObjectField("Teleport VFX", entry.teleportVFX);


                break;

            default:
                break;
        }


        return entry;
    }

    public static StatData DrawStatData(StatData entry) {
        if(entry == null) 
            entry = new StatData();
               

        entry.variant = EditorHelper.EnumPopup("Variant", entry.variant);
        entry.statName = EditorHelper.EnumPopup("Stat Name", entry.statName);
        
        entry.value = EditorGUILayout.FloatField("Value", entry.value);

        if (entry.variant == StatData.StatVariant.Range) {
            entry.minValue = EditorGUILayout.FloatField("Min Value: ", entry.minValue);
            entry.maxValue = EditorGUILayout.FloatField("Max Value: ", entry.maxValue);
            entry.value = EditorGUILayout.Slider("Starting Value", entry.value, entry.minValue, entry.maxValue);

            if (entry.minValue > entry.maxValue) {
                EditorGUILayout.LabelField("Min is higher than Max.", errorLabel);
            }

            if (entry.value > entry.maxValue) {
                EditorGUILayout.LabelField("Starting value is higher than Max.", errorLabel);
            }
        }

        return entry;
    }

    private static bool AreStatsDuplicated(StatData data, List<StatData> list) {
        for (int i = 0; i <list.Count; i++) {
            StatData currentData = list[i];

            if (currentData != data && currentData.statName == data.statName)
                return true;
        }

        return false;
    }

    public static StatModifierData DrawStatModifierData(StatModifierData entry) {
        EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(entry.targetStat.ToString()) + " Modifier", EditorStyles.boldLabel);
        entry.targetStat = EditorHelper.EnumPopup("Target Stat", entry.targetStat);
        entry.variantTarget = EditorHelper.EnumPopup("Variant", entry.variantTarget);
        entry.modifierType = EditorHelper.EnumPopup("Mod Type", entry.modifierType);

        EditorGUILayout.Separator();
        entry.modValueSetMethod = EditorHelper.EnumPopup("Set Method", entry.modValueSetMethod);

        switch (entry.modValueSetMethod) {
            case StatModifierData.ModValueSetMethod.Manual:
                entry.value = EditorGUILayout.FloatField("Value", entry.value);
                break;
            case StatModifierData.ModValueSetMethod.DeriveFromOtherStats:
                entry.deriveTarget = EditorHelper.EnumPopup("Derive Target", entry.deriveTarget);
                entry.derivedTargetStat = EditorHelper.EnumPopup("Target Stat", entry.derivedTargetStat);
                entry.deriveStatMultiplier = EditorGUILayout.FloatField("Multiplier", entry.deriveStatMultiplier);
                entry.invertDerivedValue = EditorGUILayout.Toggle("Invert?", entry.invertDerivedValue);

                if (entry.deriveTarget == StatModifierData.DeriveFromWhom.OtherEntityTarget) {
                    entry.otherTargetAbility = EditorGUILayout.TextField("Other Ability Name: ", entry.otherTargetAbility);
                    entry.otherTagetEffect = EditorGUILayout.TextField("Other Effect Name: ", entry.otherTagetEffect);
                }

                break;
            case StatModifierData.ModValueSetMethod.DeriveFromWeaponDamage:
                entry.weaponDamagePercent = EditorGUILayout.FloatField("Weapon Damage Percent", entry.weaponDamagePercent);
                break;
            case StatModifierData.ModValueSetMethod.DerivedFromMultipleSources:
                //entry.deriveTarget = EditorHelper.EnumPopup("Derive Target", entry.deriveTarget);
                //entry.derivedTargetStat = EditorHelper.EnumPopup("Target Stat", entry.derivedTargetStat);
                entry.invertDerivedValue = EditorGUILayout.Toggle("Invert?", entry.invertDerivedValue);
                
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Scalers: ", EditorStyles.boldLabel);
                entry.scalers = EditorHelper.DrawExtendedList(entry.scalers, "Scaler", DrawStatScaler);

                EditorGUILayout.Separator();
                break;

            case StatModifierData.ModValueSetMethod.DeriveFromNumberOfTargets:
            case StatModifierData.ModValueSetMethod.HardSetValue:
            case StatModifierData.ModValueSetMethod.HardReset:
                EditorGUILayout.LabelField("Not Yet Implemented", EditorHelper2.LoadStyle(errorLabel));
                break;

        }

        return entry;
    }

    public static StatScaler DrawStatScaler(StatScaler entry) {

        entry.targetStat = EditorHelper.EnumPopup("Target Stat", entry.targetStat);
        entry.deriveTarget = EditorHelper.EnumPopup("Derive Target", entry.deriveTarget);
        entry.statScaleBaseValue = EditorGUILayout.FloatField("Base Scale Value", entry.statScaleBaseValue);

        return entry;
    }

    public static EffectZoneInfo DrawEffectZoneInfo(EffectZoneInfo entry) {
        entry.removeEffectOnExit = EditorGUILayout.Toggle("Remove Effect on Exit", entry.removeEffectOnExit);
        entry.parentEffectToOrigin = EditorGUILayout.Toggle("Parent to Origin", entry.parentEffectToOrigin);
        entry.applyOncePerTarget = EditorGUILayout.Toggle("Apply Once per Target", entry.applyOncePerTarget);
        entry.applyOnInterval = EditorGUILayout.Toggle("Apply on Interval", entry.applyOnInterval);
        entry.affectSource = EditorGUILayout.Toggle("Affect Source?", entry.affectSource);

        EditorGUILayout.Separator();

        entry.spawnVFX = EditorHelper.ObjectField("Spawn VFX", entry.spawnVFX);
        entry.applyVFX = EditorHelper.ObjectField("Apply VFX", entry.applyVFX);
        entry.deathVFX = EditorHelper.ObjectField("Death VFX", entry.deathVFX);

        EditorGUILayout.Separator();

        entry.effectZonePrefab = EditorHelper.ObjectField("Effect Zone Prefab", entry.effectZonePrefab);

        return entry;
    }

    public static StatusData DrawStatusData(StatusData entry) {

        entry.statusName = EditorHelper.EnumPopup("Status Name", entry.statusName);
        entry.stackMethod = EditorHelper.EnumPopup("Stack Method", entry.stackMethod);
        entry.initialStackCount = EditorGUILayout.IntField("Initial Stacks", entry.initialStackCount);

        if(entry.stackMethod == Status.StackMethod.LimitedStacks) {
            entry.maxStacks = EditorGUILayout.IntField("Max Stacks", entry.maxStacks);
        }

        entry.statusEffectDef = EditorHelper.ObjectField("Status Effect", entry.statusEffectDef);

        entry.duration = EditorGUILayout.FloatField("Duration", entry.duration);
        entry.interval = EditorGUILayout.FloatField("Interval", entry.interval);

        entry.VFXPrefab = EditorHelper.ObjectField("VFX Prefab", entry.VFXPrefab);
        entry.vfxScaleModifier = EditorGUILayout.FloatField("VFX Scale", entry.vfxScaleModifier);


        return entry;
    }
}
