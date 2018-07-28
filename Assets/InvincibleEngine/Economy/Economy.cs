using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Economy on a per player basis.
/// Resources are expended at time of use.
/// Energy is a ratio of how efficient everything will run based on (available energy/ desired energy)s
/// </summary>
public class Economy {

    //Economy 
    [SerializeField] protected float _resources = 0;
    [SerializeField] protected float _energyCreated = 0;
    [SerializeField] protected float _energyUsed = 0;
    [SerializeField] public float MinimumEfficiency = 0.2f;

    //Total resources in economy
    public float Resources {
        get { return _resources; }
        private set { _resources = value; }
    }

    //Total energy generated
    public float EnergySum {
        get { return 0; }
        private set { }
    }
    
    //Energy gained to used ratio
    public float EnergyRatio {
        get { return Mathf.Clamp(_energyCreated / _energyUsed, MinimumEfficiency, 1); }
    }


    //Call to generate resouces on this frame
    public void OnGenerateResouces(float total) {

    }

    //Call to generate energy on this frame
    public void OnGenerateEnergy(float total) {

    }

    //Call to use energy on this frame
    public void OnUseEnergy(float total) {

    }

    //Call to use resouces to do something
    public bool OnUseResources(float total) {

        //if we do not have enough resource
        if(_resources < total) {
            return false;
        }
        
        //if we have enough, use them and return true
        else {
            _resources -= total;
            return true;
        }

    }

}
