using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.UnitFramework.Interfaces {
    public interface ICommandable {
        void ProcessCommand(UnitCommand cmd, object arg, bool overrideQueue = false);
    }
}