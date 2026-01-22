# Running PathScout Benchmarks

## Important: Always Run in Release Mode!

BenchmarkDotNet requires Release mode for accurate measurements. The framework will warn you if you try to run in Debug mode.

### Windows (PowerShell)
```powershell
cd PathScout.Benchmarks
dotnet run -c Release
```

### Linux/Mac (Bash)
```bash
cd PathScout.Benchmarks
dotnet run -c Release
```

## Run Specific Benchmark Categories

```bash
# Grid benchmarks only
dotnet run -c Release --filter *GridPathfindingBenchmarks*

# Maze benchmarks only
dotnet run -c Release --filter *MazePathfindingBenchmarks*

# Graph structure benchmarks only
dotnet run -c Release --filter *GraphStructureBenchmarks*

# Edge case benchmarks only
dotnet run -c Release --filter *EdgeCaseBenchmarks*

# Algorithm comparison only
dotnet run -c Release --filter *AlgorithmComparisonBenchmarks*
```

## Run Specific Benchmark Methods

```bash
# Just the 20x20 grid
dotnet run -c Release --filter *Grid_20x20*

# All maze benchmarks
dotnet run -c Release --filter *Maze*

# All dense graph benchmarks
dotnet run -c Release --filter *DenseGraph*
```

## Export Results

```bash
# HTML report (opens in browser)
dotnet run -c Release --exporters html

# Markdown report (for README)
dotnet run -c Release --exporters markdown

# CSV for Excel analysis
dotnet run -c Release --exporters csv

# All formats
dotnet run -c Release --exporters html markdown csv json
```

## Troubleshooting

### Error: "Assembly is non-optimized"
**Solution**: Make sure you're using `-c Release` flag
```bash
dotnet run -c Release  # ? Correct
dotnet run             # ? Wrong - defaults to Debug
```

### Error: "Benchmark returns a deferred execution result"
**Solution**: This has been fixed. The benchmarks now return `Task<int>` (path count) instead of `Task<IEnumerable<PathNode>>`.

### Benchmarks Running Too Slow
**Quick mode** (fewer iterations):
```bash
dotnet run -c Release --job short
```

**Dry mode** (single iteration, for testing):
```bash
dotnet run -c Release --job dry
```

### Want More Detail
**Include memory diagnostics**:
```bash
dotnet run -c Release --memory
```

**List all available benchmarks**:
```bash
dotnet run -c Release --list flat
```

## Best Practices

1. **Close unnecessary programs** - Browser, IDE, etc.
2. **Stable power** - Plug in laptop, disable CPU throttling
3. **Multiple runs** - BenchmarkDotNet does this automatically
4. **Consistent environment** - Same conditions for each run

## Results Location

Results are saved to: `PathScout.Benchmarks/BenchmarkDotNet.Artifacts/results/`

## Quick Validation

To quickly validate the README performance claims:

```bash
# Should be < 10ms for medium graphs (100 nodes)
dotnet run -c Release --filter *Grid_10x10*

# Should be < 50ms for large graphs (400 nodes)
dotnet run -c Release --filter *Grid_20x20*
```

## More Information

See [README.md](README.md) for detailed documentation.
