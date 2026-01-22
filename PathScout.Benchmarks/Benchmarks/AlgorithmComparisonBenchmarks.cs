using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using PathScout.Benchmarks.Helpers;

namespace PathScout.Benchmarks.Benchmarks
{
    /// <summary>
    /// Comprehensive benchmark comparing different algorithm implementations
    /// This will be useful when Dijkstra's algorithm is implemented
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class AlgorithmComparisonBenchmarks
    {
        private Core.PathScout _astar20x20 = null!;
        // Add Dijkstra when implemented:
        // private Core.PathScout _dijkstra20x20 = null!;
        
        private PathNode _start = null!;
        private PathNode _end = null!;

        [GlobalSetup]
        public void Setup()
        {
            // A* implementation
            (_astar20x20, _start, _end) = GraphGenerator.CreateGrid(20, 20);
            
            // TODO: Add Dijkstra when implemented
            // _dijkstra20x20 = new Core.PathScout(Core.Enums.AlgorithmType.Dijkstra);
            // Build same graph for fair comparison
        }

        [Benchmark(Baseline = true, Description = "A* - 20x20 Grid")]
        public async Task<int> AStar_20x20Grid()
        {
            var path = await _astar20x20.FindPath(_start.NodeId, _end.NodeId);
            return path.Count();
        }

        // TODO: Uncomment when Dijkstra is implemented
        // [Benchmark(Description = "Dijkstra - 20x20 Grid")]
        // public async Task<int> Dijkstra_20x20Grid()
        // {
        //     var path = await _dijkstra20x20.FindPath(_start.NodeId, _end.NodeId);
        //     return path.Count();
        // }
    }
}
