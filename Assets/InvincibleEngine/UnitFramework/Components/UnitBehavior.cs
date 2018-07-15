using UnityEngine;
using System.Collections.Generic;
using InvincibleEngine;
using System;

namespace InvincibleEngine {
    public class UnitBehavior : EntityBehavior, IUnit {
        
        public UnitType UnitType;
        public UnitTeam UnitTeam { get; set; }
        public bool Invulnerable { get; private set; }
        public bool Selected { get; set; } = false;

        //Indicates unit selection
        public GameObject SelectionIndicator;

        //Base health of a unit
        public float Health = 100;


        //Denotes whether this unit can be built from somwhere, eventually this should be changed 
        //to a proper flag system that tells excactly where this unit can be made from
        public bool CanBeProduced = false;

        UnitType IUnit.UnitType {
            get {
                throw new NotImplementedException();
            }
        }       
        public virtual void GenerateResource() {

        }

        public void Awake() {
            SelectionIndicator.GetComponent<Renderer>().enabled = false;
        }

        public virtual void IssueCommand(CommandType cmd, object arg, bool overrideQueue = false) {

        }

        public override void OnSelected() {
            SelectionIndicator.GetComponent<Renderer>().enabled = true;

        }

        public override void OnDeselected() {
            SelectionIndicator.GetComponent<Renderer>().enabled = false;

        }

        public virtual void TakeDamage(float damage) {

        }
        
    }
}