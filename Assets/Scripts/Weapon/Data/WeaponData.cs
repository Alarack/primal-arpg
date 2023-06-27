using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    //Cooldown
    //Shot Count
    //Fire Delay
    //Accuracy = 1f

    public Sprite weaponIcon;

    public List<StatData> statData = new List<StatData>();

    public Projectile payload;

    public List<EffectData> effectData = new List<EffectData>();

    public KeyCode keyBinding;

    public string weaponName;
    public string weaponDescription;



}
