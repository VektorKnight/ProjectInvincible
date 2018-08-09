using System;
using System.Collections.Generic;

namespace InvincibleEngine.DataTypes {
    public struct ConsoleCommand {
        public KeyValuePair<int, Type> Arguments;
        public Action<object[]> Delegate;
    }
}