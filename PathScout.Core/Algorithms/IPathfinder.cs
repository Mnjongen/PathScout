using PathScout.Core.Structure;

namespace PathScout.Core.Algorithms
{
    internal interface IPathfinder
    {
        public Task<IEnumerable<PathNode>> FindPathAsync(PathNode start, PathNode end, Func<Task> onStepProcessed, CancellationToken ct = default);
        
        // Status properties for the UI to read
        bool IsRunning { get; }
        int StepsTaken { get; }
    }
}
