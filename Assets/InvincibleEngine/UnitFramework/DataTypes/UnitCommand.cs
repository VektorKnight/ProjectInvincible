using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.UnitFramework.DataTypes {
    public struct UnitCommand {
        public UnitCommands Command;
        public object Data;
        
        public UnitCommand(UnitCommands command, object data) {
            Command = command;
            Data = data;
        }
    }
}