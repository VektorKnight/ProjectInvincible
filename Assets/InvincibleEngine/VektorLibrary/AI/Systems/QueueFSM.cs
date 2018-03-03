using System;
using System.Collections.Generic;
using InvincibleEngine.VektorLibrary.AI.Interfaces;

namespace InvincibleEngine.VektorLibrary.AI.Systems {
    /// <summary>
    /// Alternative queue-based finite state machine class.
    /// Good for AIs that should finish their current task before moving on to the next.
    /// The generic value is designed to be used as a parameter to be passed to the active task.
    /// In most cases this value will probably be the deltatime value.
    /// Author: VektorKnight
    /// </summary>
    /// <typeparam name="T">The parameter passed to the active state.</typeparam>
    public class QueueFSM<T> : IStateMachine<T> {
        // Default state
        private readonly Action<T> _defaultTask;

        // FSM Queue
        private readonly Queue<Action<T>> _taskQueue = new Queue<Action<T>>();

        // Constructor
        public QueueFSM(Action<T> defaultTask) {
            _defaultTask = defaultTask;
        }

        // Get the current task from the queue
        public Action<T> GetCurrentTask() {
            // Return the task at the front of the queue or return the default task if empty
            return _taskQueue.Count > 0 ? _taskQueue.Peek() : _defaultTask;
        }

        // Push a new task to the queue
        public void AddTask(Action<T> task) {
            _taskQueue.Enqueue(task);
        }

        // Remove the task at the front of the queue
        public void RemoveTask() {
            _taskQueue.Dequeue();
        }
        
        // Clear the entire queue
        public void ClearTasks() {
            _taskQueue.Clear();
        }

        // Update the state machine
        public void Update(T param) {
            // Update the current task
            GetCurrentTask()?.Invoke(param);
        }
    }
}