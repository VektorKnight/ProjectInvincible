namespace VektorLibrary.Pathfinding.AStar {
    public enum AStarType {
        Standard,    // Textbook standard A*
        BestFirst,   // Extremely fast but not as accurate
        JumpPoint    // Grid-Optimized search algorithm
    }
}