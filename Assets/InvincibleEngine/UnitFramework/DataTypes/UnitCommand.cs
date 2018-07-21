using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.UnitFramework.DataTypes {
    public struct UnitCommand {
        public UnitActions Action;
        public object Data;
        
        public UnitCommand(UnitActions action, object data) {
            Action = action;
            Data = data;
        }
    }
}