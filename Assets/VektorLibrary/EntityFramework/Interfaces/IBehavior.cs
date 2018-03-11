namespace VektorLibrary.EntityFramework.Interfaces {
    public interface IBehavior {
        // Property: Initialized
        bool Initialized { get; }
        
        // Property: Terminating
        bool Terminating { get; }

        /// <summary>
        /// Called when a behavior is registered.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Called everytime Physics.Simulate() is called.
        /// </summary>
        /// <param name="physicsDelta"></param>
        void PhysicsUpdate(float physicsDelta);
        
        /// <summary>
        /// Called immediately after PhysicsUpdate.
        /// </summary>
        /// <param name="entityDelta"></param>
        void EntityUpdate(float entityDelta);
        
        /// <summary>
        /// Called once per frame.
        /// </summary>
        /// <param name="renderDelta"></param>
        void RenderUpdate(float renderDelta);
        
        /// <summary>
        /// Called when a behavior is unregistered.
        /// </summary>
        void Terminate();
    }
}