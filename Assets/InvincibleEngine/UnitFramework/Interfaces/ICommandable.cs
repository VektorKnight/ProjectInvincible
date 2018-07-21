using InvincibleEngine.UnitFramework.DataTypes;

namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface ICommandable {
        void ProcessCommand(UnitCommand command);
    }
}