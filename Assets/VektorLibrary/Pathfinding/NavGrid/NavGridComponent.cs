using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VektorLibrary.Pathfinding.NavGrid {
    /// <summary>
    /// Unity component for generating nav grids.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class NavGridComponent : MonoBehaviour {
        // Unity Inspector
        [Header("NavGrid Config")]
        [SerializeField] private NavGridConfig _gridConfig;
        [SerializeField] private NavGrid _navGrid;

        [Header("NavGrid Debugging")] 
        [SerializeField] private bool _renderGrid;
        [SerializeField] private Color _passableColor = Color.cyan;
        [SerializeField] private Color _impassableColor = Color.red;
        [SerializeField] private Material _gridMaterial;
        
        // Private: Required Components
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private async void Generate() {
            // Generate the NavGrid and save it
            _gridConfig.Origin = transform.position;
            _navGrid = ScriptableObject.CreateInstance<NavGrid>();
            _navGrid.Initialize(_gridConfig);

            // Reference the mesh filter and await mesh generation
            _meshFilter = GetComponent<MeshFilter>();     
            _meshFilter.mesh = await NavGridUtility.MeshFromGridAsync(_navGrid);
            
            // Set up the grid material and generate the texture
            if (_gridMaterial == null) _gridMaterial = Resources.Load<Material>("NavGridDebug");     
                _gridMaterial.mainTexture = NavGridUtility.TextureFromGrid(_navGrid, _passableColor, _impassableColor);
            
            // Reference the mesh renderer and assign the grid material
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = _gridMaterial;
        }

        public void Start() {
            // Generate the grid if necessary
            if (_navGrid == null) Generate();
        }
    }
}