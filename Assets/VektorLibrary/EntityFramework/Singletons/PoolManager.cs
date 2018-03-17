using UnityEngine;
using VektorLibrary.Utility;

namespace VektorLibrary.EntityFramework.Singletons {
    public class PoolManager : MonoBehaviour {
    // Singleton Instance
	    public static PoolManager Instance { get; private set; }

	    // Multi-Object Pool Instance
		public static MultiObjectPool MultiObjectPool { get; private set; }
		
		// Constants: Pool Config
		public const int DEFAULT_POOL_SIZE = 512;
		
		// Public Readonly: Pool Stats
		public static int UniquePoolCount => MultiObjectPool.MultiPool.Count;
		public static int ActiveObjectCount => MultiObjectPool.GetActiveCount();
		public static int TotalObjectCount => MultiObjectPool.GetTotalCount();
	    
	    // Preload Method
	    [RuntimeInitializeOnLoadMethod]
	    private static void Preload() {
		    // Ensure this singleton initializes at startup
		    if (Instance == null) Instance = new GameObject("PoolManager").AddComponent<PoolManager>();
            
		    // Ensure this singleton does not get destroyed on scene load
		    DontDestroyOnLoad(Instance.gameObject);
            
		    // Initialize the instance
		    Instance.Initialize();
	    }

		// Initialization
		private void Initialize () {
			// Enforce Singleton Instance
			if (Instance == null) { Instance = this; }
			else if (Instance != this) { Destroy(gameObject); }
			
			// Ensure this manager is not destroyed on scene load
			DontDestroyOnLoad(gameObject);
			
			// Initialize the multi-pool
			MultiObjectPool = new MultiObjectPool();
		}
		
		// Wrapper for GetObject() from MultiPool class
		public static GameObject GetObject(GameObject obj, Vector3 position, Quaternion rotation) {
			// Fetch the object if a pool exists for it
			if (MultiObjectPool.ContainsPool(obj.name)) 
				return MultiObjectPool.GetObject(obj, position, rotation);
			
			// Create a new pool for the given object if possible and fetch an object from it
			MultiObjectPool.NewObjectPool(obj, DEFAULT_POOL_SIZE, Instance.transform);
				
			Debug.Log("<b><color=Teal>GlobalPoolManager:</color></b> Created new object pool!\n" +
					 $"<b>Object:</b> {obj.name}, <b>Size:</b> {DEFAULT_POOL_SIZE}");
				
			return MultiObjectPool.GetObject(obj, position, rotation);
		}
	}
}