using InvincibleEngine.UnitFramework.Enums;
using InvincibleEngine.WeaponSystem;

namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface IUnit : ISelectable, ICommandable , IDamageable {
        UnitType UnitType { get; }
        PlayerTeam UnitTeam { get; }
        bool Invulnerable { get; }
        bool Dying { get; }

        void SetTeam(PlayerTeam team);

        void OnDeath();
    }
}