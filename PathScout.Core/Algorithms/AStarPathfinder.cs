using PathScout.Core.Enums;
using PathScout.Core.Structure;
using System.Numerics;

namespace PathScout.Core.Algorithms
{
    /// <summary>
    /// Provides an implementation of the A* pathfinding algorithm for finding the shortest path between nodes in a
    /// graph.
    /// </summary>
    /// <remarks>AStarPathfinder exposes asynchronous pathfinding with step-by-step processing, allowing for
    /// visualization or custom logic to be executed after each algorithm step. The class tracks the running state and
    /// the number of steps taken during the search. It is thread-safe for single pathfinding operations but not
    /// designed for concurrent pathfinding requests on the same instance.</remarks>
    public class AStarPathfinder : IPathfinder
    {
        /// <summary>
        /// Gets a value indicating whether the process is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Gets the total number of steps taken.
        /// </summary>
        public int StepsTaken { get; private set; }

        /// <summary>
        /// Asynchronously finds the shortest path between two nodes using the A* algorithm.
        /// </summary>
        /// <remarks>The method pauses after each step by awaiting the provided callback, allowing for
        /// stepwise visualization or user interaction. The returned path includes both the start and end nodes if a
        /// path is found. The method does not modify the input collection except for updating node states and scores
        /// during the search.</remarks>
        /// <param name="start">The starting node for the path search.</param>
        /// <param name="end">The destination node for the path search.</param>
        /// <param name="onStepProcessed">A callback function invoked after each algorithm step, typically used for visualization or progress updates.
        /// The function is awaited before proceeding to the next step.</param>
        /// <returns>An enumerable collection of PathNode objects representing the shortest path from the start node to the end
        /// node. Returns an empty collection if no path is found or if either node is not present in the input.</returns>
        public async Task<IEnumerable<PathNode>> FindPathAsync(PathNode start, PathNode end, Func<Task> onStepProcessed, CancellationToken cancellationToken = default)
        {
            IsRunning = true;
            StepsTaken = 0;

            if (start == null || end == null)
            {
                IsRunning = false;
                return [];
            }

            var openQueue = new PriorityQueue<PathNode, float>();
            var openSet = new HashSet<PathNode>();
            var closedList = new HashSet<PathNode>();

            start.HScore = Vector2.DistanceSquared(start.Position, end.Position);
            start.GScore = 0;

            openQueue.Enqueue(start, start.FScore);
            openSet.Add(start);

            while (openQueue.Count > 0)
            {
                // 1. Find node with lowest FScore
                var current = openQueue.Dequeue();

                // Skip if already processed
                if (closedList.Contains(current)) continue;

                openSet.Remove(current);

                if (current == end)
                {
                    IsRunning = false;
                    return ReconstructPath(end);
                }

                closedList.Add(current);
                current.State = NodeState.Closed;

                // 2. Process Neighbors
                foreach (var (neighbor, edgeWeight) in current.GetConnections())
                {
                    if (closedList.Contains(neighbor) || neighbor.State == NodeState.Wall) continue;

                    float tentativeGScore = current.GScore + edgeWeight;

                    if (tentativeGScore < neighbor.GScore)
                    {
                        neighbor.Parent = current;
                        neighbor.GScore = tentativeGScore;

                        if (neighbor.HScore == float.MaxValue)
                            neighbor.HScore = Vector2.DistanceSquared(neighbor.Position, end.Position);

                        if (!openSet.Contains(neighbor))
                        {
                            neighbor.State = NodeState.Open;
                            openQueue.Enqueue(neighbor, neighbor.FScore);
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            // Re-enqueue with better priority
                            openQueue.Enqueue(neighbor, neighbor.FScore);
                        }
                    }
                }

                StepsTaken++;

                // 3. THE VISUALIZATION PAUSE
                // This waits for your timer or keypress before continuing the loop
                cancellationToken.ThrowIfCancellationRequested();
                await onStepProcessed();
            }

            IsRunning = false;
            return []; // No path found
        }

        /// <summary>
        /// Constructs the path from the start node to the specified end node by following parent references.
        /// </summary>
        /// <remarks>The returned path includes both the start and end nodes. If any node in the chain has
        /// a null parent before reaching the start node, the path will be incomplete.</remarks>
        /// <param name="endNode">The final node in the path. Must not be null and should have valid parent references tracing back to the
        /// start node.</param>
        /// <returns>A list of PathNode objects representing the reconstructed path from the start node to the specified end
        /// node, in order from start to end.</returns>
        private static List<PathNode> ReconstructPath(PathNode endNode)
        {
            var path = new List<PathNode>();
            var current = endNode;
            var visited = new HashSet<PathNode>(); // Prevent infinite loops

            while (current != null)
            {
                if (!visited.Add(current))
                    break; // Cycle detected

                current.State = NodeState.Path;
                path.Add(current);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }
    }
}