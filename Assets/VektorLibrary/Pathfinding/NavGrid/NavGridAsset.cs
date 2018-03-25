using Newtonsoft.Json;
using UnityEngine;

namespace VektorLibrary.Pathfinding.NavGrid {
    public class NavGridAsset : ScriptableObject {
        [HideInInspector] [SerializeField]
        private string _navGridData;

        public void Initialize(NavGridConfig config) {
            _navGridData = JsonConvert.SerializeObject(config);
        }

        public NavGrid Deserialize() {
            var config = JsonConvert.DeserializeObject<NavGridConfig>(_navGridData);
            return new NavGrid(config);
        }
    }
}