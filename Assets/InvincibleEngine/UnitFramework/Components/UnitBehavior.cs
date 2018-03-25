using UnityEngine;
using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine.UnitFramework.Components {
    public class UnitBehavior : EntityBehavior, IUnit {
        public UnitType UnitType { get; set; }
        public UnitTeam UnitTeam { get; set; }
        public bool Invulnerable { get; private set; }
        public bool Selected { get; set; } = false;

        public GameObject SelectionIndicator;

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