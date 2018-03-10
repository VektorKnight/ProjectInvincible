using System.Collections.Generic;
using InvincibleEngine.Components.Generic;
using UnityEngine;
using VektorLibrary.Utility;

namespace InvincibleEngine.Managers {
	/// <summary>
	/// Manages the various object pools used in the game.
	/// </summary>
	public class GlobalObjectManager : MonoBehaviour {
		
		// Constants: Unique ID Generation
		public const int MAX_NUM_IDS = 512;
		
		// Singleton Instance
		private static GlobalObjectManager _singleton;
		public static GlobalObjectManager Instance => _singleton ?? new GameObject("GlobalObjectManager").AddComponent<GlobalObjectManager>();
		
		// Multi-Object Pool Instance
		public static MultiObjectPool MultiObjectPool { get; private set; }
		
		// Unity Inspector
		[Header("Multi-Object Pool Config")] 
		[SerializeField] private bool _dynamicAllocation = true;
		[SerializeField] private int _defaultPoolSize = 128;
		[SerializeField] private List<PoolObject> _presetPoolObjects = new List<PoolObject>();
		
		// Private: Unique ID Collection
		private readonly Stack<ulong> _uniqueIds = new Stack<ulong>();
		
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
			
			// Ensure manager is set to world origin (0,0,0) with zero rotation
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			
			// Generate a bunch of unique IDs
			Debug.Log($"<b><color=Teal>GlobalObjectManager:</color></b> Generating unique IDs, this may take a second...");
			for (var i = 0; i < MAX_NUM_IDS; i++) {
				var random = (ulong)Random.Range(ulong.MinValue, ulong.MaxValue);
				while (_uniqueIds.Contains(random) || random == 0) {
					random = (ulong)Random.Range(ulong.MinValue, ulong.MaxValue);
				}
				_uniqueIds.Push(random);
			}
			Debug.Log($"<b><color=Teal>GlobalObjectManager:</color></b> Generated {_uniqueIds.Count} unique IDs!");
			
			// Create and populate the multi-pool
			MultiObjectPool = new MultiObjectPool();
			foreach (var obj in _presetPoolObjects) {
				var pooledObjRef = obj.Object.GetComponent<PooledObject>();
				if (pooledObjRef == null) continue;
				MultiObjectPool.NewObjectPool(obj.Object, obj.PoolSize, transform);
				Debug.Log("<b><color=Teal>GlobalObjectManager:</color></b> Created new object pool!\n" +
				          $"<b>Object:</b> {obj.Object.name}, <b>Size:</b> {obj.PoolSize}");
			}
		}
		
		// Grabs a unique ID from the stack if available
		public static ulong GetUniqueId() {
			// Check if there are IDs available otherwise return zero
			return Instance._uniqueIds.Count == 0 ? 0 : Instance._uniqueIds.Pop();
		}
		
		// Wrapper for GetObject() from MultiPool class
		public static GameObject GetObject(GameObject obj, Vector3 position, Quaternion rotation) {
			// Fetch the object if a pool exists for it
			if (MultiObjectPool.ContainsPool(obj.name)) {
				return MultiObjectPool.GetObject(obj, position, rotation);
			}
			
			// Create a new pool for the given object if possible and fetch an object from it
			var pooledObjRef = obj.GetComponent<PooledObject>();
			if (Instance._dynamicAllocation && pooledObjRef != null) {
				MultiObjectPool.NewObjectPool(obj, Instance._defaultPoolSize, Instance.transform);
				
				Debug.Log("<b><color=Teal>GlobalObjectManager:</color></b> Created new object pool!\n" +
				          $"<b>Object:</b> {obj.name}, <b>Size:</b> {Instance._defaultPoolSize}");
				
				return MultiObjectPool.GetObject(obj, position, rotation);
			}
			
			// Log to console if unable to provide an object
			Debug.LogWarning($"<b><color=Teal>GlobalObjectManager:</color></b> Unable to fetch or create new pool for {obj.name}!\n" +
			                 $"The multi-pool is either full, the object does not support pooling, or an unexpected exception has occured.");
			return null;
		}
	}
}
