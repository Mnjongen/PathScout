using PathScout.Core.Structure;
using PathScout.Core.Enums;

namespace PathScout.Benchmarks.Helpers
{
    /// <summary>
    /// Helper class for generating test graphs for benchmarking
    /// </summary>
    public static class GraphGenerator
    {
        /// <summary>
        /// Creates a grid graph with specified dimensions
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="spacing">Spacing between nodes</param>
        /// <returns>PathScout instance with grid, start node, and end node</returns>
        public static (Core.PathScout pathScout, PathNode start, PathNode end) CreateGrid(
            int width, 
            int height, 
            float spacing = 10f)
        {
            var pathScout = new Core.PathScout(AlgorithmType.AStar);
            var grid = new PathNode[width, height];

            // Create nodes
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[x, y] = pathScout.AddNode(x * spacing, y * spacing);
                }
            }

            // Connect adjacent nodes (4-directional)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < width - 1)
                        pathScout.AddConnection(grid[x, y].NodeId, grid[x + 1, y].NodeId);
                    if (y < height - 1)
                        pathScout.AddConnection(grid[x, y].NodeId, grid[x, y + 1].NodeId);
                }
            }

            var start = grid[0, 0];
            var end = grid[width - 1, height - 1];

            return (pathScout, start, end);
        }

        /// <summary>
        /// Creates a grid with walls/obstacles (maze-like) that guarantees a path exists
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="wallDensity">Percentage of nodes that should be walls (0.0 to 1.0)</param>
        /// <param name="seed">Random seed for reproducibility</param>
        /// <returns>PathScout instance with maze, start node, and end node</returns>
        public static (Core.PathScout pathScout, PathNode start, PathNode end) CreateMaze(
            int width,
            int height,
            float wallDensity = 0.3f,
            int seed = 42)
        {
            var (pathScout, start, end) = CreateGrid(width, height);
            var random = new Random(seed);
            
            // First, create a guaranteed path from start to end using a simple path
            // This ensures the maze is always solvable
            var guaranteedPath = CreateGuaranteedPath(width, height, random);
            
            // Get all nodes and shuffle them
            var nodes = pathScout.GetAllNodes().ToList();
            var shuffledNodes = nodes.OrderBy(x => random.Next()).ToList();

            // Add walls to nodes that are NOT on the guaranteed path
            int targetWallCount = (int)(nodes.Count * wallDensity);
            int wallsAdded = 0;

            foreach (var node in shuffledNodes)
            {
                if (wallsAdded >= targetWallCount) break;
                if (node == start || node == end) continue;
                
                // Skip nodes on the guaranteed path to ensure connectivity
                if (IsOnGuaranteedPath(node, width, height, guaranteedPath)) continue;

                node.State = NodeState.Wall;
                wallsAdded++;
            }

            return (pathScout, start, end);
        }

        /// <summary>
        /// Creates a guaranteed path from (0,0) to (width-1, height-1)
        /// Returns a set of grid positions that form this path
        /// </summary>
        private static HashSet<(int x, int y)> CreateGuaranteedPath(int width, int height, Random random)
        {
            var path = new HashSet<(int x, int y)>();
            int x = 0, y = 0;
            
            // Add start position
            path.Add((x, y));
            
            // Move right and down randomly until we reach the end
            while (x < width - 1 || y < height - 1)
            {
                // Prefer moving right or down
                if (x < width - 1 && y < height - 1)
                {
                    // Can move either right or down, choose randomly
                    if (random.NextDouble() < 0.5)
                        x++;
                    else
                        y++;
                }
                else if (x < width - 1)
                {
                    // Can only move right
                    x++;
                }
                else
                {
                    // Can only move down
                    y++;
                }
                
                path.Add((x, y));
            }
            
            return path;
        }

        /// <summary>
        /// Checks if a node is on the guaranteed path
        /// </summary>
        private static bool IsOnGuaranteedPath(PathNode node, int width, int height, HashSet<(int x, int y)> path)
        {
            // Calculate grid position from node position (assuming 10f spacing)
            int x = (int)(node.Position.X / 10f);
            int y = (int)(node.Position.Y / 10f);
            
            return path.Contains((x, y));
        }

        /// <summary>
        /// Creates a dense graph where many nodes are connected to many neighbors
        /// Guarantees connectivity between start and end
        /// </summary>
        /// <param name="nodeCount">Number of nodes</param>
        /// <param name="connectionDensity">Average number of connections per node</param>
        /// <param name="seed">Random seed for reproducibility</param>
        /// <returns>PathScout instance with dense graph, start node, and end node</returns>
        public static (Core.PathScout pathScout, PathNode start, PathNode end) CreateDenseGraph(
            int nodeCount,
            int connectionDensity = 4,
            int seed = 42)
        {
            var pathScout = new Core.PathScout(AlgorithmType.AStar);
            var random = new Random(seed);
            var nodes = new List<PathNode>();

            // Create nodes in random positions
            for (int i = 0; i < nodeCount; i++)
            {
                float x = (float)(random.NextDouble() * 1000);
                float y = (float)(random.NextDouble() * 1000);
                nodes.Add(pathScout.AddNode(x, y));
            }

            // Connect each node to its N nearest neighbors
            // This naturally creates connectivity throughout the graph
            foreach (var node in nodes)
            {
                var nearestNeighbors = nodes
                    .Where(n => n != node)
                    .OrderBy(n => DistanceSquared(node.Position, n.Position))
                    .Take(connectionDensity);

                foreach (var neighbor in nearestNeighbors)
                {
                    pathScout.AddConnection(node.NodeId, neighbor.NodeId);
                }
            }

            var start = nodes.First();
            var end = nodes.Last();

            return (pathScout, start, end);
        }

        /// <summary>
        /// Creates a sparse graph with minimal connections
        /// Guarantees a linear path from start to end
        /// </summary>
        /// <param name="nodeCount">Number of nodes</param>
        /// <param name="seed">Random seed for reproducibility</param>
        /// <returns>PathScout instance with sparse graph, start node, and end node</returns>
        public static (Core.PathScout pathScout, PathNode start, PathNode end) CreateSparseGraph(
            int nodeCount,
            int seed = 42)
        {
            var pathScout = new Core.PathScout(AlgorithmType.AStar);
            var random = new Random(seed);
            var nodes = new List<PathNode>();

            // Create nodes in a line-like pattern
            for (int i = 0; i < nodeCount; i++)
            {
                float x = i * 10f;
                float y = (float)(random.NextDouble() * 100 - 50);
                nodes.Add(pathScout.AddNode(x, y));
            }

            // Connect each node to the next one (linear chain)
            // This guarantees connectivity from start to end
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                pathScout.AddConnection(nodes[i].NodeId, nodes[i + 1].NodeId);
                
                // Occasionally add a skip connection
                if (i < nodes.Count - 2 && random.NextDouble() < 0.2)
                {
                    pathScout.AddConnection(nodes[i].NodeId, nodes[i + 2].NodeId);
                }
            }

            var start = nodes.First();
            var end = nodes.Last();

            return (pathScout, start, end);
        }

        /// <summary>
        /// Creates a graph with disconnected components for testing no-path scenarios
        /// </summary>
        /// <param name="seed">Random seed for reproducibility</param>
        /// <returns>PathScout instance with disconnected graph, start node in component 1, end node in component 2</returns>
        public static (Core.PathScout pathScout, PathNode start, PathNode end) CreateDisconnectedGraph(int seed = 42)
        {
            var pathScout = new Core.PathScout(AlgorithmType.AStar);
            var random = new Random(seed);
            
            // Create first component (10 nodes)
            var component1 = new List<PathNode>();
            for (int i = 0; i < 10; i++)
            {
                float x = (float)(random.NextDouble() * 200);
                float y = (float)(random.NextDouble() * 200);
                component1.Add(pathScout.AddNode(x, y));
            }
            
            // Connect component 1 nodes
            for (int i = 0; i < component1.Count - 1; i++)
            {
                pathScout.AddConnection(component1[i].NodeId, component1[i + 1].NodeId);
            }
            
            // Create second component (10 nodes, far away)
            var component2 = new List<PathNode>();
            for (int i = 0; i < 10; i++)
            {
                float x = 800 + (float)(random.NextDouble() * 200);
                float y = 800 + (float)(random.NextDouble() * 200);
                component2.Add(pathScout.AddNode(x, y));
            }
            
            // Connect component 2 nodes
            for (int i = 0; i < component2.Count - 1; i++)
            {
                pathScout.AddConnection(component2[i].NodeId, component2[i + 1].NodeId);
            }
            
            // Return start from component 1 and end from component 2 (no path possible)
            var start = component1.First();
            var end = component2.Last();
            
            return (pathScout, start, end);
        }

        private static float DistanceSquared(System.Numerics.Vector2 a, System.Numerics.Vector2 b)
        {
            return System.Numerics.Vector2.DistanceSquared(a, b);
        }
    }
}
