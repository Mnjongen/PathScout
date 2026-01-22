# PathScout.View

Interactive visualization for PathScout pathfinding algorithms using Raylib.

## Screenshots

![PathScout Demo](../Resources/demo.gif)
*Watch A* algorithm explore the graph in real-time*

![Interactive Features](../Resources/Overview.png)
*Build and modify graphs with intuitive controls*

## Features

### Interactive Graph Building
- **Add Nodes**: Click anywhere to create new nodes
- **Remove Nodes**: Click on nodes to delete them
- **Add Connections**: Click two nodes to connect them
- **Remove Connections**: Click two nodes to disconnect them
- **Move Nodes**: Drag nodes to reposition them (distances auto-recalculate)
- **Set Start/End**: Define pathfinding endpoints
- **Toggle Walls**: Mark nodes as obstacles
- **Generate Grid**: Create a structured 20x20 grid
- **Generate Random**: Create random graph with organic connections
- **Generate Maze**: Randomly set ~30% of nodes as walls to create maze-like obstacles

### Algorithm Visualization
- **Real-time Animation**: Watch A* algorithm explore the graph
- **Instant Mode**: Solve immediately without animation
- **Animated Mode**: 50ms delay between steps for clear visualization
- **Step-by-step Mode**: Manually step through each iteration
- **Directional Arrows**: Visual indicators showing connection direction
  - Two arrows: Bidirectional connection
  - One arrow: Unidirectional connection
- **Color-coded States**:
  - Cyan/Blue: Start node
  - Red: End node
  - Green: Final path
  - Yellow: Open nodes (being considered)
  - Dark Blue: Closed nodes (already evaluated)
  - Gray: Unvisited nodes
  - Black: Walls (blocked)
- **Score Display**: Nodes show their F-score (estimated total distance) when pathfinding is active

### Keyboard Controls

#### Mode Selection
- `1` - Add Node mode
- `2` - Remove Node mode
- `3` - Add Connection mode
- `4` - Remove Connection mode
- `5` - Move Node mode (drag to reposition)
- `S` - Set Start node
- `E` - Set End node
- `W` - Toggle Wall mode

#### Actions
- `SPACE` - Run pathfinding algorithm
- `R` - Reset (clear pathfinding state)
- `C` - Clear all nodes
- `G` - Generate sample 20x20 grid
- `H` - Generate random graph (20 nodes)
- `M` - Generate maze (30% walls)
- `T` - Toggle visualization mode (Instant ? Animated ? Step-by-Step)
- `N` - Next step (when in Step-by-Step mode)
- `ESC` - Cancel current action

#### Mouse
- `Left Click` - Interact based on current mode
- `Click + Drag` - Move nodes (in Move Node mode)

## Getting Started

### Prerequisites
- .NET 10.0
- Raylib-cs (automatically restored)

### Running the Application

```bash
cd PathScout.View
dotnet run
```

Or press F5 in Visual Studio with PathScout.View as startup project.

## Usage Guide

### 1. Creating a Graph

**Option A: Use Sample Grid**
1. Press `G` to generate a 20x20 grid
2. Start and end nodes are automatically set

**Option B: Use Random Graph**
1. Press `H` to generate a random graph with 20 nodes
2. Nodes are randomly positioned and connected to nearby neighbors
3. Start and end nodes are automatically set

**Option C: Manual Creation**
1. Press `1` for Add Node mode
2. Click anywhere to place nodes
3. Press `3` for Add Connection mode
4. Click two nodes to connect them
5. Press `S` and click a node to set start
6. Press `E` and click a node to set end

### 2. Adding Obstacles

**Option A: Manual Walls**
1. Press `W` for Toggle Wall mode
2. Click nodes to make them walls (black)
3. Click again to remove wall status

**Option B: Generate Maze**
1. Create a graph first (Grid with `G` or Random with `H`)
2. Press `M` to generate a maze
3. Approximately 30% of nodes will become walls
4. Start and end nodes are protected and won't become walls

### 3. Moving Nodes
1. Press `5` for Move Node mode
2. Click and drag a node to reposition it
3. Release to recalculate all connection distances automatically

### 4. Running Pathfinding

**Instant Mode:**
1. Ensure start and end nodes are set
2. Press `SPACE` to run
3. Path appears immediately without animation

**Animated Mode (default):**
1. Ensure start and end nodes are set
2. Press `SPACE` to run
3. Watch the algorithm animate with 50ms delays

**Step-by-Step Mode:**
1. Press `T` twice to cycle to Step-by-Step mode
2. Press `SPACE` to start
3. Press `N` to advance one step at a time

**Switching Modes:**
- Press `T` to cycle: Instant ? Animated ? Step-by-Step ? Instant

### 5. Modifying the Graph
1. Press `R` to reset pathfinding state
2. Make changes to nodes/connections
3. Press `SPACE` to re-run pathfinding

## UI Layout

```
+-----------------------------------+-------------+
|                                   | Controls    |
|                                   |             |
|     Graph Visualization           | Status      |
|                                   |             |
|                                   | Legend      |
+-----------------------------------+-------------+
```

### Right Panel Contents
- **Current Mode**: Active interaction mode
- **Controls**: Keyboard shortcuts reference
- **Status**: 
  - Node count
  - Start/End node IDs
  - Visualization mode (Instant/Animated/Step-by-Step)
  - Pathfinding status
- **Legend**: Color coding reference

## Architecture

### Key Components

**PathfindingVisualizer.cs**
- Manages graph state and user interactions
- Bridges UI events with PathScout.Core
- Handles pathfinding execution with visualization callbacks

**Renderer.cs**
- Pure rendering logic using Raylib
- Draws nodes, connections, and UI
- Color-codes based on node states

**Program.cs**
- Main game loop
- Input handling
- Frame updates

## Customization

### Adjusting Animation Speed
In `PathfindingVisualizer.cs`, modify the delay in the Animated mode:
```csharp
case VisualizationMode.Animated:
    await Task.Delay(50); // Change milliseconds (lower = faster)
    break;
```

### Changing Colors
In `Renderer.cs`, modify color constants:
```csharp
private static readonly Color NODE_OPEN = new Color(255, 220, 100, 255);
```

### Grid Size
```csharp
visualizer.CreateSampleGrid(rows: 20, cols: 20, spacing: 35, offset);
```

## Performance

- Handles 100+ nodes smoothly
- 60 FPS rendering
- Async pathfinding doesn't block UI
- Efficient connection drawing (no duplicates)

## Troubleshooting

**Window doesn't open:**
- Ensure Raylib-cs package is restored
- Try `dotnet restore`

**Pathfinding doesn't start:**
- Verify start and end nodes are set (green and red)
- Check nodes are connected (lines visible)
- Ensure previous pathfinding completed

**Can't click nodes:**
- Make sure you're not clicking in the UI panel (right side)
- Try clicking closer to node centers

## Future Enhancements

Potential additions:
- [ ] Dijkstra's algorithm support
- [ ] Weighted edges visualization
- [ ] Export/import graph layouts
- [ ] Diagonal connections
- [ ] Multiple pathfinding comparisons
- [ ] Performance metrics display
- [ ] Undo/redo functionality
