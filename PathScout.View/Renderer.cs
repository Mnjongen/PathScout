using Raylib_cs;
using PathScout.Core.Structure;
using PathScout.Core.Enums;
using System.Numerics;

namespace PathScout.View
{
    public static class Renderer
    {
        private static readonly Color BACKGROUND = new Color(30, 30, 40, 255);
        private static readonly Color UI_BACKGROUND = new Color(40, 40, 50, 255);
        private static readonly Color UI_BORDER = new Color(80, 80, 90, 255);
        private static readonly Color TEXT_COLOR = Color.White;
        private static readonly Color HIGHLIGHT_COLOR = new Color(100, 150, 255, 255);

        // Node colors
        private static readonly Color NODE_UNVISITED = new Color(180, 180, 200, 255);
        private static readonly Color NODE_OPEN = new Color(255, 220, 100, 255);
        private static readonly Color NODE_CLOSED = new Color(100, 100, 150, 255);
        private static readonly Color NODE_PATH = new Color(100, 255, 100, 255);
        private static readonly Color NODE_WALL = new Color(50, 50, 50, 255);
        private static readonly Color NODE_START = new Color(50, 200, 255, 255);
        private static readonly Color NODE_END = new Color(255, 100, 100, 255);
        private static readonly Color CONNECTION_COLOR = new Color(150, 150, 170, 255);
        private static readonly Color ARROW_COLOR = new Color(200, 200, 220, 255);

        private const int UI_PANEL_WIDTH = 250;
        private const int UI_PADDING = 10;
        private const int BUTTON_HEIGHT = 30;
        private const float NODE_RADIUS = 15f;

        public static void DrawScene(PathfindingVisualizer visualizer, Vector2 mousePos)
        {
            Raylib.ClearBackground(BACKGROUND);

            // Draw connections first (so they appear behind nodes)
            DrawConnections(visualizer);

            // Draw connection preview
            DrawConnectionPreview(visualizer, mousePos);

            // Draw nodes
            DrawNodes(visualizer);

            // Draw UI
            DrawUI(visualizer, mousePos);
        }

        private static void DrawConnections(PathfindingVisualizer visualizer)
        {
            var drawnConnections = new HashSet<(int, int)>();

            foreach (var node in visualizer.GetAllNodes())
            {
                foreach (var (neighbor, _) in node.GetConnections())
                {
                    var pair = node.NodeId < neighbor.NodeId
                        ? (node.NodeId, neighbor.NodeId)
                        : (neighbor.NodeId, node.NodeId);

                    if (drawnConnections.Add(pair))
                    {
                        // Check if connection is bidirectional
                        bool isBidirectional = neighbor.GetConnections().Any(c => c.node == node);

                        // Draw the connection line
                        Raylib.DrawLineEx(node.Position, neighbor.Position, 2f, CONNECTION_COLOR);

                        // Draw arrow(s) to show direction
                        if (isBidirectional)
                        {
                            // Draw arrows on both ends
                            DrawArrowHead(node.Position, neighbor.Position, ARROW_COLOR, 0.7f);
                            DrawArrowHead(neighbor.Position, node.Position, ARROW_COLOR, 0.7f);
                        }
                        else
                        {
                            // Draw single arrow from node to neighbor
                            DrawArrowHead(node.Position, neighbor.Position, ARROW_COLOR, 0.85f);
                        }
                    }
                }
            }
        }

        private static void DrawArrowHead(Vector2 from, Vector2 to, Color color, float positionAlongLine)
        {
            // Calculate the position along the line where arrow should be drawn
            Vector2 arrowPos = Vector2.Lerp(from, to, positionAlongLine);
            
            // Direction vector
            Vector2 direction = Vector2.Normalize(to - from);
            
            // Perpendicular vector for arrow wings
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            // Arrow dimensions
            float arrowLength = 10f;
            float arrowWidth = 6f;
            
            // Calculate arrow points
            Vector2 arrowTip = arrowPos + direction * arrowLength * 0.3f;
            Vector2 arrowLeft = arrowPos - direction * arrowLength * 0.7f + perpendicular * arrowWidth;
            Vector2 arrowRight = arrowPos - direction * arrowLength * 0.7f - perpendicular * arrowWidth;
            
            // Draw filled triangle for arrow head
            Raylib.DrawTriangle(arrowTip, arrowLeft, arrowRight, color);
            
            // Draw outline for better visibility
            Raylib.DrawTriangleLines(arrowTip, arrowLeft, arrowRight, new Color(60, 60, 70, 255));
        }

        private static void DrawConnectionPreview(PathfindingVisualizer visualizer, Vector2 mousePos)
        {
            var startNode = visualizer.GetConnectionStartNode();
            if (startNode != null && 
                (visualizer.CurrentMode == InteractionMode.AddConnection || 
                 visualizer.CurrentMode == InteractionMode.RemoveConnection))
            {
                var color = visualizer.CurrentMode == InteractionMode.AddConnection 
                    ? new Color(100, 255, 100, 150) 
                    : new Color(255, 100, 100, 150);
                Raylib.DrawLineEx(startNode.Position, mousePos, 2f, color);
            }
        }

        private static void DrawNodes(PathfindingVisualizer visualizer)
        {
            foreach (var node in visualizer.GetAllNodes())
            {
                Color nodeColor = GetNodeColor(node, visualizer);
                
                // Highlight if being dragged
                bool isDragged = node == visualizer.DraggedNode;
                float radius = isDragged ? NODE_RADIUS * 1.3f : NODE_RADIUS;
                
                // Draw node circle
                Raylib.DrawCircleV(node.Position, radius, nodeColor);
                
                // Draw outline (thicker if dragged)
                if (isDragged)
                {
                    Raylib.DrawCircleLines((int)node.Position.X, (int)node.Position.Y, radius, new Color(255, 255, 100, 255));
                    Raylib.DrawCircleLines((int)node.Position.X, (int)node.Position.Y, radius + 2, new Color(255, 255, 100, 150));
                }
                else
                {
                    Raylib.DrawCircleLines((int)node.Position.X, (int)node.Position.Y, NODE_RADIUS, Color.Black);
                }

                // Draw node score (show actual distance if FScore is finite, otherwise show nothing)
                string scoreText = "";
                if (node.FScore < float.MaxValue)
                {
                    // Convert squared distance back to actual distance for display
                    float actualDistance = (float)Math.Sqrt(node.FScore);
                    scoreText = actualDistance.ToString("F0"); // No decimal places
                }
                
                if (!string.IsNullOrEmpty(scoreText))
                {
                    var textSize = Raylib.MeasureText(scoreText, 12);
                    Raylib.DrawText(scoreText, 
                        (int)(node.Position.X - textSize / 2), 
                        (int)(node.Position.Y - 6), 
                        12, 
                        Color.White);
                }
            }
        }

        private static Color GetNodeColor(PathNode node, PathfindingVisualizer visualizer)
        {
            // Start/End nodes take priority
            if (node == visualizer.StartNode)
                return NODE_START;
            if (node == visualizer.EndNode)
                return NODE_END;

            // Then state-based coloring
            return node.State switch
            {
                NodeState.Open => NODE_OPEN,
                NodeState.Closed => NODE_CLOSED,
                NodeState.Path => NODE_PATH,
                NodeState.Wall => NODE_WALL,
                _ => NODE_UNVISITED
            };
        }

        public static void DrawUI(PathfindingVisualizer visualizer, Vector2 mousePos)
        {
            int screenWidth = Raylib.GetScreenWidth();
            int panelX = screenWidth - UI_PANEL_WIDTH;

            // Draw panel background
            Raylib.DrawRectangle(panelX, 0, UI_PANEL_WIDTH, Raylib.GetScreenHeight(), UI_BACKGROUND);
            Raylib.DrawLine(panelX, 0, panelX, Raylib.GetScreenHeight(), UI_BORDER);

            int yPos = UI_PADDING;

            // Title
            Raylib.DrawText("PathScout Visualizer", panelX + UI_PADDING, yPos, 16, TEXT_COLOR);
            yPos += 30;

            // Mode Selection
            Raylib.DrawText("Mode:", panelX + UI_PADDING, yPos, 14, TEXT_COLOR);
            yPos += 20;

            var modes = new[]
            {
                (InteractionMode.AddNode, "Add Node (1)"),
                (InteractionMode.RemoveNode, "Remove Node (2)"),
                (InteractionMode.AddConnection, "Add Connection (3)"),
                (InteractionMode.RemoveConnection, "Remove Connection (4)"),
                (InteractionMode.MoveNode, "Move Node (5)"),
                (InteractionMode.SetStart, "Set Start (S)"),
                (InteractionMode.SetEnd, "Set End (E)"),
                (InteractionMode.SetWall, "Toggle Wall (W)")
            };

            foreach (var (mode, label) in modes)
            {
                var color = visualizer.CurrentMode == mode ? HIGHLIGHT_COLOR : TEXT_COLOR;
                Raylib.DrawText(label, panelX + UI_PADDING + 10, yPos, 12, color);
                yPos += 18;
            }

            yPos += 10;
            Raylib.DrawLine(panelX + UI_PADDING, yPos, panelX + UI_PANEL_WIDTH - UI_PADDING, yPos, UI_BORDER);
            yPos += 15;

            // Controls
            Raylib.DrawText("Controls:", panelX + UI_PADDING, yPos, 14, TEXT_COLOR);
            yPos += 20;

            var controls = new[]
            {
                "SPACE: Run Pathfinding",
                "R: Reset",
                "C: Clear All",
                "G: Create Sample Grid",
                "H: Create Random Graph",
                "M: Generate Maze",
                "T: Toggle Visualization Mode",
                "N: Next Step (Step Mode)",
                "ESC: Cancel Action"
            };

            foreach (var control in controls)
            {
                Raylib.DrawText(control, panelX + UI_PADDING + 5, yPos, 11, new Color(200, 200, 200, 255));
                yPos += 16;
            }

            yPos += 10;
            Raylib.DrawLine(panelX + UI_PADDING, yPos, panelX + UI_PANEL_WIDTH - UI_PADDING, yPos, UI_BORDER);
            yPos += 15;

            // Status
            Raylib.DrawText("Status:", panelX + UI_PADDING, yPos, 14, TEXT_COLOR);
            yPos += 20;

            var nodeCount = visualizer.GetAllNodes().Count();
            Raylib.DrawText($"Nodes: {nodeCount}", panelX + UI_PADDING + 5, yPos, 11, TEXT_COLOR);
            yPos += 16;

            var startText = visualizer.StartNode != null ? $"Node {visualizer.StartNode.NodeId}" : "None";
            Raylib.DrawText($"Start: {startText}", panelX + UI_PADDING + 5, yPos, 11, TEXT_COLOR);
            yPos += 16;

            var endText = visualizer.EndNode != null ? $"Node {visualizer.EndNode.NodeId}" : "None";
            Raylib.DrawText($"End: {endText}", panelX + UI_PADDING + 5, yPos, 11, TEXT_COLOR);
            yPos += 16;

            var visualizationModeText = visualizer.CurrentVisualizationMode switch
            {
                VisualizationMode.Instant => "Instant",
                VisualizationMode.Animated => "Animated",
                VisualizationMode.StepByStep => "Step-by-Step",
                _ => "Unknown"
            };
            Raylib.DrawText($"Mode: {visualizationModeText}", panelX + UI_PADDING + 5, yPos, 11, TEXT_COLOR);
            yPos += 16;

            if (visualizer.IsPathfinding)
            {
                Raylib.DrawText("Pathfinding...", panelX + UI_PADDING + 5, yPos, 11, HIGHLIGHT_COLOR);
                yPos += 16;
            }

            // Legend
            yPos += 10;
            Raylib.DrawLine(panelX + UI_PADDING, yPos, panelX + UI_PANEL_WIDTH - UI_PADDING, yPos, UI_BORDER);
            yPos += 15;

            Raylib.DrawText("Legend:", panelX + UI_PADDING, yPos, 14, TEXT_COLOR);
            yPos += 20;

            DrawLegendItem(panelX + UI_PADDING + 5, yPos, NODE_START, "Start Node"); yPos += 20;
            DrawLegendItem(panelX + UI_PADDING + 5, yPos, NODE_END, "End Node"); yPos += 20;
            DrawLegendItem(panelX + UI_PADDING + 5, yPos, NODE_UNVISITED, "Unvisited"); yPos += 20;
            DrawLegendItem(panelX + UI_PADDING + 5, yPos, NODE_OPEN, "Open"); yPos += 20;
            DrawLegendItem(panelX + UI_PADDING + 5, yPos, NODE_CLOSED, "Closed"); yPos += 20;
            DrawLegendItem(panelX + UI_PADDING + 5, yPos, NODE_PATH, "Path"); yPos += 20;
            DrawLegendItem(panelX + UI_PADDING + 5, yPos, NODE_WALL, "Wall"); yPos += 20;
            
            // Add note about node scores
            yPos += 5;
            Raylib.DrawText("Node numbers show F-score", panelX + UI_PADDING + 5, yPos, 10, new Color(150, 150, 150, 255));
            yPos += 12;
            Raylib.DrawText("(distance estimate)", panelX + UI_PADDING + 5, yPos, 10, new Color(150, 150, 150, 255));
        }

        private static void DrawLegendItem(int x, int y, Color color, String label)
        {
            Raylib.DrawCircle(x + 8, y + 6, 6, color);
            Raylib.DrawCircleLines(x + 8, y + 6, 6, Color.Black);
            Raylib.DrawText(label, x + 20, y, 11, TEXT_COLOR);
        }

        public static int GetUIPanelWidth() => UI_PANEL_WIDTH;
    }
}
