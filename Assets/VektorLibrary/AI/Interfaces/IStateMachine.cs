using System;

namespace VektorLibrary.AI.Interfaces {
    public interface IStateMachine<T> {
        Action<T> GetCurrentState();
        void AddState(Action<T> state);
        void SetState(Action<T> state);
        void RemoveState();  
        void Reset();
        void Update(T param);
    }
}