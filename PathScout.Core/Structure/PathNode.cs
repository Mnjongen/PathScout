using PathScout.Core.Enums;
using System.Numerics;

namespace PathScout.Core.Structure
{
    public class PathNode
    {
        /// <summary>
        /// Unique identifier for the node
        /// </summary>
        public int NodeId { get; set; }
        /// <summary>
        /// Gets the current traversal state of the node.
        /// </summary>
        public NodeState State { get; set; } = NodeState.Unvisited;
        /// <summary>
        /// Gets or sets the position of the object in 2D space.
        /// </summary>
        public Vector2 Position { get; set; }

        // A* specific properties
        /// <summary>
        /// Gets the estimated total cost (F-score) for reaching the goal from the start node via the current node, as
        /// used in the A* pathfinding algorithm.
        /// </summary>
        /// <remarks>The F-score is calculated as the sum of the actual cost from the start node to the
        /// current node (G-score) and the heuristic estimated cost from the current node to the goal (H-score). Lower
        /// F-scores indicate more promising paths during A* search.</remarks>
        public float FScore => GScore + HScore;
        /// <summary>
        /// Gets the cost from the start node to this node, as calculated by the pathfinding algorithm.
        /// </summary>
        public float GScore { get; set; } = float.MaxValue;
        /// <summary>
        /// Distance to the end goal
        /// </summary>
        public float HScore { get; set; } = float.MaxValue;
        /// <summary>
        /// Gets the parent node in the path, or null if this node hasn't been traversed yet.
        /// </summary>
        public PathNode? Parent { get; set; }

        /// <summary>
        /// Gets the number of active connections.
        /// </summary>
        public int ConnectionsCount => _connections.Count;

        private readonly Dictionary<PathNode, float> _connections;

        public PathNode(float x, float y, int id)
        {
            _connections = [];

            Position = new Vector2(x, y);
            NodeId = id;
        }

        /// <summary>
        /// Adds a connection to the specified node if it does not already exist.
        /// </summary>
        /// <param name="node">The node to connect to. Cannot be null.</param>
        /// <returns>true if the connection was added; otherwise, false if the connection already exists.</returns>
        public bool AddConnection(PathNode node)
        {
            if (node == null || node == this)
                return false;

            float edgeWeight = Vector2.DistanceSquared(Position, node.Position);

            return _connections.TryAdd(node, edgeWeight);
        }

        /// <summary>
        /// Removes the specified connection from this node.
        /// </summary>
        /// <param name="node">The node representing the connection to remove. Cannot be null.</param>
        /// <returns>true if the connection was successfully removed; otherwise, false.</returns>
        public bool RemoveConnection(PathNode node)
        {
            return _connections.Remove(node);
        }

        /// <summary>
        /// Removes the connection associated with the specified node identifier.
        /// </summary>
        /// <param name="nodeId">The unique identifier of the node whose connection is to be removed.</param>
        /// <returns>true if the connection was found and removed; otherwise, false.</returns>
        public bool RemoveConnection(int nodeId)
        {
            foreach (var kvp in _connections)
            {
                if (kvp.Key.NodeId == nodeId)
                    return _connections.Remove(kvp.Key);
            }
            return false;
        }

        /// <summary>
        /// Returns an enumerable collection of nodes that are directly connected to this node.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{PathNode}"/> containing all nodes connected to this node. The collection will be
        /// empty if there are no connections.</returns>
        public IEnumerable<(PathNode node, float weight)> GetConnections()
        {
            return _connections.Select(kv => (kv.Key, kv.Value));
        }

        public void Reset()
        {
            if (State != NodeState.Wall)
                State = NodeState.Unvisited;

            GScore = float.MaxValue;
            HScore = float.MaxValue;
            Parent = null;
        }

        public override bool Equals(object? obj) => obj is PathNode node && NodeId == node.NodeId;
        public override int GetHashCode() => NodeId.GetHashCode();
    }
}
