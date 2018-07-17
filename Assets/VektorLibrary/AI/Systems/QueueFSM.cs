using System;
using System.Collections.Generic;
using VektorLibrary.AI.Interfaces;

namespace VektorLibrary.AI.Systems {
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
        public Action<T> GetCurrentState() {
            // Return the task at the front of the queue or return the default task if empty
            return _taskQueue.Count > 0 ? _taskQueue.Peek() : _defaultTask;
        }

        // Push a new task to the queue
        public void AddState(Action<T> task) {
            _taskQueue.Enqueue(task);
        }
        
        // Forcibly sets the state of the FSM
        public void SetState(Action<T> state) {
            Reset();
            _taskQueue.Enqueue(state);
        }

        // Remove the task at the front of the queue
        public void RemoveState() {
            _taskQueue.Dequeue();
        }
        
        // Clear the entire queue
        public void Reset() {
            _taskQueue.Clear();
        }

        // Update the state machine
        public void Update(T param) {
            // Update the current task
            GetCurrentState()?.Invoke(param);
        }
    }
}