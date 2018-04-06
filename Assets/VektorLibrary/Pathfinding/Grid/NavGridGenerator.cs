using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using VektorLibrary.EntityFramework.Components;
using VektorLibrary.Pathfinding.AStar;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VektorLibrary.Pathfinding.Grid {
    /// <summary>
    /// Unity component for generating nav grids.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(LineRenderer))]
    public class NavGridGenerator : EntityBehavior {
        // Unity Inspector
        [Header("NavGrid Config")]
        [SerializeField] private NavGridConfig _gridConfig;
        private NavGrid _navGrid;

        [Header("NavGrid Debugging")] 
        [SerializeField] private bool _renderGrid;
        [SerializeField] private Color _passableColor = Color.cyan;
        [SerializeField] private Color _impassableColor = Color.red;
        [SerializeField] private Material _gridMaterial;

        [Header("NavGrid Testing: A*")] 
        [SerializeField] private Transform _pathStart, _pathEnd;
        private bool _waitingForPath;
        
        // Private: Required Components
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private LineRenderer _lineRenderer;
        
        #if UNITY_EDITOR
        public async void GenerateGrid() {
            // Set Json.Net default settings
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            
            // Generate the NavGrid and save it
            _gridConfig.Origin = transform.position;
            
            // Initialize a new NavGrid and save it to the resources folder
            var gridName = SceneManager.GetActiveScene().name + "_NavGrid.asset";
            var textureName = SceneManager.GetActiveScene().name + "_NavTexture.asset";
            var navGridAsset = ScriptableObject.CreateInstance<NavGridAsset>();
            navGridAsset.Initialize(_gridConfig);
            var navGrid = navGridAsset.Deserialize();
            
            // Reference the mesh filter and await mesh generation
            _meshFilter = GetComponent<MeshFilter>();
            _meshFilter.mesh = await NavGridUtility.MeshFromGridAsync(navGrid);
            
            // Set up the grid material and generate the texture
            if (_gridMaterial == null) _gridMaterial = Resources.Load<Material>("NavGridDebug");     
            var texture = NavGridUtility.TextureFromGrid(navGrid, _passableColor, _impassableColor);
            
            // Reference the mesh renderer and assign the grid material
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = _gridMaterial;
            
            EditorUtility.SetDirty(navGridAsset);
            AssetDatabase.CreateAsset(texture, $"Assets/GameAssets/Resources/{textureName}");
            _meshRenderer.sharedMaterial.mainTexture = texture;
            AssetDatabase.CreateAsset(navGridAsset, $"Assets/GameAssets/Resources/{gridName}");
            AssetDatabase.SaveAssets();
        }
        #endif

        public override void OnRegister () {            
            // Load the NavGrid for this scene
            _navGrid = NavGridUtility.GetSceneNavGrid();
            
            // Reference the line renderer component
            _lineRenderer = GetComponent<LineRenderer>();

            // Call the base method
            base.OnRegister();
        }

        public override void OnPhysicsUpdate(float physicsDelta) {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(mouseRay, out hit)) return;
            _pathEnd.position = hit.point;
        }

        public override void OnRenderUpdate(float renderDelta) {
            // Skip queueing up a new path job if we're waiting on a previous
            if (_waitingForPath || _navGrid == null) return;
            NavGridUtility.CalculatePath(_navGrid, new AStarRequest(_pathStart.position, _pathEnd.position, -1, OnPathFound));
            _waitingForPath = true;
            base.OnRenderUpdate(renderDelta);
        }
        
        public void OnPathFound(AStarResult result) {
            _waitingForPath = false;
            if (!result.Success) return;
            _lineRenderer.positionCount = result.Nodes.Length;
            _lineRenderer.SetPositions(result.Nodes);    
        }
    }
}