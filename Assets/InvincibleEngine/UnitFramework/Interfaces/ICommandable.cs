using InvincibleEngine.UnitFramework.DataTypes;

namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface ICommandable {
        void ProcessCommand<T>(UnitCommand<T> command, bool overrideQueue = true);
    }
}