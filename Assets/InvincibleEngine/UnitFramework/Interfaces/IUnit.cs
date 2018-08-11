using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.WeaponSystem;

namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface IUnit : ISelectable, ICommandable , IDamageable {
        UnitType UnitType { get; }
        ETeam UnitTeam { get; }
        bool Invulnerable { get; }
        bool Dying { get; }

        void SetTeam(ETeam team);

        void OnDeath();
    }
}