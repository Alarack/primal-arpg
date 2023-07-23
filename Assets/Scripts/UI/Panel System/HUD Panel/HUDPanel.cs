using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDPanel : BasePanel
{


    public ResourceGlobeDisplay healthGlobe;
    public ResourceGlobeDisplay essenceGlobe;


    protected override void Start() {
        base.Start();

      
    }

    public override void Open() {
        base.Open();

        healthGlobe.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.Health));
        essenceGlobe.Setup(EntityManager.ActivePlayer.Stats.GetStat<StatRange>(StatName.Essence));
    }

}
