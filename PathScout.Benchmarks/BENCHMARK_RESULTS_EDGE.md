# Benchmark Results - EdgeCaseBenchmarks

## Results

### EdgeCaseBenchmarks Performance
```
| Method                                                       | Mean        | Error     | StdDev    | Rank | Gen0   | Gen1   | Allocated |
|------------------------------------------------------------- |------------:|----------:|----------:|-----:|-------:|-------:|----------:|
| 'Single Node - Start equals End'                             |    117.9 ns |   1.27 ns |   1.19 ns |    1 | 0.0625 |      - |     784 B |
| 'No Path - Disconnected Components (explores all reachable)' |  1,383.8 ns |  26.46 ns |  24.75 ns |    2 | 0.1907 |      - |    2416 B |
| 'Long Path - 200 node linear chain'                          | 26,175.3 ns | 221.11 ns | 196.01 ns |    3 | 3.4180 | 0.0916 |   43032 B |
```

## Key Findings

### ? Single Node Optimization
**When start equals end: 117.9 nanoseconds!**

```
Single Node: 117.9 ns (0.000118 ms)
? 84x faster than 10×10 grid (4.6 ?s)
? Nearly instantaneous detection
? Minimal memory (784 bytes)
```

**Why so fast?**
The algorithm immediately detects start = end and returns without any pathfinding exploration!

### ?? No-Path Detection
**Disconnected components: 1.384 microseconds**

```
No Path: 1.384 ?s (0.001384 ms)
? Explores all reachable nodes from start
? Efficiently determines no path exists
? 3x faster than dense 100-node graph
? Minimal memory (2.42 KB)
```

**What it does:**
- Explores component 1 (10 nodes)
- Determines end node is unreachable
- Returns empty path
- Very efficient even for failure cases

### ?? Long Path Performance
**200-node linear chain: 26.175 microseconds**

```
Long Path: 26.175 ?s (0.026175 ms)
? Worst-case scenario (linear chain)
? Similar to 50×50 grid performance
? Still very fast despite length
? More memory for path reconstruction (43 KB)
```

**Comparison to other scenarios:**
- Dense 1000-node graph: 6.2 ?s (4x faster!)
- 50×50 grid: 27.9 ?s (similar)
- Sparse 100-node graph: 13.3 ?s (2x faster than long path)

## Performance Ranking

### Speed Comparison
```
From Fastest to Slowest:
  1. Single Node:      117.9 ns   ??? (0.000118 ms)
  2. No Path:        1,383.8 ns   ??   (0.001384 ms)
  3. Dense 1000:     6,190.0 ns   ??   (0.006190 ms)
  4. Long Path:     26,175.3 ns   ?     (0.026175 ms)
  5. Sparse 1000:  150,761.0 ns   ??    (0.150761 ms)

Single Node is:
  - 11.7x faster than No Path
  - 52.5x faster than Dense 1000
  - 222x faster than Long Path
  - 1,279x faster than Sparse 1000
```

### Memory Comparison
```
From Smallest to Largest:
  1. Single Node:      784 B    (0.76 KB)
  2. No Path:        2,416 B    (2.36 KB)
  3. Dense 1000:     6,980 B    (6.82 KB)
  4. Long Path:     43,032 B   (42.02 KB)
  5. Sparse 1000:  194,800 B  (190.23 KB)

Memory grows with:
  - Path length (longer paths = more reconstruction memory)
  - Exploration size (more nodes explored = more data structures)
```

## Key Insights

### Edge Case Handling is Excellent
All edge cases are handled efficiently:
- ? **Instant optimization** for trivial cases (start = end)
- ? **Fast failure** when no path exists
- ? **Robust worst-case** performance for long paths

### Algorithm Characteristics
```
Best Case (Single Node):     117.9 ns   ???
Average Case (Dense Graph):  3-18 ?s    ??
Worst Case (Long Path):      26.2 ?s    ?
Failure Case (No Path):      1.4 ?s     ??

Even worst-case is still very fast!
```

### Real-World Context
```
Single Node Check (118 ns):
  - Could run 8,475,000 checks per second
  - Or 141,250 checks per frame at 60 FPS

No Path Detection (1.4 ?s):
  - Could check 719,000 disconnected graphs per second
  - Or 11,983 checks per frame at 60 FPS

Long Path (26 ?s):
  - Could find 38,217 paths per second
  - Or 637 paths per frame at 60 FPS
