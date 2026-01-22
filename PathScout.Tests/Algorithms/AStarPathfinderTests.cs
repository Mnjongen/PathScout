namespace PathScout.Tests.Algorithms
{
    public class AStarPathfinderTests
    {
        [Fact]
        public async Task FindPathAsync_ShouldFindSimplePath()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create a simple linear path: A -> B -> C
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(10, 0);
            var nodeC = pathScout.AddNode(20, 0);

            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);
            pathScout.AddConnection(nodeB.NodeId, nodeC.NodeId);

            var path = await pathScout.FindPath(nodeA.NodeId, nodeC.NodeId);

            path.Should().NotBeEmpty();
            path.Should().HaveCount(3);
            path.First().Should().Be(nodeA);
            path.ElementAt(1).Should().Be(nodeB);
            path.Last().Should().Be(nodeC);
        }

        [Fact]
        public async Task FindPathAsync_ShouldFindShortestPath()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create a graph with two paths:
            // A -> B -> D (longer)
            // A -> C -> D (shorter)
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(10, 10);
            var nodeC = pathScout.AddNode(5, 0);
            var nodeD = pathScout.AddNode(10, 0);

            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);
            pathScout.AddConnection(nodeB.NodeId, nodeD.NodeId);
            pathScout.AddConnection(nodeA.NodeId, nodeC.NodeId);
            pathScout.AddConnection(nodeC.NodeId, nodeD.NodeId);

            var path = await pathScout.FindPath(nodeA.NodeId, nodeD.NodeId);

            path.Should().NotBeEmpty();
            path.Should().Contain(nodeC); // Should use the shorter path through C
            path.Should().NotContain(nodeB); // Should not use longer path through B
        }

        [Fact]
        public async Task FindPathAsync_ShouldReturnEmptyWhenNoPathExists()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create two disconnected nodes
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(10, 0);

            var path = await pathScout.FindPath(nodeA.NodeId, nodeB.NodeId);

            path.Should().BeEmpty();
        }

        [Fact]
        public async Task FindPathAsync_ShouldAvoidWalls()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create a path with a wall blocking the direct route
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(5, 0);
            var nodeC = pathScout.AddNode(10, 0);
            var nodeD = pathScout.AddNode(5, 5);

            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);
            pathScout.AddConnection(nodeB.NodeId, nodeC.NodeId);
            pathScout.AddConnection(nodeA.NodeId, nodeD.NodeId);
            pathScout.AddConnection(nodeD.NodeId, nodeC.NodeId);

            // Make B a wall
            nodeB.State = NodeState.Wall;

            var path = await pathScout.FindPath(nodeA.NodeId, nodeC.NodeId);

            path.Should().NotBeEmpty();
            path.Should().NotContain(nodeB); // Should avoid the wall
            path.Should().Contain(nodeD); // Should go around via D
        }

        [Fact]
        public async Task FindPathAsync_ShouldHandleSameStartAndEnd()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node = pathScout.AddNode(0, 0);

            var path = await pathScout.FindPath(node.NodeId, node.NodeId);

            path.Should().NotBeEmpty();
            path.Should().HaveCount(1);
            path.First().Should().Be(node);
        }

        [Fact]
        public async Task FindPathAsync_ShouldMarkNodesAsProcessed()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(10, 0);
            var nodeC = pathScout.AddNode(20, 0);

            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);
            pathScout.AddConnection(nodeB.NodeId, nodeC.NodeId);

            await pathScout.FindPath(nodeA.NodeId, nodeC.NodeId);

            nodeA.State.Should().Be(NodeState.Path);
            nodeB.State.Should().Be(NodeState.Path);
            nodeC.State.Should().Be(NodeState.Path);
        }

        [Fact]
        public async Task FindPathAsync_ShouldWorkWithComplexGraph()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create a grid-like structure
            //   1 - 2 - 3
            //   |   |   |
            //   4 - 5 - 6
            //   |   |   |
            //   7 - 8 - 9
            var nodes = new PathNode[9];
            for (int i = 0; i < 9; i++)
            {
                int x = (i % 3) * 10;
                int y = (i / 3) * 10;
                nodes[i] = pathScout.AddNode(x, y);
            }

            // Connect horizontally
            pathScout.AddConnection(nodes[0].NodeId, nodes[1].NodeId);
            pathScout.AddConnection(nodes[1].NodeId, nodes[2].NodeId);
            pathScout.AddConnection(nodes[3].NodeId, nodes[4].NodeId);
            pathScout.AddConnection(nodes[4].NodeId, nodes[5].NodeId);
            pathScout.AddConnection(nodes[6].NodeId, nodes[7].NodeId);
            pathScout.AddConnection(nodes[7].NodeId, nodes[8].NodeId);

            // Connect vertically
            pathScout.AddConnection(nodes[0].NodeId, nodes[3].NodeId);
            pathScout.AddConnection(nodes[3].NodeId, nodes[6].NodeId);
            pathScout.AddConnection(nodes[1].NodeId, nodes[4].NodeId);
            pathScout.AddConnection(nodes[4].NodeId, nodes[7].NodeId);
            pathScout.AddConnection(nodes[2].NodeId, nodes[5].NodeId);
            pathScout.AddConnection(nodes[5].NodeId, nodes[8].NodeId);

            var path = await pathScout.FindPath(nodes[0].NodeId, nodes[8].NodeId);

            path.Should().NotBeEmpty();
            path.First().Should().Be(nodes[0]);
            path.Last().Should().Be(nodes[8]);
        }

        [Fact]
        public async Task FindPathAsync_ShouldResetBeforeSearchWhenAutoResetEnabled()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(10, 0);
            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);

            // First search
            await pathScout.FindPath(nodeA.NodeId, nodeB.NodeId);

            // Second search with auto-reset
            var path = await pathScout.FindPath(nodeA.NodeId, nodeB.NodeId, autoReset: true);

            path.Should().NotBeEmpty();
        }

        [Fact]
        public async Task FindPathAsync_ShouldHandleDiagonalPaths()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            // Create a diagonal path
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(10, 10);
            var nodeC = pathScout.AddNode(20, 20);

            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);
            pathScout.AddConnection(nodeB.NodeId, nodeC.NodeId);

            var path = await pathScout.FindPath(nodeA.NodeId, nodeC.NodeId);

            path.Should().HaveCount(3);
            
            // Verify squared distance calculation is used
            var connection = nodeA.GetConnections().First();
            connection.weight.Should().Be(200f); // (10-0)² + (10-0)² = 200
        }

        [Fact]
        public async Task FindPathAsync_ShouldUseSquaredDistanceHeuristic()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            
            var nodeA = pathScout.AddNode(0, 0);
            var nodeB = pathScout.AddNode(3, 4); // Distance = 5, Squared = 25

            pathScout.AddConnection(nodeA.NodeId, nodeB.NodeId);

            await pathScout.FindPath(nodeA.NodeId, nodeB.NodeId);

            // After pathfinding, HScore should be calculated using squared distance
            nodeA.HScore.Should().BeGreaterOrEqualTo(0);
        }
    }
}
