using PathScout.Core.Structure;
using PathScout.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PathScout.Core.Algorithms
{
    public class DijkstrasAlgorithm : IPathfinder
    {
        private readonly PathScout _pathScout;

        public bool IsRunning { get; private set; }
        public int StepsTaken { get; private set; }

        public DijkstrasAlgorithm(PathScout pathScout)
        {
            _pathScout = pathScout;
        }

        public async Task<IEnumerable<PathNode>> FindPathAsync(PathNode start, PathNode end, Func<Task> onStepProcessed, CancellationToken cancellationToken = default)
        {
            IsRunning = true;
            StepsTaken = 0;

            if (start == null || end == null)
            {
                IsRunning = false;
                return Array.Empty<PathNode>();
            }

            // TODO: Implement Dijkstra's algorithm
            // Placeholder implementation
            IsRunning = false;
            return Array.Empty<PathNode>();
        }
    }
}
