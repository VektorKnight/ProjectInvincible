using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface IUnit : ISelectable, ICommandable {
        UnitType UnitType { get; }
        ETeam UnitTeam { get; }
        bool Invulnerable { get; }
        bool Dying { get; }

        void SetTeam(ETeam team);
        
        void ApplyDamage(float damage);

        void OnDeath();
    }
}