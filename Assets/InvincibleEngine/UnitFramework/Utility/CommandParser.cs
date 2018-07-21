using System;
using System.Collections.Generic;
using InvincibleEngine.UnitFramework.DataTypes;
using InvincibleEngine.UnitFramework.Enums;

namespace InvincibleEngine.UnitFramework.Utility {
    public class CommandParser {
        // Command - Delegate Dictionary
        private readonly Dictionary<UnitActions, Action<object>> _commandMapping;
        
        // Constructor
        public CommandParser() {
            _commandMapping = new Dictionary<UnitActions, Action<object>>();
        }
        
        // Bind a delegate to a particular command
        public void RegisterHandler(UnitActions command, Action<object> action) {
            _commandMapping.Add(command, action);
        }
        
        // Process a given command
        public void ProcessCommand(UnitCommand command) {
            // Exit if the command is not registered in the dictionary
            if (!_commandMapping.ContainsKey(command.Action)) return;
            
            // Invoke the registered delegate
            _commandMapping[command.Action].Invoke(command.Data);
        }
    }
}