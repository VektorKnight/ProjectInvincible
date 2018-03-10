using System;

namespace VektorLibrary.AI.Interfaces {
    public interface IStateMachine<T> {
        Action<T> GetCurrentTask();
        void AddTask(Action<T> task);
        void RemoveTask();
        void ClearTasks();
        void Update(T param);
    }
}