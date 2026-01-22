# Benchmark Results - MazePathfindingBenchmarks

## Results

### MazePathfindingBenchmarks Performance
```
| Method                   | Mean      | Error     | StdDev    | Rank | Gen0   | Gen1   | Allocated |
|------------------------- |----------:|----------:|----------:|-----:|-------:|-------:|----------:|
| '20x20 Maze - 50% Walls' |  9.573 us | 0.1889 us | 0.3259 us |    1 | 1.2360 | 0.0153 |  15.16 KB |
| '20x20 Maze - 30% Walls' | 10.015 us | 0.1944 us | 0.1723 us |    1 | 1.3123 | 0.0153 |  16.18 KB |
| '20x20 Maze - 10% Walls' | 10.194 us | 0.1092 us | 0.0968 us |    1 | 1.2970 | 0.0153 |  16.06 KB |
| '50x50 Maze - 30% Walls' | 31.543 us | 0.3109 us | 0.2909 us |    2 | 3.1128 | 0.0610 |  38.54 KB |
```

## Key Findings

### ?? Counter-Intuitive Discovery
**More walls = Faster pathfinding!**
- 50% walls: 9.573 ?s (fastest) ?
- 30% walls: 10.015 ?s
- 10% walls: 10.194 ?s (slowest)

**Reason**: Higher wall density reduces the search space (fewer explorable nodes).

### ?? Performance Highlights
- ? **20×20 mazes (400 nodes)**: Solve in ~10 microseconds
- ? **50×50 maze (2,500 nodes)**: Solves in 31.5 microseconds
- ? **Excellent scalability**: 6.25x more nodes = only 3.2x slower
- ? **Low memory**: Minimal allocations (15-16 KB for 400 nodes)
- ? **Stable performance**: Very low standard deviation (< 0.33 ?s)

### ?? Memory Efficiency
- 400 nodes: 15-16 KB
- 2,500 nodes: 38.54 KB
- Minimal GC pressure (Gen0: ~1.2-3.1 per 1000 ops)

## Performance Insights

### Why Sub-Linear Scaling?
A* is highly efficient because:
1. **Heuristic guidance**: Quickly focuses on promising paths
2. **Early termination**: Stops as soon as goal is found
3. **Squared distance**: Fast calculation without sqrt()
4. **Dictionary lookups**: O(1) node access
5. **Pre-calculated weights**: No runtime distance calculations

### Real-World Context
```
Even the largest maze (2,500 nodes) completes in 31.5 ?s

For a 60 FPS game (16.67ms per frame):
You could run 529 maze pathfinding operations per frame!
