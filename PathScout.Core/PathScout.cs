using PathScout.Core.Algorithms;
using PathScout.Core.Enums;
using PathScout.Core.Structure;

namespace PathScout.Core
{
    /// <summary>
    /// Provides functionality for managing a graph of path nodes and performing pathfinding operations using a
    /// specified algorithm.
    /// </summary>
    /// <remarks>The PathScout class allows you to add, remove, and connect nodes to form a graph structure,
    /// and to find shortest paths between nodes using algorithms such as Dijkstra's or A*. The class manages node
    /// identifiers and connections internally. Thread safety is not guaranteed; if used from multiple threads, external
    /// synchronization is required.</remarks>
    public class PathScout
    {

        private readonly IPathfinder _algorithm;
        private readonly Dictionary<int, PathNode> _nodeMap = new();

        private int _nextNodeId = 1;

        /// <summary>
        /// Gets the number of nodes currently contained in the collection.
        /// </summary>
        public int NodeCount => _nodeMap.Count;

        /// <summary>
        /// Initializes a new instance of the PathScout class using the specified pathfinding algorithm.
        /// </summary>
        /// <param name="algorithm">The algorithm type to use for pathfinding operations.</param>
        public PathScout(AlgorithmType algorithm)
        {
            _algorithm = CreateAlgorithm(algorithm);
        }

        /// <summary>
        /// Adds a new path node at the specified coordinates and returns the created node.
        /// </summary>
        /// <param name="x">The X-coordinate of the node to add.</param>
        /// <param name="y">The Y-coordinate of the node to add.</param>
        /// <returns>The newly created <see cref="PathNode"/> instance representing the node at the specified coordinates.</returns>
        public PathNode AddNode(float x, float y)
        {
            var newNode = new PathNode(x, y, _nextNodeId++);
            _nodeMap.Add(newNode.NodeId, newNode);
            return newNode;
        }

        /// <summary>
        /// Removes the node with the specified identifier from the graph, along with all connections to it.
        /// </summary>
        /// <remarks>When a node is removed, all connections from other nodes to this node are also
        /// removed. If the specified node does not exist in the graph, no action is taken and the method returns
        /// false.</remarks>
        /// <param name="nodeId">The unique identifier of the node to remove from the graph.</param>
        /// <returns>true if the node was found and removed; otherwise, false.</returns>
        public bool RemoveNode(int nodeId)
        {
            // Remove node from map and remove it from all other nodes' connections
            if (_nodeMap.Remove(nodeId, out var nodeToRemove))
            {
                foreach (var node in _nodeMap.Values)
                {
                    node.RemoveConnection(nodeToRemove);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the path node associated with the specified node identifier, if it exists.
        /// </summary>
        /// <param name="nodeId">The unique identifier of the node to retrieve.</param>
        /// <returns>The <see cref="PathNode"/> associated with the specified <paramref name="nodeId"/>, or <see
        /// langword="null"/> if no such node exists.</returns>
        public PathNode? GetNode(int nodeId)
        {
            return _nodeMap.GetValueOrDefault(nodeId);
        }

        /// <summary>
        /// Attempts to create a unidirectional connection from the node identified by <paramref name="nodeA"/> to the node 
        /// identified by <paramref name="nodeB"/>.
        /// </summary>
        /// <remarks>The method returns false if either node does not exist or if the connection could not
        /// be established.</remarks>
        /// <param name="nodeA">The identifier of the first node to connect.</param>
        /// <param name="nodeB">The identifier of the second node to connect.</param>
        /// <returns>true if the connection was successfully created; otherwise, false.</returns>
        public bool AddConnection(int nodeA, int nodeB, bool bidirectional = true)
        {
            if (!_nodeMap.TryGetValue(nodeA, out var firstNode) ||
                !_nodeMap.TryGetValue(nodeB, out var secondNode))
                return false;

            if (nodeA == nodeB)
                return false; // Prevent self-connections

            var result = firstNode.AddConnection(secondNode);
            if (bidirectional)
                result &= secondNode.AddConnection(firstNode);

            return result;
        }

        /// <summary>
        /// Removes the connection between the specified nodes, if it exists.
        /// </summary>
        /// <remarks>If either node does not exist or there is no connection between them, the method
        /// returns false.</remarks>
        /// <param name="nodeA">The identifier of the first node in the connection to remove.</param>
        /// <param name="nodeB">The identifier of the second node in the connection to remove.</param>
        /// <param name="bidirectional">If true, removes the connection in both directions; otherwise, removes only the connection from nodeA to nodeB.</param>
        /// <returns>true if the connection was successfully removed; otherwise, false.</returns>
        public bool RemoveConnection(int nodeA, int nodeB, bool bidirectional = true)
        {
            var firstNode = GetNode(nodeA);
            var secondNode = GetNode(nodeB);
            if (firstNode == null || secondNode == null)
                return false;

            var result = firstNode.RemoveConnection(secondNode);
            if (bidirectional)
                result &= secondNode.RemoveConnection(firstNode);

            return result;
        }

        /// <summary>
        /// Removes all nodes from the collection.
        /// </summary>
        /// <remarks>After calling this method, the collection will be empty. This operation cannot be
        /// undone.</remarks>
        public void ClearAllNodes()
        {
            _nodeMap.Clear();
        }

        /// <summary>
        /// Resets the state of all nodes managed by this instance to their initial values.
        /// </summary>
        /// <remarks>Call this method to reinitialize all nodes, typically before starting a new operation
        /// or to clear previous state. This method does not remove nodes; it only resets their internal
        /// state.</remarks>
        public void ResetNodeStates()
        {
            foreach (var node in _nodeMap.Values)
            {
                node.Reset();
            }
        }

        /// <summary>
        /// Returns a collection containing all nodes in the current path graph.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="PathNode"/> objects representing all nodes. The collection will be
        /// empty if the graph contains no nodes.</returns>
        public IReadOnlyCollection<PathNode> GetAllNodes()
        {
            return _nodeMap.Values;
        }

        /// <summary>
        /// Asynchronously finds the shortest path between two nodes in the graph.
        /// </summary>
        /// <param name="nodeA">The identifier of the starting node for the path search.</param>
        /// <param name="nodeB">The identifier of the destination node for the path search.</param>
        /// <param name="autoReset">If true, resets all node states before pathfinding; otherwise, uses current node states.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a sequence of <see
        /// cref="PathNode"/> objects representing the nodes in the shortest path from <paramref name="nodeA"/> to
        /// <paramref name="nodeB"/>. The sequence is empty if no path exists.</returns>
        public async Task<IEnumerable<PathNode>> FindPath(int nodeA, int nodeB, bool autoReset = true)
        {
            if(!_nodeMap.TryGetValue(nodeA, out var start) || !_nodeMap.TryGetValue(nodeB, out var end))
                return Enumerable.Empty<PathNode>();

            if (autoReset)
                ResetNodeStates();

            return await _algorithm.FindPathAsync(start, end, () => Task.CompletedTask);
        }

        /// <summary>
        /// Asynchronously finds the shortest path between two nodes with step-by-step callback for visualization.
        /// </summary>
        /// <param name="nodeA">The identifier of the starting node for the path search.</param>
        /// <param name="nodeB">The identifier of the destination node for the path search.</param>
        /// <param name="onStepProcessed">A callback function invoked after each algorithm step.</param>
        /// <param name="autoReset">If true, resets all node states before pathfinding; otherwise, uses current node states.</param>
        /// <param name="cancellationToken">Token to cancel the pathfinding operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a sequence of <see
        /// cref="PathNode"/> objects representing the nodes in the shortest path from <paramref name="nodeA"/> to
        /// <paramref name="nodeB"/>. The sequence is empty if no path exists.</returns>
        public async Task<IEnumerable<PathNode>> FindPath(int nodeA, int nodeB, Func<Task> onStepProcessed, bool autoReset = true, CancellationToken cancellationToken = default)
        {
            if(!_nodeMap.TryGetValue(nodeA, out var start) || !_nodeMap.TryGetValue(nodeB, out var end))
                return Enumerable.Empty<PathNode>();

            if (autoReset)
                ResetNodeStates();

            return await _algorithm.FindPathAsync(start, end, onStepProcessed, cancellationToken);
        }

        private IPathfinder CreateAlgorithm(AlgorithmType type)
        {
            return type switch
            {
                AlgorithmType.Dijkstras => new DijkstrasAlgorithm(this),
                AlgorithmType.AStar => new AStarPathfinder(),
                _ => throw new ArgumentException($"Unknown algorithm type: {type}")
            };
        }
    }
}
