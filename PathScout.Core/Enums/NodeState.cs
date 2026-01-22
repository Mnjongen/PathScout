namespace PathScout.Core.Enums
{
    public enum NodeState
    {
        /// <summary>
        /// The "default". The algorithm hasn't even looked at this node yet.
        /// </summary>
        Unvisited,
        /// <summary>
        /// The algorithm has "discovered" the node and added to its to-do list.
        /// </summary>
        Open,
        /// <summary>
        /// The algorithm has finished evaluating this node and found the best way to get there.
        /// </summary>
        Closed,
        /// <summary>
        /// This node is part of the final path found by the algorithm.
        /// </summary>
        Path,
        /// <summary>
        /// This node is not traversable.
        /// </summary>
        Wall
    }
}
