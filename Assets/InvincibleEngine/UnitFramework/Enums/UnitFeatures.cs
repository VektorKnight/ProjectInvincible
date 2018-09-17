using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvincibleEngine.UnitFramework.Enums {
    [Flags] public enum UnitFeatures {
        None = 0,           // Utterly useless waste of resources
        Offensive = 1,      // Has offensive features (weaponry)
        Defensive = 2,      // Has defensive features (shields, anti-missile, etc)
        Intel = 4,          // Has intel features like (radar, line of sight)
        Construction = 8,   // Can build structures
        Factory = 16,       // Can build units
        Economy = 32        // Produces economic resources
    }
}
