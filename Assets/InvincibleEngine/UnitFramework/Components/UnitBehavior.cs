using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.UnitFramework.Interfaces;
using VektorLibrary.EntityFramework.Components;

namespace InvincibleEngine.UnitFramework.Components {
    public abstract class UnitBehavior : EntityBehavior, IUnit {
        public UnitType UnitType { get; set; }
        public UnitTeam UnitTeam { get; set; }
        public bool Invulnerable { get; private set; }

        public abstract void IssueCommand(CommandType cmd, object arg, bool overrideQueue = false);

        public abstract void OnSelected();

        public abstract void OnDeselected();
        
        public abstract void TakeDamage(float damage);
    }
}