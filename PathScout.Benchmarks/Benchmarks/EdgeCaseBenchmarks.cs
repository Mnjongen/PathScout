using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using PathScout.Benchmarks.Helpers;

namespace PathScout.Benchmarks.Benchmarks
{
    /// <summary>
    /// Benchmarks for worst-case and edge-case scenarios
    /// Tests performance under challenging conditions
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class EdgeCaseBenchmarks
    {
        private Core.PathScout _noPathScenario = null!;
        private Core.PathScout _singleNodeScenario = null!;
        private Core.PathScout _longPathScenario = null!;
        
        private PathNode _noPathStart = null!;
        private PathNode _noPathEnd = null!;
        private PathNode _singleNode = null!;
        private PathNode _longPathStart = null!;
        private PathNode _longPathEnd = null!;

        [GlobalSetup]
        public void Setup()
        {
            // No path scenario - two disconnected components
            (_noPathScenario, _noPathStart, _noPathEnd) = GraphGenerator.CreateDisconnectedGraph(seed: 42);
            
            // Single node scenario
            _singleNodeScenario = new Core.PathScout(Core.Enums.AlgorithmType.AStar);
            _singleNode = _singleNodeScenario.AddNode(5, 5);
            
            // Long path scenario - linear chain forcing maximum exploration
            (_longPathScenario, _longPathStart, _longPathEnd) = GraphGenerator.CreateSparseGraph(200);
        }

        [Benchmark(Description = "No Path - Disconnected Components (explores all reachable)")]
        public async Task<int> NoPath_DisconnectedComponents()
        {
            var path = await _noPathScenario.FindPath(_noPathStart.NodeId, _noPathEnd.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "Single Node - Start equals End")]
        public async Task<int> SingleNode_StartEqualsEnd()
        {
            var path = await _singleNodeScenario.FindPath(_singleNode.NodeId, _singleNode.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "Long Path - 200 node linear chain")]
        public async Task<int> LongPath_200Nodes()
        {
            var path = await _longPathScenario.FindPath(_longPathStart.NodeId, _longPathEnd.NodeId);
            return path.Count();
        }
    }
}
