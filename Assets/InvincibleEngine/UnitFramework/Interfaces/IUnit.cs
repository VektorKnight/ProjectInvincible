using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface IUnit : ISelectable, ICommandable {
        UnitType UnitType { get; }
        Team UnitTeam { get; }
        bool Invulnerable { get; }
        bool Dying { get; }

        void SetTeam(Team team);
        
        void ApplyDamage(float damage);

        void OnDeath();
    }
}