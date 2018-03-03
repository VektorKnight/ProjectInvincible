using System;
using System.Collections.Generic;
using InvincibleEngine.VektorLibrary.AI.Interfaces;

namespace InvincibleEngine.VektorLibrary.AI.Systems {
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
        private readonly Action<T> _defaultTask;

        // FSM Stack
        private readonly Stack<Action<T>> _taskStack = new Stack<Action<T>>();

        // Constructor
        public StackFSM(Action<T> defaultTask) {
            _defaultTask = defaultTask;
        }

        // Get the current task from the stack
        public Action<T> GetCurrentTask() {
            // Return the task at the top of the stack or return the default task if empty
            return _taskStack.Count > 0 ? _taskStack.Peek() : _defaultTask;
        }

        // Push a new task to the stack
        public void AddTask(Action<T> actiontask) {
            _taskStack.Push(actiontask);
        }

        // Pop a task from the stack
        public void RemoveTask() {
            _taskStack.Pop();
        }
        
        // Clear the entire stack
        public void ClearTasks() {
            _taskStack.Clear();
        }

        // Update the task machine
        public void Update(T param) {
            // Update the current task
            GetCurrentTask()?.Invoke(param);
        }
    }
}