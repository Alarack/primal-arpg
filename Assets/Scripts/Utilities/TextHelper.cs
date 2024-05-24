using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;


public static class TextHelper 
{


    public static string FormatStat(StatName stat, float value) {

        //StringBuilder builder = new StringBuilder();
        string bonusColor = ColorUtility.ToHtmlStringRGB(Color.green);
        string penaltyColor = ColorUtility.ToHtmlStringRGB(Color.red);

        string result = stat switch {
            StatName.Health => $"<color=#{bonusColor}>+" + value + "</color>",
            StatName.Essence => $"<color=#{bonusColor}>+" + value + "</color>",
            //StatName.Vitality => throw new System.NotImplementedException(),
            StatName.MoveSpeed when value < 0 => $"<color=#{penaltyColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.MoveSpeed when value > 0 => $"<color=#{bonusColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.RotationSpeed => throw new System.NotImplementedException(),
            //StatName.BaseDamage => throw new System.NotImplementedException(),
            //StatName.Stamina => throw new System.NotImplementedException(),
            //StatName.Mana => throw new System.NotImplementedException(),
            //StatName.Money => throw new System.NotImplementedException(),
            StatName.DetectionRange => throw new System.NotImplementedException(),
            StatName.Knockback => throw new System.NotImplementedException(),
            StatName.ProjectileLifetime => $"<color=#{bonusColor}>" + (value * 100) + "% </color>",
            StatName.EffectLifetime => $"<color=#{bonusColor}>" + (value * 100) + "% </color>",
            //StatName.EffectIntensity_Percent => throw new System.NotImplementedException(),
            StatName.Cooldown when value < 0 => $"<color=#{bonusColor}>" + (value * 100) + "% </color>",
            StatName.Cooldown when value >= 0 => $"<color=#{penaltyColor}>+" + (value * 100) + "% </color>",
            StatName.ShotCount => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.FireDelay => $"<color=#{bonusColor}>" + (value * 100) + "% </color>",
            StatName.Accuracy when value >= 0 => $"<color=#{bonusColor}>" + (value * 100) + "% </color>",
            StatName.Accuracy when value < 0 => $"<color=#{penaltyColor}>" + (value * 100) + "% </color>",
            StatName.DashSpeed => throw new System.NotImplementedException(),
            StatName.DashDuration => $"<color=#{bonusColor}>" + (value * 100) + "% </color>",
            StatName.EffectInterval when value < 0 => $"<color=#{bonusColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.EffectInterval when value > 0 => $"<color=#{penaltyColor}>+" + (value * 100) + "% </color>",
            StatName.AbilityCharge => "",
            //StatName.CooldownReduction when value > 0 => builder.Append("Cooldown Reduction").Append("-").Append( (value * 100) + "%").ToString(),
            StatName.CooldownReduction when value > 0 => $"<color=#{bonusColor}>" +(value * 100) + "% </color>",
            StatName.CooldownReduction when value <= 0 => "+" + (value * 100) + "%",
            StatName.GlobalDamageModifier when value > 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.GlobalDamageModifier when value < 0 => $"<color=#{penaltyColor}>-" + (value) * 100 + "% </color>",
            StatName.GlobalEffectDurationModifier when value > 0 => $"<color=#{bonusColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.GlobalEffectDurationModifier when value < 0 => $"<color=#{penaltyColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.MeleeDamageModifier when value > 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.MeleeDamageModifier when value < 0 => $"<color=#{penaltyColor}>-" + (value) * 100 + "% </color>",
            StatName.OverloadChance when value >= 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.OverloadChance when value < 0 => $"<color=#{penaltyColor}>-" + (value) * 100 + "% </color>",
            StatName.OverloadDamageModifier when value >= 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.OverloadDamageModifier when value < 0 => $"<color=#{penaltyColor}>-" + (value) * 100 + "% </color>",
            StatName.StatModifierValue => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.AbilityWeaponCoefficicent when value < 0 => $"<color=#{penaltyColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.AbilityWeaponCoefficicent when value > 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.ProjectilePierceCount when value > 0 => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.AbilityRuneSlots when value > 0 => $"<color=#{bonusColor}>" + value + "</color> more",
            StatName.AbilityRuneSlots when value < 0 => $"<color=#{penaltyColor}>" + value + "</color> less",
            StatName.GlobalEffectIntervalModifier when value < 0 => $"<color=#{bonusColor}>-" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.GlobalEffectIntervalModifier when value > 0 => $"<color=#{penaltyColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.GlobalEffectSizeModifier when value > 0 => $"<color=#{bonusColor}>+" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.GlobalEffectSizeModifier when value < 0 => $"<color=#{penaltyColor}>-" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.GlobalProjectileSizeModifier when value > 0 => $"<color=#{bonusColor}>+" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.GlobalProjectileSizeModifier when value < 0 => $"<color=#{penaltyColor}>-" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.EffectSize when value > 0 => $"<color=#{bonusColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.EffectSize when value < 0 => $"<color=#{penaltyColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.ProjectileChainCount => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.ProjectilePierceCount => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.ProjectileSplitCount => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.ProjectileSplitQuantity => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.ProjectileSize when value >= 0 => $"<color=#{bonusColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.ProjectileSize when value < 0 => $"<color=#{penaltyColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.ForceDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.WaterDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.MinionDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.FireDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.PoisonDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.AirDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.TimeDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.ArcaneDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.VoidDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.SpatialDamageModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.EssenceRegenerationRate when value > 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.EssenceRegenerationValue when value > 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.MaxMinionCount => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.CastSpeedModifier when value >= 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.CastSpeedModifier when value < 0 => $"<color=#{penaltyColor}>-" + (value) * 100 + "% </color>",
            StatName.DashCooldown => $"<color=#{bonusColor}>-" + (value) * 100 + "% </color>",
            StatName.GlobalMoveSpeedModifier => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.EssenceRegenerationRate when value < 0 => $"<color=#{penaltyColor}>" + (value) * 100 + "% </color>",
            StatName.EssenceShield when value > 0 => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.ProcChance when value > 0 => $"<color=#{bonusColor}>" + (value) * 100 + "%</color>",
            StatName.Armor when value > 0 => $"<color=#{bonusColor}>+" + (value) * 100 + "%</color>",
            StatName.Armor when value < 0 => $"<color=#{penaltyColor}>-" + (value) * 100 + "%</color>",
            StatName.VulnerableArcane => $"<color=#{bonusColor}>" + (value) * 100 + "%</color>",
            StatName.VulnerableSpace => $"<color=#{bonusColor}>" + (value) * 100 + "%</color>",
            StatName.VulnerableTime => $"<color=#{bonusColor}>" + (value) * 100 + "%</color>",
            StatName.VulnerableVoid => $"<color=#{bonusColor}>" + (value) * 100 + "%</color>",
            StatName.GlobalProjectileLifetimeModifier when value >= 0 => $"<color=#{bonusColor}>" + (value) * 100 + "%</color>",
            StatName.GlobalProjectileLifetimeModifier when value < 0 => $"<color=#{penaltyColor}>" + (value) * 100 + "%</color>",
            StatName.GlobalProjectileSpeedModifier when value >= 0 => $"<color=#{bonusColor}>" + (value) * 100 + "%</color>",
            StatName.GlobalProjectileSpeedModifier when value < 0 => $"<color=#{penaltyColor}>" + (value) * 100 + "%</color>",
            StatName.GlobalEssenceCostModifier when value >= 0 => $"<color=#{bonusColor}>" + (value) * 100 + "%</color>",
            StatName.GlobalEssenceCostModifier when value < 0 => $"<color=#{penaltyColor}>" + (value) * 100 + "%</color>",


            _ => "No Entry For: " + stat,
        };

        //string rarityText = $"<color=#{bonusColor}>{result}</color>";


        return "<b>" + result + "</b>";

    }

    public static string PretifyStatName(StatName stat) {
        string result = stat switch {
            StatName.Health => stat.ToString(),
            //StatName.Vitality => throw new System.NotImplementedException(),
            StatName.MoveSpeed => stat.ToString().SplitCamelCase(),
            StatName.RotationSpeed => stat.ToString().SplitCamelCase(),
            //StatName.BaseDamage => throw new System.NotImplementedException(),
            //StatName.Stamina => throw new System.NotImplementedException(),
            //StatName.Mana => throw new System.NotImplementedException(),
            //StatName.Money => throw new System.NotImplementedException(),
            StatName.DetectionRange => stat.ToString().SplitCamelCase(),
            StatName.Knockback => stat.ToString(),
            StatName.ProjectileLifetime => stat.ToString().SplitCamelCase(),
            StatName.EffectLifetime => stat.ToString().SplitCamelCase(),
            //StatName.EffectIntensity_Percent => throw new System.NotImplementedException(),
            StatName.Cooldown => stat.ToString(),
            StatName.ShotCount => stat.ToString().SplitCamelCase(),
            StatName.FireDelay => stat.ToString().SplitCamelCase(),
            //StatName.Accuracy => throw new System.NotImplementedException(),
            StatName.DashSpeed => stat.ToString().SplitCamelCase(),
            StatName.DashDuration => stat.ToString().SplitCamelCase(),
            StatName.EffectInterval => stat.ToString().SplitCamelCase(),
            StatName.AbilityCharge => "Charge",
            StatName.CooldownReduction => stat.ToString().SplitCamelCase(),
            StatName.GlobalDamageModifier => "Global Damage",
            StatName.GlobalEffectDurationModifier => "Global Effect Duration",
            StatName.MeleeDamageModifier => "Melee Damage",
            StatName.OverloadChance => stat.ToString().SplitCamelCase(),
            StatName.OverloadDamageModifier => "Overload Damage",
            //StatName.StatModifierValue => throw new System.NotImplementedException(),
            StatName.AbilityWeaponCoefficicent => "Weapon Damage",
            StatName.ProjectilePierceCount => "Pierce Count",
            StatName.AbilityRuneSlots => "Rune Slots",
            //StatName.StackCount => throw new System.NotImplementedException(),
            StatName.GlobalEffectIntervalModifier => "Global Effect Interval",
            StatName.DashCooldown => stat.ToString().SplitCamelCase(),
            StatName.ProjectileChainCount => "Chain Count",
            StatName.ProjectileSplitCount => "Split Count",
            StatName.ProjectileSplitQuantity => "Split Quantity",
            //StatName.ProjectileEffectContrabution => throw new System.NotImplementedException(),
            StatName.EffectMaxTargets => "Targets",
            StatName.GlobalEffectSizeModifier => "Global Effect Size",
            StatName.GlobalEffectRangeModifier => "Global Range",
            StatName.EffectSize => "Effect Size",
            StatName.EffectRange => "Range",
            StatName.GlobalProjectileSizeModifier => "Global Projectile Size",
            StatName.ProjectileSize => "Projectile Size",
            //StatName.StatScaler => throw new System.NotImplementedException(),
            StatName.ForceDamageModifier => "Force Damage",
            StatName.WaterDamageModifier => "Water Damage",
            StatName.FireDamageModifier => "Fire Damage",
            StatName.MinionDamageModifier => "Minion Damage",
            StatName.EssenceRegenerationRate => stat.ToString().SplitCamelCase(),
            StatName.EssenceRegenerationValue => "Essence Regeneration Amount",
            StatName.AirDamageModifier => "Air Damage",
            StatName.PoisonDamageModifier => "Poison Damage",
            StatName.MaxMinionCount => "Max Minions",
            StatName.CastSpeedModifier => "Cast Speed",
            StatName.Essence => stat.ToString(),
            StatName.GlobalMoveSpeedModifier => "Move Speed",
            _ => "Stat not found: " + stat,
        };

        return result;
    }

    public static string ColorizeText(string text, Color color, float size = 0f) {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);

        string colorizedText = $"<color=#{hexColor}><b>" + text + "</b></color>";

        if(size <= 0f) {
            return colorizedText;
        }
        else {
            return SizeText(colorizedText, size);
        }

        //return $"<color=#{hexColor}><b>" + text + "</b></color>";
    }

    public static string SizeText(string text, float size) {

        return $"<size=#{size}>" + text + "</size>";
    }


    public static string RoundTimeToPlaces(float time, int places) {
        float result = (float)System.Math.Round(time,places);

        return result.ToString();
    }


    public static string SplitCamelCase(this string str) {
        return Regex.Replace(
            Regex.Replace(
                str,
                @"(\P{Ll})(\P{Ll}\p{Ll})",
                "$1 $2"
            ),
            @"(\p{Ll})(\P{Ll})",
            "$1 $2"
        );
    }





}
