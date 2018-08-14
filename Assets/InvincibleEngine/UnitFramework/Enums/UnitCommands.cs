using System;

namespace InvincibleEngine.UnitFramework.Enums {
    [Flags] public enum UnitCommands {
        Move = 0,
        AMove = 1,
        Engage = 2,
        Assist = 4,
        Patrol = 8,
        Hold = 16,
        Stop = 32
    }
    
}