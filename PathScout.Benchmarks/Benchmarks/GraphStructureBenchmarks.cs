using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using PathScout.Benchmarks.Helpers;

namespace PathScout.Benchmarks.Benchmarks
{
    /// <summary>
    /// Benchmarks for different graph structures
    /// Tests dense vs sparse graphs and their impact on pathfinding performance
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class GraphStructureBenchmarks
    {
        private Core.PathScout _denseGraph100 = null!;
        private Core.PathScout _denseGraph500 = null!;
        private Core.PathScout _denseGraph1000 = null!;
        private Core.PathScout _sparseGraph100 = null!;
        private Core.PathScout _sparseGraph500 = null!;
        private Core.PathScout _sparseGraph1000 = null!;
        
        private PathNode _startDense100 = null!;
        private PathNode _endDense100 = null!;
        private PathNode _startDense500 = null!;
        private PathNode _endDense500 = null!;
        private PathNode _startDense1000 = null!;
        private PathNode _endDense1000 = null!;
        private PathNode _startSparse100 = null!;
        private PathNode _endSparse100 = null!;
        private PathNode _startSparse500 = null!;
        private PathNode _endSparse500 = null!;
        private PathNode _startSparse1000 = null!;
        private PathNode _endSparse1000 = null!;

        [GlobalSetup]
        public void Setup()
        {
            // Dense graphs (average 6 connections per node)
            (_denseGraph100, _startDense100, _endDense100) = 
                GraphGenerator.CreateDenseGraph(100, connectionDensity: 6, seed: 42);
            (_denseGraph500, _startDense500, _endDense500) = 
                GraphGenerator.CreateDenseGraph(500, connectionDensity: 6, seed: 42);
            (_denseGraph1000, _startDense1000, _endDense1000) = 
                GraphGenerator.CreateDenseGraph(1000, connectionDensity: 6, seed: 42);
            
            // Sparse graphs (minimal connections)
            (_sparseGraph100, _startSparse100, _endSparse100) = 
                GraphGenerator.CreateSparseGraph(100, seed: 42);
            (_sparseGraph500, _startSparse500, _endSparse500) = 
                GraphGenerator.CreateSparseGraph(500, seed: 42);
            (_sparseGraph1000, _startSparse1000, _endSparse1000) = 
                GraphGenerator.CreateSparseGraph(1000, seed: 42);
        }

        [Benchmark(Description = "Dense Graph - 100 nodes, ~6 connections/node")]
        public async Task<int> DenseGraph_100Nodes()
        {
            var path = await _denseGraph100.FindPath(_startDense100.NodeId, _endDense100.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "Dense Graph - 500 nodes, ~6 connections/node")]
        public async Task<int> DenseGraph_500Nodes()
        {
            var path = await _denseGraph500.FindPath(_startDense500.NodeId, _endDense500.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "Dense Graph - 1000 nodes, ~6 connections/node")]
        public async Task<int> DenseGraph_1000Nodes()
        {
            var path = await _denseGraph1000.FindPath(_startDense1000.NodeId, _endDense1000.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "Sparse Graph - 100 nodes, ~1-2 connections/node")]
        public async Task<int> SparseGraph_100Nodes()
        {
            var path = await _sparseGraph100.FindPath(_startSparse100.NodeId, _endSparse100.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "Sparse Graph - 500 nodes, ~1-2 connections/node")]
        public async Task<int> SparseGraph_500Nodes()
        {
            var path = await _sparseGraph500.FindPath(_startSparse500.NodeId, _endSparse500.NodeId);
            return path.Count();
        }

        [Benchmark(Description = "Sparse Graph - 1000 nodes, ~1-2 connections/node")]
        public async Task<int> SparseGraph_1000Nodes()
        {
            var path = await _sparseGraph1000.FindPath(_startSparse1000.NodeId, _endSparse1000.NodeId);
            return path.Count();
        }
    }
}
