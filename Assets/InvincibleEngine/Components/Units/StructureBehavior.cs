using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBehavior : UnitBehavior {

    //Between 0 and 1 
    private int _BuildProgress = 0;
    public int BuildingProgress {
        get { return _BuildProgress; }
        set { _BuildProgress = Mathf.Clamp(value, 0, 1); }
    }

    


    public int EnergyGen = 0;
    public int ResourceGen = 0;

    public override void OnEntityUpdate(float entityDelta) {
        base.OnEntityUpdate(entityDelta);
 

    }
}
