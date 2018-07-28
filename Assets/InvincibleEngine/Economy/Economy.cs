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
    protected float _resources = 0;
    protected float _energy = 0;

    public float Resources {
        get { return 0; }
        private set { }
    }
    public float EnergySum {
        get { return 0; }
        private set { }
    }
    public float EnergyRatio {
        get { return 0; }
        private set { }
    }


    //Call to generate resouces on this frame
    public void OnGenerateResouces(float total) {

    }

    //Call to generate energy on this frame
    public void OnGenerateEnergy() {

    }

}
