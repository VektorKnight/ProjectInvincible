using InvincibleEngine.UnitFramework.Components;
using InvincibleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBehavior : UnitBehavior {

    //Build cost in raw units
    public float BuildCost = 500;
    public float BuildResources = 0;

    //Initial build point, structures don't move
    public Vector3 Origin;

    //Sturcture properties
    float Height;

    //Static effects
    public ParticleSystem BuildingEffect;

    //Returns build progress between 0 and 1
    public float BuildingProgress {
        get { return Mathf.Clamp((float)BuildResources / (float)BuildCost, 0, 1); }
    }

    //Returns true if currently building
    public bool Building {
        get {
            if (BuildingProgress == 1) { return false; } else { return true; }
        }
    }

    public int EnergyGen = 0;
    public int ResourceGen = 0;

    public override void Start() {
        
        //Grab initial creation values
        Origin = transform.position;
        Height = GetComponent<BoxCollider>().size.y*2;

        

        base.Start();
    }

    //Behavior
    public override void OnEntityHostUpdate(float entityDelta) {
 
        //Structures, while being build, disable some of their functionality and play build animations
        if(Building) {

            //Start effects
            if (!BuildingEffect.isPlaying) {
                BuildingEffect.Play();
            }

            //Attempt to grab some resources and build as much as we can
            BuildResources++;

            //Show build progess visuals
            transform.position = new Vector3(Origin.x, Origin.y - (Height * (1 - BuildingProgress)), Origin.z);
        }

        //Normal behavior when no longer building
        else {
            if(BuildingEffect.isPlaying) {
                BuildingEffect.Stop();
            }
        }

        base.OnEntityHostUpdate(entityDelta);

    }
}
