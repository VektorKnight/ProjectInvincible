using UnityEngine;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.Utility;

namespace VektorLibrary.EntityFramework.Singletons {
	/// <summary>
	/// Manages instantiation and tracking of objects within the game. 
	/// Also handles object pooling for supported objects.
	/// </summary>
	public class ObjectManager : MonoBehaviour {		
		// Singleton Instance
		public static ObjectManager Instance { get; private set; }

		// Multi-Object Pool Instance
		public static MultiObjectPool MultiObjectPool { get; private set; }
		
		// Constants: Pool Config
		public const int DEFAULT_POOL_SIZE = 1024;
		
		// Public Readonly: Pool Stats
		public static int UniquePoolCount => MultiObjectPool.MultiPool.Count;
		public static int ActiveObjectCount => MultiObjectPool.GetActiveCount();
		public static int TotalObjectCount => MultiObjectPool.GetTotalCount();
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Preload() {
			//Make sure the Managers object exists
			var managers = GameObject.Find("Managers") ?? new GameObject("Managers");

			// Ensure this singleton initializes at startup
			if (Instance == null) Instance = managers.GetComponent<ObjectManager>() ?? managers.AddComponent<ObjectManager>();

			// Ensure this singleton does not get destroyed on scene load
			DontDestroyOnLoad(Instance.gameObject);
            
			// Initialize the instance
			Instance.Initialize();
		}

		// Initialization
		private void Initialize () {		
			// Initialize the multi-pool
			MultiObjectPool = new MultiObjectPool();
		}
		
		// Wrapper for GetObject() from MultiPool class
		public static GameObject GetObject(GameObject obj, Vector3 position, Quaternion rotation) {
			// Fetch the object if a pool exists for it
			if (MultiObjectPool.ContainsPool(obj.name)) {
				return MultiObjectPool.GetObject(obj, position, rotation);
			}
			
			// Create a new pool for the given object if possible and fetch an object from it
			var pooledObjRef = obj.GetComponent<PooledBehavior>();
			if (pooledObjRef != null) {
				MultiObjectPool.NewObjectPool(obj, DEFAULT_POOL_SIZE, Instance.transform);
				
				DevConsole.Log("ObjectManager", "Created new object pool!\n" +
				               $"<b>Object:</b> {obj.name}, <b>Size:</b> {DEFAULT_POOL_SIZE}");
				
				return MultiObjectPool.GetObject(obj, position, rotation);
			}
			
			// Log to console if unable to provide an object
			Debug.LogWarning($"<b><color=Teal>GlobalObjectManager:</color></b> Unable to fetch or create new pool for {obj.name}!\n" +
			                 $"The multi-pool is either full, the object does not support pooling, or an unexpected exception has occured.");
			return null;
		}
	}
}
