using InvincibleEngine.EntityFramework.Interfaces;
using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface IUnit : ISelectable, ICommandable {
        UnitType UnitType { get; }
        UnitTeam UnitTeam { get; }
        bool Invulnerable { get; }
        
        void TakeDamage(float damage);
    }
}