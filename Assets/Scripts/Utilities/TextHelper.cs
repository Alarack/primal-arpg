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
            StatName.Health => throw new System.NotImplementedException(),
            StatName.Vitality => throw new System.NotImplementedException(),
            StatName.MoveSpeed => throw new System.NotImplementedException(),
            StatName.RotationSpeed => throw new System.NotImplementedException(),
            StatName.BaseDamage => throw new System.NotImplementedException(),
            StatName.Stamina => throw new System.NotImplementedException(),
            StatName.Mana => throw new System.NotImplementedException(),
            StatName.Money => throw new System.NotImplementedException(),
            StatName.DetectionRange => throw new System.NotImplementedException(),
            StatName.Knockback => throw new System.NotImplementedException(),
            StatName.ProjectileLifetime => throw new System.NotImplementedException(),
            StatName.EffectLifetime => throw new System.NotImplementedException(),
            StatName.EffectIntensity_Percent => throw new System.NotImplementedException(),
            StatName.Cooldown when value < 0 => $"<color=#{bonusColor}>" + (value * 100) + "% </color>",
            StatName.Cooldown when value >= 0 => $"<color=#{penaltyColor}>+" + (value * 100) + "% </color>",
            StatName.ShotCount => $"<color=#{bonusColor}>" + value + "</color>",
            StatName.FireDelay => throw new System.NotImplementedException(),
            StatName.Accuracy => throw new System.NotImplementedException(),
            StatName.DashSpeed => throw new System.NotImplementedException(),
            StatName.DashDuration => throw new System.NotImplementedException(),
            StatName.EffectInterval when value < 0 => $"<color=#{bonusColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.EffectInterval when value > 0 => $"<color=#{penaltyColor}>+" + (value * 100) + "% </color>",
            StatName.AbilityCharge => throw new System.NotImplementedException(),
            //StatName.CooldownReduction when value > 0 => builder.Append("Cooldown Reduction").Append("-").Append( (value * 100) + "%").ToString(),
            StatName.CooldownReduction when value > 0 => $"<color=#{bonusColor}>-" +(value * 100) + "% </color>",
            StatName.CooldownReduction when value <= 0 => "+" + (value * 100) + "%",
            StatName.GlobalDamageModifier => throw new System.NotImplementedException(),
            StatName.GlobalEffectDurationModifier when value > 0 => $"<color=#{bonusColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.GlobalEffectDurationModifier when value < 0 => $"<color=#{penaltyColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.MeleeDamageModifier when value > 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.MeleeDamageModifier when value < 0 => $"<color=#{penaltyColor}>-" + (value) * 100 + "% </color>",
            StatName.OverloadChance when value >= 0 => $"<color=#{bonusColor}>" + (value) * 100 + "% </color>",
            StatName.OverloadChance when value < 0 => $"<color=#{penaltyColor}>-" + (value) * 100 + "% </color>",
            StatName.OverloadDamageModifier => throw new System.NotImplementedException(),
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
            StatName.ProjectileSize when value >= 0 => $"<color=#{bonusColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            StatName.ProjectileSize when value < 0 => $"<color=#{penaltyColor}>" + (Mathf.Abs(value) * 100) + "% </color>",
            _ => "No Entry For: " + stat,
        };

        //string rarityText = $"<color=#{bonusColor}>{result}</color>";


        return "<b>" + result + "</b>";

    }

    public static string PretifyStatName(StatName stat) {
        string result = stat switch {
            StatName.Health => throw new System.NotImplementedException(),
            StatName.Vitality => throw new System.NotImplementedException(),
            StatName.MoveSpeed => throw new System.NotImplementedException(),
            StatName.RotationSpeed => throw new System.NotImplementedException(),
            StatName.BaseDamage => throw new System.NotImplementedException(),
            StatName.Stamina => throw new System.NotImplementedException(),
            StatName.Mana => throw new System.NotImplementedException(),
            StatName.Money => throw new System.NotImplementedException(),
            StatName.DetectionRange => throw new System.NotImplementedException(),
            StatName.Knockback => throw new System.NotImplementedException(),
            StatName.ProjectileLifetime => throw new System.NotImplementedException(),
            StatName.EffectLifetime => throw new System.NotImplementedException(),
            StatName.EffectIntensity_Percent => throw new System.NotImplementedException(),
            StatName.Cooldown => throw new System.NotImplementedException(),
            StatName.ShotCount => throw new System.NotImplementedException(),
            StatName.FireDelay => throw new System.NotImplementedException(),
            StatName.Accuracy => throw new System.NotImplementedException(),
            StatName.DashSpeed => throw new System.NotImplementedException(),
            StatName.DashDuration => throw new System.NotImplementedException(),
            StatName.EffectInterval => throw new System.NotImplementedException(),
            StatName.AbilityCharge => throw new System.NotImplementedException(),
            StatName.CooldownReduction => throw new System.NotImplementedException(),
            StatName.GlobalDamageModifier => throw new System.NotImplementedException(),
            StatName.GlobalEffectDurationModifier => throw new System.NotImplementedException(),
            StatName.MeleeDamageModifier => throw new System.NotImplementedException(),
            StatName.OverloadChance => throw new System.NotImplementedException(),
            StatName.OverloadDamageModifier => throw new System.NotImplementedException(),
            StatName.StatModifierValue => throw new System.NotImplementedException(),
            StatName.AbilityWeaponCoefficicent => "Weapon Damage",
            StatName.ProjectilePierceCount => throw new System.NotImplementedException(),
            StatName.AbilityRuneSlots => throw new System.NotImplementedException(),
            StatName.StackCount => throw new System.NotImplementedException(),
            StatName.GlobalEffectIntervalModifier => throw new System.NotImplementedException(),
            StatName.DashCooldown => throw new System.NotImplementedException(),
            StatName.ProjectileChainCount => throw new System.NotImplementedException(),
            StatName.ProjectileSplitCount => throw new System.NotImplementedException(),
            StatName.ProjectileSplitQuantity => throw new System.NotImplementedException(),
            StatName.ProjectileEffectContrabution => throw new System.NotImplementedException(),
            StatName.EffectMaxTargets => throw new System.NotImplementedException(),
            StatName.GlobalEffectSizeModifier => throw new System.NotImplementedException(),
            StatName.GlobalEffectRangeModifier => throw new System.NotImplementedException(),
            StatName.EffectSize => "Effect Size",
            StatName.EffectRange => throw new System.NotImplementedException(),
            StatName.GlobalProjectileSizeModifier => throw new System.NotImplementedException(),
            StatName.ProjectileSize => "Projectile Size",
            StatName.StatScaler => throw new System.NotImplementedException(),
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
