namespace InvincibleEngine {
    public interface IEntity {
        // Property: Registered
        bool Registered { get; }
        
        // Property: Terminating
        bool Terminating { get; }

        /// <summary>
        /// Called immediately after this behavior is registered with the manager.
        /// </summary>
        void OnRegister();
        
        /// <summary>
        /// Called everytime Physics.Simulate() is called.
        /// </summary>
        /// <param name="physicsDelta"></param>
        void OnPhysicsUpdate(float physicsDelta);
        
        /// <summary>
        /// Called immediately after PhysicsUpdate.
        /// </summary>
        /// <param name="entityDelta"></param>
        void OnEntityHostUpdate(float entityDelta);
        
        /// <summary>
        /// Called once per frame.
        /// </summary>
        /// <param name="renderDelta"></param>
        void OnRenderUpdate(float renderDelta);
        
        /// <summary>
        /// Called when a behavior is unregistered.
        /// </summary>
        void Terminate();
    }
}