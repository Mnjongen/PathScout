namespace PathScout.Tests.Core
{
    public class PathScoutTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithAlgorithm()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);

            pathScout.Should().NotBeNull();
            pathScout.NodeCount.Should().Be(0);
        }

        [Fact]
        public void AddNode_ShouldAddNodeAndReturnIt()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);

            var node = pathScout.AddNode(10, 20);

            node.Should().NotBeNull();
            node.Position.X.Should().Be(10);
            node.Position.Y.Should().Be(20);
            node.NodeId.Should().Be(1);
            pathScout.NodeCount.Should().Be(1);
        }

        [Fact]
        public void AddNode_ShouldIncrementNodeIds()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);

            var node1 = pathScout.AddNode(0, 0);
            var node2 = pathScout.AddNode(10, 10);
            var node3 = pathScout.AddNode(20, 20);

            node1.NodeId.Should().Be(1);
            node2.NodeId.Should().Be(2);
            node3.NodeId.Should().Be(3);
        }

        [Fact]
        public void GetNode_ShouldReturnNodeById()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var addedNode = pathScout.AddNode(10, 20);

            var retrievedNode = pathScout.GetNode(addedNode.NodeId);

            retrievedNode.Should().NotBeNull();
            retrievedNode.Should().BeSameAs(addedNode);
        }

        [Fact]
        public void GetNode_ShouldReturnNullForNonExistentId()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);

            var node = pathScout.GetNode(999);

            node.Should().BeNull();
        }

        [Fact]
        public void RemoveNode_ShouldRemoveNodeAndReturnTrue()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node = pathScout.AddNode(10, 20);

            var result = pathScout.RemoveNode(node.NodeId);

            result.Should().BeTrue();
            pathScout.NodeCount.Should().Be(0);
            pathScout.GetNode(node.NodeId).Should().BeNull();
        }

        [Fact]
        public void RemoveNode_ShouldRemoveConnectionsFromOtherNodes()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node1 = pathScout.AddNode(0, 0);
            var node2 = pathScout.AddNode(10, 0);
            var node3 = pathScout.AddNode(20, 0);

            pathScout.AddConnection(node1.NodeId, node2.NodeId);
            pathScout.AddConnection(node2.NodeId, node3.NodeId);

            pathScout.RemoveNode(node2.NodeId);

            node1.ConnectionsCount.Should().Be(0);
            node3.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void RemoveNode_ShouldReturnFalseForNonExistent()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);

            var result = pathScout.RemoveNode(999);

            result.Should().BeFalse();
        }

        [Fact]
        public void AddConnection_ShouldCreateBidirectionalConnection()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node1 = pathScout.AddNode(0, 0);
            var node2 = pathScout.AddNode(3, 4);

            var result = pathScout.AddConnection(node1.NodeId, node2.NodeId);

            result.Should().BeTrue();
            node1.ConnectionsCount.Should().Be(1);
            node2.ConnectionsCount.Should().Be(1);
            node1.GetConnections().Should().Contain(c => c.node == node2);
            node2.GetConnections().Should().Contain(c => c.node == node1);
        }

        [Fact]
        public void AddConnection_ShouldPreventSelfConnection()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node = pathScout.AddNode(0, 0);

            var result = pathScout.AddConnection(node.NodeId, node.NodeId);

            result.Should().BeFalse();
            node.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void AddConnection_ShouldReturnFalseForNonExistentNodes()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node = pathScout.AddNode(0, 0);

            var result = pathScout.AddConnection(node.NodeId, 999);

            result.Should().BeFalse();
        }

        [Fact]
        public void RemoveConnection_ShouldRemoveConnection()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node1 = pathScout.AddNode(0, 0);
            var node2 = pathScout.AddNode(10, 0);
            pathScout.AddConnection(node1.NodeId, node2.NodeId);

            var result = pathScout.RemoveConnection(node1.NodeId, node2.NodeId);

            result.Should().BeTrue();
            node1.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void RemoveConnection_ShouldReturnFalseForNonExistent()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node1 = pathScout.AddNode(0, 0);
            var node2 = pathScout.AddNode(10, 0);

            var result = pathScout.RemoveConnection(node1.NodeId, node2.NodeId);

            result.Should().BeFalse();
        }

        [Fact]
        public void ClearAllNodes_ShouldRemoveAllNodes()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            pathScout.AddNode(0, 0);
            pathScout.AddNode(10, 10);
            pathScout.AddNode(20, 20);

            pathScout.ClearAllNodes();

            pathScout.NodeCount.Should().Be(0);
            pathScout.GetAllNodes().Should().BeEmpty();
        }

        [Fact]
        public void ResetNodeStates_ShouldResetAllNodes()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node1 = pathScout.AddNode(0, 0);
            var node2 = pathScout.AddNode(10, 10);

            node1.State = NodeState.Closed;
            node1.GScore = 10f;
            node2.State = NodeState.Open;
            node2.HScore = 15f;

            pathScout.ResetNodeStates();

            node1.State.Should().Be(NodeState.Unvisited);
            node1.GScore.Should().Be(float.MaxValue);
            node1.HScore.Should().Be(float.MaxValue);
            node2.State.Should().Be(NodeState.Unvisited);
            node2.GScore.Should().Be(float.MaxValue);
            node2.HScore.Should().Be(float.MaxValue);
        }

        [Fact]
        public void ResetNodeStates_ShouldPreserveWalls()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node = pathScout.AddNode(0, 0);
            node.State = NodeState.Wall;

            pathScout.ResetNodeStates();

            node.State.Should().Be(NodeState.Wall);
        }

        [Fact]
        public void GetAllNodes_ShouldReturnAllNodes()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node1 = pathScout.AddNode(0, 0);
            var node2 = pathScout.AddNode(10, 10);
            var node3 = pathScout.AddNode(20, 20);

            var allNodes = pathScout.GetAllNodes();

            allNodes.Should().HaveCount(3);
            allNodes.Should().Contain(node1);
            allNodes.Should().Contain(node2);
            allNodes.Should().Contain(node3);
        }

        [Fact]
        public async Task FindPath_ShouldReturnEmptyForNonExistentStartNode()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node = pathScout.AddNode(0, 0);

            var path = await pathScout.FindPath(999, node.NodeId);

            path.Should().BeEmpty();
        }

        [Fact]
        public async Task FindPath_ShouldReturnEmptyForNonExistentEndNode()
        {
            var pathScout = new PathScout.Core.PathScout(AlgorithmType.AStar);
            var node = pathScout.AddNode(0, 0);

            var path = await pathScout.FindPath(node.NodeId, 999);

            path.Should().BeEmpty();
        }
    }
}
