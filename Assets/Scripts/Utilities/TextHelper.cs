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
            //StatName.CooldownReduction when value > 0 => builder.Append("Cooldown Reduction").Append("-").Append( (value * 100) + "%").ToString(),
            StatName.CooldownReduction when value > 0 => $"<color=#{bonusColor}>-" +(value * 100) + "% </color>",
            StatName.CooldownReduction when value <= 0 => "+" + (value * 100) + "%",
            StatName.GlobalDamageModifier => throw new System.NotImplementedException(),
            StatName.GlobalEffectDurationModifier => throw new System.NotImplementedException(),
            StatName.MeleeDamageModifier => throw new System.NotImplementedException(),
            StatName.OverloadChance when value >= 0 => "+" + (value * 100) + "%",
            StatName.OverloadChance when value < 0 => "-" + (value * 100) + "%",
            StatName.OverloadDamageModifier => throw new System.NotImplementedException(),
            _ => "",
        };

        string rarityText = $"<color=#{bonusColor}>{result}</color>";


        return result;

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
