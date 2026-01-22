using System.Diagnostics;

namespace PathScout.Tests.Integration
{
    /// <summary>
    /// Integration tests that verify the complete pathfinding workflow
    /// </summary>
    public class PathfindingIntegrationTests
    {
        [Fact]
        public async Task CompleteWorkflow_ShouldFindOptimalPath()
        {
            // Arrange - Create a realistic scenario
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create a 5x5 grid
            var grid = new PathNode[5, 5];
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    grid[x, y] = pathScout.AddNode(x * 10, y * 10);
                }
            }

            // Connect all adjacent nodes
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    if (x < 4) pathScout.AddConnection(grid[x, y].NodeId, grid[x + 1, y].NodeId);
                    if (y < 4) pathScout.AddConnection(grid[x, y].NodeId, grid[x, y + 1].NodeId);
                }
            }

            // Create walls to force a specific path
            grid[1, 1].State = NodeState.Wall;
            grid[2, 1].State = NodeState.Wall;
            grid[3, 1].State = NodeState.Wall;

            // Act - Find path from top-left to bottom-right
            var path = await pathScout.FindPath(grid[0, 0].NodeId, grid[4, 4].NodeId);

            // Assert
            path.Should().NotBeEmpty();
            path.First().Should().Be(grid[0, 0]);
            path.Last().Should().Be(grid[4, 4]);
            
            // Path should avoid walls
            path.Should().NotContain(grid[1, 1]);
            path.Should().NotContain(grid[2, 1]);
            path.Should().NotContain(grid[3, 1]);
        }

        [Fact]
        public async Task MultiplePathfindingOperations_ShouldWorkCorrectly()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(10, 0);
            var nodeC = pathScout.AddNode(20, 0);
            var nodeD = pathScout.AddNode(30, 0);

            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);
            pathScout.AddConnection(nodeB.NodeId, nodeC.NodeId);
            pathScout.AddConnection(nodeC.NodeId, nodeD.NodeId);

            // First path
            var path1 = await pathScout.FindPath(nodeA.NodeId, nodeC.NodeId);
            path1.Should().HaveCount(3);

            // Second path (should reset and work correctly)
            var path2 = await pathScout.FindPath(nodeB.NodeId, nodeD.NodeId);
            path2.Should().HaveCount(3);

            // Third path
            var path3 = await pathScout.FindPath(nodeA.NodeId, nodeD.NodeId);
            path3.Should().HaveCount(4);
        }

        [Fact]
        public async Task DynamicGraphModification_ShouldAffectPathfinding()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(10, 0);
            var nodeC = pathScout.AddNode(10, 10);
            var nodeD = pathScout.AddNode(20, 0);

            // Initial connections: A -> B -> D and A -> C -> D
            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);
            pathScout.AddConnection(nodeB.NodeId, nodeD.NodeId);
            pathScout.AddConnection(nodeA.NodeId, nodeC.NodeId);
            pathScout.AddConnection(nodeC.NodeId, nodeD.NodeId);

            var path1 = await pathScout.FindPath(nodeA.NodeId, nodeD.NodeId);
            path1.Should().Contain(nodeB); // Shorter path through B

            // Now block the direct path
            pathScout.RemoveConnection(nodeB.NodeId, nodeD.NodeId);

            var path2 = await pathScout.FindPath(nodeA.NodeId, nodeD.NodeId);
            path2.Should().Contain(nodeC); // Must now go through C
            path2.Should().NotContain(nodeB);
        }

        [Fact]
        public async Task LargeGraph_ShouldHandleEfficiently()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create a 20x20 grid (400 nodes)
            var gridSize = 20;
            var grid = new PathNode[gridSize, gridSize];
            
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    grid[x, y] = pathScout.AddNode(x * 10, y * 10);
                }
            }

            // Connect all adjacent nodes
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (x < gridSize - 1)
                        pathScout.AddConnection(grid[x, y].NodeId, grid[x + 1, y].NodeId);
                    if (y < gridSize - 1)
                        pathScout.AddConnection(grid[x, y].NodeId, grid[x, y + 1].NodeId);
                }
            }

            // Find path from corner to corner
            var startTime = Stopwatch.GetTimestamp();
            var path = await pathScout.FindPath(grid[0, 0].NodeId, grid[gridSize - 1, gridSize - 1].NodeId);
            var elapsed = Stopwatch.GetElapsedTime(startTime);

            path.Should().NotBeEmpty();
            path.First().Should().Be(grid[0, 0]);
            path.Last().Should().Be(grid[gridSize - 1, gridSize - 1]);
            
            // Should complete in reasonable time (adjust threshold as needed)
            elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task DisconnectedComponents_ShouldReturnNoPath()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create two separate components
            var component1A = pathScout.AddNode(0, 0);
            var component1B = pathScout.AddNode(10, 0);
            
            var component2A = pathScout.AddNode(100, 0);
            var component2B = pathScout.AddNode(110, 0);

            pathScout.AddConnection(component1A.NodeId, component1B.NodeId);
            pathScout.AddConnection(component2A.NodeId, component2B.NodeId);

            var startTime = Stopwatch.GetTimestamp();
            var path = await pathScout.FindPath(component1A.NodeId, component2A.NodeId);
            var elapsed = Stopwatch.GetElapsedTime(startTime);

            path.Should().BeEmpty();
        }

        [Fact]
        public void NodeModification_ShouldPersistAcrossOperations()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            var node1 = pathScout.AddNode(0, 0);
            var node2 = pathScout.AddNode(10, 0);
            
            pathScout.AddConnection(node1.NodeId, node2.NodeId);
            
            // Modify node state
            node1.State = NodeState.Wall;
            
            // Retrieve node again
            var retrievedNode = pathScout.GetNode(node1.NodeId);
            
            retrievedNode.Should().BeSameAs(node1);
            retrievedNode!.State.Should().Be(NodeState.Wall);
            retrievedNode.ConnectionsCount.Should().Be(1);
        }

        [Fact]
        public async Task PathWithSingleNode_ShouldReturnThatNode()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var singleNode = pathScout.AddNode(5, 5);

            var path = await pathScout.FindPath(singleNode.NodeId, singleNode.NodeId);

            path.Should().ContainSingle();
            path.First().Should().Be(singleNode);
        }

        [Theory]
        [InlineData(0, 0, 10, 0)]      // Horizontal
        [InlineData(0, 0, 0, 10)]      // Vertical
        [InlineData(0, 0, 10, 10)]     // Diagonal
        [InlineData(0, 0, 3, 4)]       // Pythagorean triple
        public async Task VariousDistances_ShouldCalculateCorrectly(float x1, float y1, float x2, float y2)
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            var node1 = pathScout.AddNode(x1, y1);
            var node2 = pathScout.AddNode(x2, y2);
            
            pathScout.AddConnection(node1.NodeId, node2.NodeId);

            var path = await pathScout.FindPath(node1.NodeId, node2.NodeId);

            path.Should().HaveCount(2);
            
            // Verify connection uses squared distance
            var connection = node1.GetConnections().First();
            var expectedSquared = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
            connection.weight.Should().BeApproximately(expectedSquared, 0.001f);
        }
    }
}
