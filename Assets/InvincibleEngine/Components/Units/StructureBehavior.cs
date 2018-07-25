using InvincibleEngine;
using System.Collections;
using System.Collections.Generic;
using InvincibleEngine.UnitFramework.Components;
using UnityEngine;

public class StructureBehavior : UnitBehavior {

    //Structure Properties
    [Header("Structure Properties")]
    [SerializeField] private float BuildCost = 500;
    [SerializeField] private float BuildResources = 0;
    [SerializeField] private Vector3 Origin;
    [SerializeField] private float Height;

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
    public override void OnSimUpdate(float fixedDelta, bool isHost) {
 
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

        base.OnSimUpdate(fixedDelta, isHost);
    }
}
