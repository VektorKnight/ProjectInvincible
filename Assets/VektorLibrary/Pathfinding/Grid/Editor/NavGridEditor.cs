using UnityEditor;
using UnityEngine;

namespace VektorLibrary.Pathfinding.NavGrid.Editor {
    public class NavGridEditor : EditorWindow {
        [MenuItem("Window/NavGrid Editor")]
        public static void ShowWindow() {
            GetWindow(typeof(NavGridEditor));
        }
        
        private NavGridConfig _gridConfig;
        private NavGrid _navGrid;
    
        void OnGUI () {
            GUILayout.Label ("NavGrid Config", EditorStyles.boldLabel);
        }
    }
}