using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.UnitFramework.DataTypes {
    public struct UnitCommand<T> {
        public UnitCommands Command;
        public T Data;
        
        public UnitCommand(UnitCommands command, T data) {
            Command = command;
            Data = data;
        }
    }
}