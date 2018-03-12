using System.Collections.Generic;
using InvincibleEngine.Components.Generic;
using UnityEngine;
using VektorLibrary.Utility;

namespace InvincibleEngine.Managers {
	/// <summary>
	/// Manages instantiation and tracking of objects within the game. 
	/// Also handles object pooling for supported objects.
	/// </summary>
	public class ObjectManager : MonoBehaviour {		
		// Singleton Instance
		private static ObjectManager _singleton;
		public static ObjectManager Instance => _singleton ?? new GameObject("GlobalObjectManager").AddComponent<ObjectManager>();
		
		// Multi-Object Pool Instance
		public static MultiObjectPool MultiObjectPool { get; private set; }
		
		// Constants: Pool Config
		public const bool DYNAMIC_ALLOCATION = true;
		public const int DEFAULT_POOL_SIZE = 512;
		
		// Public Readonly: Pool Stats
		public static int UniquePoolCount => MultiObjectPool.MultiPool.Count;
		public static int ActiveObjectCount => MultiObjectPool.GetActiveCount();
		public static int TotalObjectCount => MultiObjectPool.GetTotalCount();

		// Initialization
		private void Start () {
			// Enforce Singleton Instance
			if (_singleton == null) { _singleton = this; }
			else if (_singleton != this) { Destroy(gameObject); }
			
			// Ensure this manager is not destroyed on scene load
			DontDestroyOnLoad(gameObject);
			
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
			var pooledObjRef = obj.GetComponent<PooledObject>();
			if (DYNAMIC_ALLOCATION && pooledObjRef != null) {
				MultiObjectPool.NewObjectPool(obj, DEFAULT_POOL_SIZE, Instance.transform);
				
				Debug.Log("<b><color=Teal>GlobalObjectManager:</color></b> Created new object pool!\n" +
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
