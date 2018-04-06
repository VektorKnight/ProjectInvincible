using UnityEditor;
using UnityEngine;

namespace VektorLibrary.Pathfinding.Grid.Editor {
    [CustomEditor(typeof(NavGridGenerator))]
    public class NavGridEditor : UnityEditor.Editor {    
        public override void OnInspectorGUI() {

            DrawDefaultInspector();
            var generator = (NavGridGenerator) target;
            
            if(GUILayout.Button("Generate")) {
                generator.GenerateGrid();
            }
        }
    }
}