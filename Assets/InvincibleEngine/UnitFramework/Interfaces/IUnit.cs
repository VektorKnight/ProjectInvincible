using InvincibleEngine;
namespace InvincibleEngine {
    public interface IUnit : ISelectable, ICommandable {
        UnitType UnitType { get; }
        UnitTeam UnitTeam { get; }
        bool Invulnerable { get; }
        
        void TakeDamage(float damage);
    }
}