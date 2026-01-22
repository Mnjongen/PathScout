using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using PathScout.Benchmarks.Helpers;

namespace PathScout.Benchmarks.Benchmarks
{
    /// <summary>
    /// Benchmarks for maze/obstacle scenarios
    /// Tests pathfinding performance with walls and obstacles at various densities
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class MazePathfindingBenchmarks
    {
        private Core.PathScout _maze20x20_10percent = null!;
        private Core.PathScout _maze20x20_30percent = null!;
        private Core.PathScout _maze20x20_50percent = null!;
        private Core.PathScout _maze50x50_30percent = null!;
        
        private PathNode _start20x20_10 = null!;
        private PathNode _end20x20_10 = null!;
        private PathNode _start20x20_30 = null!;
        private PathNode _end20x20_30 = null!;
        private PathNode _start20x20_50 = null!;
        private PathNode _end20x20_50 = null!;
        private PathNode _start50x50_30 = null!;
        private PathNode _end50x50_30 = null!;

        [GlobalSetup]
        public void Setup()
        {
            // 20x20 maze with 10% walls
            (_maze20x20_10percent, _start20x20_10, _end20x20_10) = 
                GraphGenerator.CreateMaze(20, 20, 0.1f, seed: 42);
            
            // 20x20 maze with 30% walls
            (_maze20x20_30percent, _start20x20_30, _end20x20_30) = 
                GraphGenerator.CreateMaze(20, 20, 0.3f, seed: 42);
            
            // 20x20 maze with 50% walls
            (_maze20x20_50percent, _start20x20_50, _end20x20_50) = 
                GraphGenerator.CreateMaze(20, 20, 0.5f, seed: 42);
            
            // 50x50 maze with 30% walls
            (_maze50x50_30percent, _start50x50_30, _end50x50_30) = 
                GraphGenerator.CreateMaze(50, 50, 0.3f, seed: 42);
        }

        [Benchmark(Description = "20x20 Maze - 10% Walls")]
        public async Task<int> Maze_20x20_10Percent()
        {
            var path = await _maze20x20_10percent.FindPath(_start20x20_10.NodeId, _end20x20_10.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "20x20 Maze - 30% Walls")]
        public async Task<int> Maze_20x20_30Percent()
        {
            var path = await _maze20x20_30percent.FindPath(_start20x20_30.NodeId, _end20x20_30.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "20x20 Maze - 50% Walls")]
        public async Task<int> Maze_20x20_50Percent()
        {
            var path = await _maze20x20_50percent.FindPath(_start20x20_50.NodeId, _end20x20_50.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "50x50 Maze - 30% Walls")]
        public async Task<int> Maze_50x50_30Percent()
        {
            var path = await _maze50x50_30percent.FindPath(_start50x50_30.NodeId, _end50x50_30.NodeId);
            return path.Count();
        }
    }
}
