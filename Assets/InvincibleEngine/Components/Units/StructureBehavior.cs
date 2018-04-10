using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBehavior : UnitBehavior {

    public int EnergyGen = 0;
    public int ResourceGen = 0;

    public override void OnEntityUpdate(float entityDelta) {
        base.OnEntityUpdate(entityDelta);
  MatchManager.Instance.OnGenerateResource(ResourceGen, EnergyGen);
        if (MatchManager.Instance.MatchHost) {
          
        }
    }
}
