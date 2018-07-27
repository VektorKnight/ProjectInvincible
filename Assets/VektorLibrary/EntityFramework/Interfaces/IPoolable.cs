namespace VektorLibrary.EntityFramework.Interfaces {
    public interface IPoolable {      
        /// <summary>
        /// Called when this object is retrieved from a pool.
        /// </summary>
        void OnRetrieved();
        
        /// <summary>
        /// Called when this object is returned to a pool.
        /// </summary>
        void OnReturned();
        
        /// <summary>
        /// Returns this object to it's pool.
        /// </summary>
        void ReturnToPool();
    }
}