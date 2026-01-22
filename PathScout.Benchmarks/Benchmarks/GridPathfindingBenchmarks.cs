using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using PathScout.Benchmarks.Helpers;

namespace PathScout.Benchmarks.Benchmarks
{
    /// <summary>
    /// Benchmarks for grid-based pathfinding scenarios
    /// Tests various grid sizes that are common in games and applications
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class GridPathfindingBenchmarks
    {
        private Core.PathScout _pathScout10x10 = null!;
        private Core.PathScout _pathScout20x20 = null!;
        private Core.PathScout _pathScout50x50 = null!;
        private Core.PathScout _pathScout100x100 = null!;
        
        private PathNode _start10x10 = null!;
        private PathNode _end10x10 = null!;
        private PathNode _start20x20 = null!;
        private PathNode _end20x20 = null!;
        private PathNode _start50x50 = null!;
        private PathNode _end50x50 = null!;
        private PathNode _start100x100 = null!;
        private PathNode _end100x100 = null!;

        [GlobalSetup]
        public void Setup()
        {
            // 10x10 grid (100 nodes)
            (_pathScout10x10, _start10x10, _end10x10) = GraphGenerator.CreateGrid(10, 10);
            
            // 20x20 grid (400 nodes)
            (_pathScout20x20, _start20x20, _end20x20) = GraphGenerator.CreateGrid(20, 20);
            
            // 50x50 grid (2,500 nodes)
            (_pathScout50x50, _start50x50, _end50x50) = GraphGenerator.CreateGrid(50, 50);
            
            // 100x100 grid (10,000 nodes)
            (_pathScout100x100, _start100x100, _end100x100) = GraphGenerator.CreateGrid(100, 100);
        }

        [Benchmark(Description = "10x10 Grid (100 nodes, corner to corner)")]
        public async Task<int> Grid_10x10()
        {
            var path = await _pathScout10x10.FindPath(_start10x10.NodeId, _end10x10.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "20x20 Grid (400 nodes, corner to corner)")]
        public async Task<int> Grid_20x20()
        {
            var path = await _pathScout20x20.FindPath(_start20x20.NodeId, _end20x20.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "50x50 Grid (2,500 nodes, corner to corner)")]
        public async Task<int> Grid_50x50()
        {
            var path = await _pathScout50x50.FindPath(_start50x50.NodeId, _end50x50.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "100x100 Grid (10,000 nodes, corner to corner)")]
        public async Task<int> Grid_100x100()
        {
            var path = await _pathScout100x100.FindPath(_start100x100.NodeId, _end100x100.NodeId);
            return path.Count();
        }
    }
}
