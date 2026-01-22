using System.Numerics;

namespace PathScout.Tests.Structure
{
    public class PathNodeTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            var node = new PathNode(10, 20, 1);

            node.NodeId.Should().Be(1);
            node.Position.Should().Be(new Vector2(10, 20));
            node.State.Should().Be(NodeState.Unvisited);
            node.GScore.Should().Be(float.MaxValue);
            node.HScore.Should().Be(float.MaxValue);
            node.Parent.Should().BeNull();
            node.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void AddConnection_ShouldAddConnection()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(3, 4, 2);

            var result = node1.AddConnection(node2);

            result.Should().BeTrue();
            node1.ConnectionsCount.Should().Be(1);
            node1.GetConnections().Should().ContainSingle()
                .Which.node.Should().Be(node2);
        }

        [Fact]
        public void AddConnection_ShouldCalculateSquaredDistance()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(3, 4, 2); // Distance = 5, Squared = 25

            node1.AddConnection(node2);

            var connection = node1.GetConnections().First();
            connection.weight.Should().Be(25f);
        }

        [Fact]
        public void AddConnection_ShouldPreventNullConnection()
        {
            var node = new PathNode(0, 0, 1);

            var result = node.AddConnection(null!);

            result.Should().BeFalse();
            node.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void AddConnection_ShouldPreventSelfConnection()
        {
            var node = new PathNode(0, 0, 1);

            var result = node.AddConnection(node);

            result.Should().BeFalse();
            node.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void AddConnection_ShouldPreventDuplicateConnection()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(3, 4, 2);

            node1.AddConnection(node2);
            var result = node1.AddConnection(node2);

            result.Should().BeFalse();
            node1.ConnectionsCount.Should().Be(1);
        }

        [Fact]
        public void RemoveConnection_ShouldRemoveExistingConnection()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(3, 4, 2);
            node1.AddConnection(node2);

            var result = node1.RemoveConnection(node2);

            result.Should().BeTrue();
            node1.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void RemoveConnection_ShouldReturnFalseForNonExistent()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(3, 4, 2);

            var result = node1.RemoveConnection(node2);

            result.Should().BeFalse();
        }

        [Fact]
        public void RemoveConnectionById_ShouldRemoveConnection()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(3, 4, 2);
            node1.AddConnection(node2);

            var result = node1.RemoveConnection(2);

            result.Should().BeTrue();
            node1.ConnectionsCount.Should().Be(0);
        }

        [Fact]
        public void RemoveConnectionById_ShouldReturnFalseForNonExistent()
        {
            var node1 = new PathNode(0, 0, 1);

            var result = node1.RemoveConnection(999);

            result.Should().BeFalse();
        }

        [Fact]
        public void FScore_ShouldReturnSumOfGScoreAndHScore()
        {
            var node = new PathNode(0, 0, 1)
            {
                GScore = 10f,
                HScore = 15f
            };

            node.FScore.Should().Be(25f);
        }

        [Fact]
        public void Reset_ShouldResetAllScoresAndParent()
        {
            var node = new PathNode(0, 0, 1);
            var parent = new PathNode(1, 1, 2);
            node.State = NodeState.Closed;
            node.GScore = 10f;
            node.HScore = 15f;
            node.Parent = parent;

            node.Reset();

            node.State.Should().Be(NodeState.Unvisited);
            node.GScore.Should().Be(float.MaxValue);
            node.HScore.Should().Be(float.MaxValue);
            node.Parent.Should().BeNull();
        }

        [Fact]
        public void Reset_ShouldPreserveWallState()
        {
            var node = new PathNode(0, 0, 1);
            node.State = NodeState.Wall;

            node.Reset();

            node.State.Should().Be(NodeState.Wall);
        }

        [Fact]
        public void Equals_ShouldReturnTrueForSameNodeId()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(10, 10, 1);

            node1.Equals(node2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalseForDifferentNodeId()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(0, 0, 2);

            node1.Equals(node2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ShouldBeBasedOnNodeId()
        {
            var node1 = new PathNode(0, 0, 1);
            var node2 = new PathNode(10, 10, 1);

            node1.GetHashCode().Should().Be(node2.GetHashCode());
        }
    }
}
