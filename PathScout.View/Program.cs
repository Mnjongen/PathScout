using Raylib_cs;
using PathScout.View;
using System.Numerics;
using System.Diagnostics;

const int SCREEN_WIDTH = 1200;
const int SCREEN_HEIGHT = 800;
const string WINDOW_TITLE = "PathScout - A* Pathfinding Visualizer";

Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, WINDOW_TITLE);
Raylib.SetTargetFPS(60);

var visualizer = new PathfindingVisualizer();

// Create a sample grid to get started
visualizer.CreateSampleGrid(20, 20, 35, new Vector2(50, 50));

Task? pathfindingTask = null;
bool isMouseDown = false;

while (!Raylib.WindowShouldClose())
{
    // Input handling
    var mousePos = Raylib.GetMousePosition();
    
    // Mode switching
    if (Raylib.IsKeyPressed(KeyboardKey.One)) visualizer.CurrentMode = InteractionMode.AddNode;
    if (Raylib.IsKeyPressed(KeyboardKey.Two)) visualizer.CurrentMode = InteractionMode.RemoveNode;
    if (Raylib.IsKeyPressed(KeyboardKey.Three)) visualizer.CurrentMode = InteractionMode.AddConnection;
    if (Raylib.IsKeyPressed(KeyboardKey.Four)) visualizer.CurrentMode = InteractionMode.RemoveConnection;
    if (Raylib.IsKeyPressed(KeyboardKey.Five)) visualizer.CurrentMode = InteractionMode.MoveNode;
    if (Raylib.IsKeyPressed(KeyboardKey.S)) visualizer.CurrentMode = InteractionMode.SetStart;
    if (Raylib.IsKeyPressed(KeyboardKey.E)) visualizer.CurrentMode = InteractionMode.SetEnd;
    if (Raylib.IsKeyPressed(KeyboardKey.W)) visualizer.CurrentMode = InteractionMode.SetWall;

    // Actions
    if (Raylib.IsKeyPressed(KeyboardKey.Space) && pathfindingTask == null)
    {
        pathfindingTask = Task.Run(async () =>
        {
            long startTime = Stopwatch.GetTimestamp();
            await visualizer.RunPathfindingWithVisualization(async () =>
            {
                // This callback is called after each step
                await Task.CompletedTask;
            });
            TimeSpan duration = Stopwatch.GetElapsedTime(startTime);

            Console.WriteLine($"Pathfinding completed in {duration.TotalMilliseconds} ms");
        });
    }

    if (Raylib.IsKeyPressed(KeyboardKey.R))
    {
        visualizer.ResetPathfinding();
    }

    if (Raylib.IsKeyPressed(KeyboardKey.C))
    {
        visualizer.ClearGraph();
    }

    if (Raylib.IsKeyPressed(KeyboardKey.G))
    {
        visualizer.CreateSampleGrid(20, 20, 35, new Vector2(50, 50));
    }

    if (Raylib.IsKeyPressed(KeyboardKey.H))
    {
        // Create random graph - H for "random" (R is taken by Reset)
        visualizer.CreateRandomGraph(
            nodeCount: 20, 
            maxConnections: 4, 
            areaMin: new Vector2(50, 50), 
            areaMax: new Vector2(SCREEN_WIDTH - Renderer.GetUIPanelWidth() - 50, SCREEN_HEIGHT - 50));
    }

    if (Raylib.IsKeyPressed(KeyboardKey.M))
    {
        // Generate maze - M for "maze"
        visualizer.GenerateMaze(wallPercentage: 0.3f);
    }

    if (Raylib.IsKeyPressed(KeyboardKey.T))
    {
        // Cycle through visualization modes
        visualizer.CurrentVisualizationMode = visualizer.CurrentVisualizationMode switch
        {
            VisualizationMode.Instant => VisualizationMode.Animated,
            VisualizationMode.Animated => VisualizationMode.StepByStep,
            VisualizationMode.StepByStep => VisualizationMode.Instant,
            _ => VisualizationMode.Animated
        };
    }

    if (Raylib.IsKeyPressed(KeyboardKey.N) && visualizer.CurrentVisualizationMode == VisualizationMode.StepByStep)
    {
        visualizer.TriggerManualStep();
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
    {
        visualizer.CancelCurrentAction();
    }

    // Mouse handling (only if not clicking on UI panel)
    if (mousePos.X < SCREEN_WIDTH - Renderer.GetUIPanelWidth())
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            isMouseDown = true;
            visualizer.HandleClick(mousePos);
        }

        if (Raylib.IsMouseButtonDown(MouseButton.Left) && isMouseDown)
        {
            // Handle dragging for MoveNode mode
            if (visualizer.CurrentMode == InteractionMode.MoveNode && visualizer.DraggedNode != null)
            {
                visualizer.HandleDrag(mousePos);
            }
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            isMouseDown = false;
            visualizer.HandleRelease();
        }
    }
    else
    {
        // Released mouse on UI panel
        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            isMouseDown = false;
            visualizer.HandleRelease();
        }
    }

    // Check if pathfinding task is complete
    if (pathfindingTask != null && pathfindingTask.IsCompleted)
    {
        pathfindingTask = null;
    }

    // Rendering
    Raylib.BeginDrawing();
    Renderer.DrawScene(visualizer, mousePos);
    
    // Draw FPS
    Raylib.DrawFPS(10, 10);
    
    Raylib.EndDrawing();
}

Raylib.CloseWindow();
