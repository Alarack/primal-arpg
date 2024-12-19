using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Mastery Definition")]
public class MasteryDefiniiton : ScriptableObject
{

    public List<MasteryData> masteryData = new List<MasteryData>();


    public MasteryData GetMasteryDataByName(string name) {
        for (int i = 0; i < masteryData.Count; i++) {
            if (masteryData[i].masteryName == name) {
                return masteryData[i];
            }
        }
        return null;
    }


}
