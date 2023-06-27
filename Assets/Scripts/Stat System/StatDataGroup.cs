using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat Data Group")]
public class StatDataGroup : ScriptableObject
{

    public List<StatData> dataList = new List<StatData>();

}
