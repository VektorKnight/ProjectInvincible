using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBehavior : UnitBehavior {

    public int EnergyGen = 0;
    public int ResourceGen = 0;

    private void FixedUpdate() {
        MatchManager.Instance.OnGenerateResource(ResourceGen, EnergyGen);
    }
}
