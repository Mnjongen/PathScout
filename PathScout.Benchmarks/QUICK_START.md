# PathScout Benchmarks - Quick Reference

## What Was Created

### Project Structure
```
PathScout.Benchmarks/
??? PathScout.Benchmarks.csproj      # Project file with BenchmarkDotNet
??? Program.cs                        # Entry point for benchmarks
??? GlobalUsings.cs                   # Global using statements
??? README.md                         # Comprehensive documentation
?
??? Helpers/
?   ??? GraphGenerator.cs            # Graph generation utilities
?       ??? CreateGrid()             # Grid graphs (width × height)
?       ??? CreateMaze()             # Grids with obstacles
?       ??? CreateDenseGraph()       # Many connections per node
?       ??? CreateSparseGraph()      # Minimal connections
?
??? Benchmarks/
    ??? GridPathfindingBenchmarks.cs         # Grid sizes: 10×10, 20×20, 50×50, 100×100
    ??? MazePathfindingBenchmarks.cs         # Obstacle densities: 10%, 30%, 50%
    ??? GraphStructureBenchmarks.cs          # Dense vs Sparse (100-1000 nodes)
    ??? EdgeCaseBenchmarks.cs                # No-path, single-node, long-chain
    ??? AlgorithmComparisonBenchmarks.cs     # A* vs Dijkstra (ready for Dijkstra)
```

## Quick Commands

### Run All Benchmarks
```bash
cd PathScout.Benchmarks
dotnet run -c Release
```

### Run Specific Category
```bash
# Grid benchmarks
dotnet run -c Release --filter *GridPathfindingBenchmarks*

# Maze benchmarks
dotnet run -c Release --filter *MazePathfindingBenchmarks*

# Graph structure benchmarks
dotnet run -c Release --filter *GraphStructureBenchmarks*

# Edge case benchmarks
dotnet run -c Release --filter *EdgeCaseBenchmarks*

# Algorithm comparison
dotnet run -c Release --filter *AlgorithmComparisonBenchmarks*
```

### Run Single Benchmark
```bash
dotnet run -c Release --filter *Grid_20x20*
```

### Export Results
```bash
# HTML report
dotnet run -c Release --exporters html

# Markdown report
dotnet run -c Release --exporters markdown

# CSV for Excel
dotnet run -c Release --exporters csv

# All formats
dotnet run -c Release --exporters html markdown csv json
```

## Benchmark Categories Explained

### 1. GridPathfindingBenchmarks
- **Purpose**: Validates README claims about grid performance
- **Scenarios**: 
  - 10×10 (100 nodes)
  - 20×20 (400 nodes) ? README claim verification
  - 50×50 (2,500 nodes)
  - 100×100 (10,000 nodes)

### 2. MazePathfindingBenchmarks
- **Purpose**: Tests pathfinding with obstacles
- **Scenarios**:
  - 20×20 with 10% walls (light obstacles)
  - 20×20 with 30% walls (moderate obstacles)
  - 20×20 with 50% walls (heavy obstacles)
  - 50×50 with 30% walls (large maze)

### 3. GraphStructureBenchmarks
- **Purpose**: Compare dense vs sparse graph performance
- **Dense**: ~6 connections per node (realistic game scenarios)
- **Sparse**: ~1-2 connections per node (linear paths)
- **Sizes**: 100, 500, 1000 nodes

### 4. EdgeCaseBenchmarks
- **Purpose**: Worst-case scenario testing
- **Scenarios**:
  - No path available (disconnected graph)
  - Single node (start = end)
  - Long path (200-node chain)

### 5. AlgorithmComparisonBenchmarks
- **Purpose**: Side-by-side algorithm comparison
- **Current**: A* baseline
- **Future**: Add Dijkstra when implemented

## Expected Results (Validation Goals)

### Actual Benchmark Results

#### GridPathfindingBenchmarks ? Completed
| Scenario | Grid Size | Nodes | Performance | Status |
|----------|-----------|-------|-------------|---------|
| 10×10 Grid | 100 nodes | 180 connections | **4.590 ?s** | ? Excellent |
| 20×20 Grid | 400 nodes | 760 connections | **9.879 ?s** | ? Excellent |
| 50×50 Grid | 2,500 nodes | 4,900 connections | **27.922 ?s** | ? Excellent |
| 100×100 Grid | 10,000 nodes | 19,800 connections | **74.779 ?s** | ? Excellent |

**Key Finding**: Sub-linear scaling! ~2.5x slower per 4-6x increase in nodes

#### MazePathfindingBenchmarks ? Completed
| Scenario | Grid Size | Wall Density | Performance | Status |
|----------|-----------|--------------|-------------|---------|
| 20×20 Maze | 400 nodes | 10% walls | **10.2 ?s** | ? Excellent |
| 20×20 Maze | 400 nodes | 30% walls | **10.0 ?s** | ? Excellent |
| 20×20 Maze | 400 nodes | 50% walls | **9.6 ?s** | ? Excellent |
| 50×50 Maze | 2,500 nodes | 30% walls | **31.5 ?s** | ? Excellent |

**Key Finding**: Higher wall density = faster performance (fewer nodes to explore)

#### GraphStructureBenchmarks ? Completed
| Scenario | Nodes | Connectivity | Performance | Status |
|----------|-------|--------------|-------------|---------|
| Dense Graph | 100 | ~6 connections/node | **3.129 ?s** | ? Fastest |
| Dense Graph | 500 | ~6 connections/node | **17.983 ?s** | ? Fast |
| Dense Graph | 1,000 | ~6 connections/node | **6.190 ?s** | ? Very Fast |
| Sparse Graph | 100 | ~1-2 connections/node | **13.334 ?s** | ?? Slower |
| Sparse Graph | 500 | ~1-2 connections/node | **70.265 ?s** | ?? Much Slower |
| Sparse Graph | 1,000 | ~1-2 connections/node | **150.761 ?s** | ?? Slowest |

**Key Finding**: Graph connectivity matters MORE than node count! Dense graphs are 4-24x faster than sparse graphs.

#### EdgeCaseBenchmarks ? Completed
| Scenario | Description | Performance | Memory | Status |
|----------|-------------|-------------|--------|---------|
| Single Node | Start = End | **117.9 ns** | 784 B | ? Instant |
| No Path | Disconnected components | **1.384 ?s** | 2.42 KB | ? Very Fast |
| Long Path | 200-node linear chain | **26.175 ?s** | 43.03 KB | ? Fast |

**Key Finding**: Edge cases are handled extremely efficiently. Single-node check is 84x faster than normal pathfinding!

#### AlgorithmComparisonBenchmarks ? Baseline Established
| Algorithm | Grid Size | Performance | Memory | Status |
|-----------|-----------|-------------|--------|---------|
| A* | 20×20 (400 nodes) | **10.38 ?s** | 17.85 KB | ? Baseline (1.00x) |
| Dijkstra | 20×20 (400 nodes) | *Not implemented* | - | ? Pending |

**Key Finding**: A* baseline established at 10.38 ?s for 20×20 grid. Ready for Dijkstra comparison when implemented.

---

**All Available Benchmarks Complete!** ??

Send Dijkstra implementation results when ready for algorithm comparison!

## Sample Output

```
| Method                    | Mean      | Rank | Gen0   | Allocated |
|-------------------------- |----------:|-----:|-------:|----------:|
| Grid_10x10               | 15.23 ?s  |    1 | 0.9155 |    5.6 KB |
| Grid_20x20               | 62.47 ?s  |    2 | 3.6621 |   22.4 KB |
| Maze_20x20_30Percent     | 45.12 ?s  |    3 | 2.7466 |   16.8 KB |
| DenseGraph_100Nodes      | 28.34 ?s  |    4 | 1.8311 |   11.2 KB |
```

**Interpretation**:
- **Mean**: Average execution time (lower is better)
- **Rank**: Performance ranking (1 = fastest)
- **Gen0**: Garbage collections (lower is better)
- **Allocated**: Memory used (lower is better)

## Next Steps

### 1. Run Initial Benchmarks
```bash
cd PathScout.Benchmarks
dotnet run -c Release
```

### 2. Review Results
- Check if performance meets README claims
- Identify bottlenecks
- Look for unexpected slow scenarios

### 3. Update README with Real Data
Replace estimates with actual benchmark results:
```markdown
## Performance Metrics

From benchmarks (your machine):
- **10×10 Grid**: ~15 ?s
- **20×20 Grid**: ~62 ?s  
- **50×50 Grid**: ~412 ?s
- **100×100 Grid**: ~1.8 ms
```

### 4. When Implementing Dijkstra

Uncomment in `AlgorithmComparisonBenchmarks.cs`:
```csharp
[GlobalSetup]
public void Setup()
{
    (_astar20x20, _start, _end) = GraphGenerator.CreateGrid(20, 20);
    
    // Add Dijkstra comparison
    _dijkstra20x20 = new Core.PathScout(AlgorithmType.Dijkstra);
    var grid = new PathNode[20, 20];
    // ... build same graph ...
}

[Benchmark(Description = "Dijkstra - 20x20 Grid")]
public async Task<IEnumerable<PathNode>> Dijkstra_20x20Grid()
{
    return await _dijkstra20x20.FindPath(_start.NodeId, _end.NodeId);
}
```

## Tips for Accurate Benchmarking

1. **Always use Release mode**: `-c Release`
2. **Close other applications**: Minimize background interference
3. **Run multiple times**: BenchmarkDotNet does this automatically
4. **Stable environment**: Don't run during system updates
5. **Warm machine**: CPU should be at steady temperature

## Troubleshooting

**Benchmarks too slow?**
```bash
dotnet run -c Release --job short
```

**Want more detail?**
```bash
dotnet run -c Release --memory
```

**Compare with previous run?**
- BenchmarkDotNet saves results in `BenchmarkDotNet.Artifacts/results/`
- Use comparison tools to track performance over time

## Documentation

See [PathScout.Benchmarks/README.md](README.md) for:
- Detailed explanation of each benchmark
- How to customize benchmarks
- How to add new scenarios
- CI/CD integration examples
- Advanced BenchmarkDotNet features

---

**Ready to benchmark?** Run `dotnet run -c Release` and see your pathfinding performance in action!
