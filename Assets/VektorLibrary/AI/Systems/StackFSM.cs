using System;
using System.Collections.Generic;
using VektorLibrary.AI.Interfaces;

namespace VektorLibrary.AI.Systems {
    /// <summary>
    /// Basic stack-based finite task machine class.
    /// Good for AIs that need to swap tasks immediately, overriding the current.
    /// The generic value is designed to be used as a parameter to be passed to the active task.
    /// In most cases this value will probably be the deltatime value.
    /// Author: VektorKnight
    /// </summary>
    /// <typeparam name="T">The parameter passed to the active task.</typeparam>
    public class StackFSM<T> : IStateMachine<T> {
        // Default task
        private readonly Action<T> _defaultState;

        // FSM Stack
        private readonly Stack<Action<T>> _stateStack = new Stack<Action<T>>();

        // Constructor
        public StackFSM(Action<T> defaultState) {
            _defaultState = defaultState;
        }

        // Get the current task from the stack
        public Action<T> GetCurrentState() {
            // Return the task at the top of the stack or return the default task if empty
            return _stateStack.Count > 0 ? _stateStack.Peek() : _defaultState;
        }

        // Push a new task to the stack
        public void AddState(Action<T> state) {
            _stateStack.Push(state);
        }
        
        // Forcibly sets the state of the FSM
        public void SetState(Action<T> state) {
            _stateStack.Clear();
            _stateStack.Push(state);
        }

        // Pop a task from the stack
        public void RemoveState() {
            _stateStack.Pop();
        }
        
        // Clear the entire stack
        public void Reset() {
            _stateStack.Clear();
        }

        // Update the task machine
        public void Update(T param) {
            // Update the current task
            GetCurrentState()?.Invoke(param);
        }
    }
}