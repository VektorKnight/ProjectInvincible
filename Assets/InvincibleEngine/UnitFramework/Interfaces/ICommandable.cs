using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.EntityFramework.Interfaces {
    public interface ICommandable {
        void IssueCommand(CommandType cmd, object arg, bool overrideQueue = false);
    }
}