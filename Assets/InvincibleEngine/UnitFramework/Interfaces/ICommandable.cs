using InvincibleEngine;

namespace InvincibleEngine {
    public interface ICommandable {
        void IssueCommand(CommandType cmd, object arg, bool overrideQueue = false);
    }
}