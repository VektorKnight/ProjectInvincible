using UnityEngine;
using System.Collections.Generic;
using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using VektorLibrary.EntityFramework.Components;
using System;

namespace InvincibleEngine.UnitFramework.Components {
    public class UnitBehavior : EntityBehavior, IUnit {
        public UnitType UnitType;
        public UnitTeam UnitTeam { get; set; }
        public bool Invulnerable { get; private set; }
        public bool Selected { get; set; } = false;

        UnitType IUnit.UnitType {
            get {
                throw new NotImplementedException();
            }
        }
        
        
        public GameObject SelectionIndicator;
        public float Health = 100;

        public virtual void GenerateResource() {

        }

        public void Awake() {
            SelectionIndicator.GetComponent<Renderer>().enabled = false;
        }

        public virtual void IssueCommand(CommandType cmd, object arg, bool overrideQueue = false) {

        }

        public virtual void OnSelected() {
            SelectionIndicator.GetComponent<Renderer>().enabled = true;

        }

        public virtual void OnDeselected() {
            SelectionIndicator.GetComponent<Renderer>().enabled = false;

        }

        public virtual void TakeDamage(float damage) {

        }
        
    }
}