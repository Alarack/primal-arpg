using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum RecoveryType {
    Timed,
    
}


[Serializable]
public class RecoveryData 
{
    public RecoveryType type;
    //public int initialCharges = 1;
    //public int maxCharges = 1;

    public List<TriggerData> recoveryTriggers = new List<TriggerData>();
    public TriggerActivationCounterData counterData = new TriggerActivationCounterData();


    //Cooldown
    public float cooldown;

}
