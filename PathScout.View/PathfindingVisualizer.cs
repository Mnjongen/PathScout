using PathScout.Core.Enums;
using PathScout.Core.Structure;
using System.Numerics;

namespace PathScout.View
{
    public enum InteractionMode
    {
        AddNode,
        RemoveNode,
        AddConnection,
        RemoveConnection,
        SetStart,
        SetEnd,
        SetWall,
        MoveNode
    }

    public enum VisualizationMode
    {
        Instant,      // No delay, instant solve
        Animated,     // 50ms delay between steps
        StepByStep    // Manual step-by-step
    }

    public class PathfindingVisualizer
    {
        private readonly PathScout.Core.PathScout _pathScout;
        private InteractionMode _mode = InteractionMode.AddNode;
        private VisualizationMode _visualizationMode = VisualizationMode.Animated;
        private PathNode? _startNode;
        private PathNode? _endNode;
        private PathNode? _selectedNode;
        private PathNode? _connectionStartNode;
        private PathNode? _draggedNode;
        
        private bool _isPathfinding = false;
        private bool _manualStep = false;
        
        private const float NODE_RADIUS = 15f;
        private const float NODE_CLICK_RADIUS = 20f;
        private const float CONNECTION_THICKNESS = 2f;

        public PathfindingVisualizer()
        {
            _pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
        }

        public InteractionMode CurrentMode
        {
            get => _mode;
            set => _mode = value;
        }

        public VisualizationMode CurrentVisualizationMode
        {
            get => _visualizationMode;
            set => _visualizationMode = value;
        }

        public PathNode? StartNode => _startNode;
        public PathNode? EndNode => _endNode;
        public bool IsPathfinding => _isPathfinding;
        public PathNode? DraggedNode => _draggedNode;

        public PathNode? GetNodeAtPosition(Vector2 position)
        {
            foreach (var node in _pathScout.GetAllNodes())
            {
                var distance = Vector2.Distance(node.Position, position);
                if (distance <= NODE_CLICK_RADIUS)
                {
                    return node;
                }
            }
            return null;
        }

        public void HandleClick(Vector2 position)
        {
            var clickedNode = GetNodeAtPosition(position);

            switch (_mode)
            {
                case InteractionMode.AddNode:
                    if (clickedNode == null)
                    {
                        _pathScout.AddNode(position.X, position.Y);
                    }
                    break;

                case InteractionMode.RemoveNode:
                    if (clickedNode != null)
                    {
                        if (clickedNode == _startNode) _startNode = null;
                        if (clickedNode == _endNode) _endNode = null;
                        _pathScout.RemoveNode(clickedNode.NodeId);
                    }
                    break;

                case InteractionMode.AddConnection:
                    if (clickedNode != null)
                    {
                        if (_connectionStartNode == null)
                        {
                            _connectionStartNode = clickedNode;
                        }
                        else
                        {
                            _pathScout.AddConnection(_connectionStartNode.NodeId, clickedNode.NodeId);
                            _connectionStartNode = null;
                        }
                    }
                    break;

                case InteractionMode.RemoveConnection:
                    if (clickedNode != null)
                    {
                        if (_connectionStartNode == null)
                        {
                            _connectionStartNode = clickedNode;
                        }
                        else
                        {
                            _pathScout.RemoveConnection(_connectionStartNode.NodeId, clickedNode.NodeId);
                            _connectionStartNode = null;
                        }
                    }
                    break;

                case InteractionMode.SetStart:
                    if (clickedNode != null)
                    {
                        _startNode = clickedNode;
                    }
                    break;

                case InteractionMode.SetEnd:
                    if (clickedNode != null)
                    {
                        _endNode = clickedNode;
                    }
                    break;

                case InteractionMode.SetWall:
                    if (clickedNode != null)
                    {
                        clickedNode.State = clickedNode.State == NodeState.Wall 
                            ? NodeState.Unvisited 
                            : NodeState.Wall;
                    }
                    break;

                case InteractionMode.MoveNode:
                    if (clickedNode != null)
                    {
                        _draggedNode = clickedNode;
                    }
                    break;
            }
        }

        public void HandleDrag(Vector2 position)
        {
            if (_draggedNode != null)
            {
                _draggedNode.Position = position;
            }
        }

        public void HandleRelease()
        {
            if (_draggedNode != null)
            {
                // Recalculate all connection weights for this node
                RecalculateNodeConnections(_draggedNode);
                
                // Reset pathfinding state since graph structure changed
                _pathScout.ResetNodeStates();
                
                _draggedNode = null;
            }
        }

        private void RecalculateNodeConnections(PathNode node)
        {
            // Get all connections and their neighbors
            var connections = node.GetConnections().ToList();
            
            // Remove all connections
            foreach (var (neighbor, _) in connections)
            {
                node.RemoveConnection(neighbor);
            }
            
            // Re-add connections with recalculated distances
            foreach (var (neighbor, _) in connections)
            {
                node.AddConnection(neighbor);
            }
            
            // Also update connections from neighbors that point to this node
            foreach (var otherNode in _pathScout.GetAllNodes())
            {
                if (otherNode == node) continue;
                
                var hasConnectionToNode = otherNode.GetConnections().Any(c => c.node == node);
                if (hasConnectionToNode)
                {
                    otherNode.RemoveConnection(node);
                    otherNode.AddConnection(node);
                }
            }
        }

        public void CancelCurrentAction()
        {
            _connectionStartNode = null;
            _draggedNode = null;
        }

        public void ClearGraph()
        {
            _pathScout.ClearAllNodes();
            _startNode = null;
            _endNode = null;
            _connectionStartNode = null;
        }

        public void ResetPathfinding()
        {
            _pathScout.ResetNodeStates();
            if (_startNode != null && _startNode.State == NodeState.Unvisited)
                _startNode.State = NodeState.Unvisited;
            if (_endNode != null && _endNode.State == NodeState.Unvisited)
                _endNode.State = NodeState.Unvisited;
        }

        public async Task RunPathfinding()
        {
            if (_startNode == null || _endNode == null || _isPathfinding)
                return;

            _isPathfinding = true;
            _pathScout.ResetNodeStates();

            await _pathScout.FindPath(_startNode.NodeId, _endNode.NodeId, autoReset: false);

            _isPathfinding = false;
        }

        public async Task RunPathfindingWithVisualization(Func<Task> renderCallback)
        {
            if (_startNode == null || _endNode == null || _isPathfinding)
                return;

            _isPathfinding = true;
            _pathScout.ResetNodeStates();

            await _pathScout.FindPath(_startNode.NodeId, _endNode.NodeId, 
                async () =>
                {
                    await renderCallback();
                    
                    switch (_visualizationMode)
                    {
                        case VisualizationMode.Instant:
                            // No delay - instant solve
                            break;
                            
                        case VisualizationMode.Animated:
                            // 50ms delay for animation
                            await Task.Delay(50);
                            break;
                            
                        case VisualizationMode.StepByStep:
                            // Wait for manual step
                            _manualStep = false;
                            while (!_manualStep && _isPathfinding)
                            {
                                await Task.Delay(16); // ~60 FPS
                            }
                            break;
                    }
                },
                autoReset: false);

            _isPathfinding = false;
        }

        public void TriggerManualStep()
        {
            _manualStep = true;
        }

        public IEnumerable<PathNode> GetAllNodes() => _pathScout.GetAllNodes();

        public PathNode? GetConnectionStartNode() => _connectionStartNode;

        public void CreateSampleGrid(int rows, int cols, float spacing, Vector2 offset)
        {
            ClearGraph();

            var grid = new PathNode[rows, cols];
            
            // Create nodes
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    var posX = offset.X + x * spacing;
                    var posY = offset.Y + y * spacing;
                    grid[x, y] = _pathScout.AddNode(posX, posY);
                }
            }

            // Connect adjacent nodes
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (x < cols - 1)
                        _pathScout.AddConnection(grid[x, y].NodeId, grid[x + 1, y].NodeId);
                    if (y < rows - 1)
                        _pathScout.AddConnection(grid[x, y].NodeId, grid[x, y + 1].NodeId);
                }
            }

            // Set start and end
            _startNode = grid[0, 0];
            _endNode = grid[cols - 1, rows - 1];
        }

        public void CreateRandomGraph(int nodeCount, int maxConnections, Vector2 areaMin, Vector2 areaMax)
        {
            ClearGraph();

            if (nodeCount <= 0) return;

            var random = new Random();
            var nodes = new List<PathNode>();

            // Create nodes at random positions
            for (int i = 0; i < nodeCount; i++)
            {
                float x = random.Next((int)areaMin.X, (int)areaMax.X);
                float y = random.Next((int)areaMin.Y, (int)areaMax.Y);
                nodes.Add(_pathScout.AddNode(x, y));
            }

            // Create random connections
            foreach (var node in nodes)
            {
                // Determine how many connections this node should have
                int connectionCount = random.Next(1, Math.Min(maxConnections + 1, nodes.Count));
                
                // Find closest nodes and connect to some of them
                var closestNodes = nodes
                    .Where(n => n != node)
                    .OrderBy(n => Vector2.DistanceSquared(node.Position, n.Position))
                    .Take(connectionCount * 2) // Take more candidates than needed
                    .OrderBy(_ => random.Next()) // Randomize selection
                    .Take(connectionCount)
                    .ToList();

                foreach (var neighbor in closestNodes)
                {
                    // Only add if not already connected
                    if (!node.GetConnections().Any(c => c.node == neighbor))
                    {
                        _pathScout.AddConnection(node.NodeId, neighbor.NodeId);
                    }
                }
            }

            // Set start and end to random nodes
            if (nodes.Count > 0)
            {
                _startNode = nodes[0];
                _endNode = nodes[nodes.Count - 1];
            }
        }

        public void GenerateMaze(float wallPercentage = 0.3f)
        {
            var allNodes = _pathScout.GetAllNodes().ToList();
            if (allNodes.Count == 0) return;

            var random = new Random();
            
            // Calculate how many walls to create
            int wallCount = (int)(allNodes.Count * wallPercentage);
            
            // Get nodes that aren't start or end
            var availableNodes = allNodes
                .Where(n => n != _startNode && n != _endNode)
                .ToList();

            if (availableNodes.Count == 0) return;

            // Randomly select nodes to become walls
            var wallNodes = availableNodes
                .OrderBy(_ => random.Next())
                .Take(wallCount)
                .ToList();

            // Set selected nodes as walls
            foreach (var node in wallNodes)
            {
                node.State = NodeState.Wall;
            }
        }
    }
}
