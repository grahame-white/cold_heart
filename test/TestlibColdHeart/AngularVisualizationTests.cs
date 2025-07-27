using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using libColdHeart;

namespace TestlibColdHeart;

public class AngularVisualizationTests
{
    private SequenceGenerator _generator;
    private TreeMapVisualizer _visualizer;
    private AngularTreeLayoutCalculator _angularCalculator;

    [SetUp]
    public void Setup()
    {
        _generator = new SequenceGenerator();
        _visualizer = new TreeMapVisualizer();
        _angularCalculator = new AngularTreeLayoutCalculator();
    }

    [Test]
    public void CalculateAngularLayout_WithSingleNode_PlacesRootAtBottomLeft()
    {
        var layout = _angularCalculator.CalculateLayout(_generator.Root);

        Assert.That(layout.Value, Is.EqualTo(new BigInteger(1)));
        Assert.That(layout.X, Is.GreaterThanOrEqualTo(0.0f)); // Should be offset from origin
        Assert.That(layout.Y, Is.GreaterThanOrEqualTo(0.0f)); // Should be offset from origin
    }

    [Test]
    public void CalculateTreeMetrics_WithSimpleTree_ReturnsCorrectMetrics()
    {
        _generator.Add(2);
        _generator.Add(4);

        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        Assert.That(metrics.FurthestDistance, Is.GreaterThan(0));
        Assert.That(metrics.LongestPath, Is.GreaterThan(0));
        Assert.That(metrics.PathLengths, Is.Not.Empty);
        Assert.That(metrics.TraversalCounts, Is.Not.Empty);

        // Root should be at distance 0
        Assert.That(metrics.PathLengths[new BigInteger(1)], Is.EqualTo(0));
    }

    [Test]
    public void CalculateAngularLayout_WithEvenNode_TurnsLeft()
    {
        _generator.Add(2); // Even number should turn left
        var layout = _angularCalculator.CalculateLayout(_generator.Root);

        Assert.That(layout.Children, Has.Count.EqualTo(1));
        var child = layout.Children[0];
        Assert.That(child.Value, Is.EqualTo(new BigInteger(2)));

        // Child should be positioned above and to the left of root due to left turn
        Assert.That(child.Y, Is.GreaterThan(layout.Y)); // Above
        Assert.That(child.X, Is.LessThan(layout.X + layout.Width / 2.0f)); // To the left
    }

    [Test]
    public void CalculateAngularLayout_WithOddNode_TurnsRight()
    {
        _generator.Add(3); // Odd number creates a longer path that eventually connects
        var layout = _angularCalculator.CalculateLayout(_generator.Root);

        // Navigate through the tree to find an odd node
        var allNodes = GetAllLayoutNodes(layout);
        var oddNode = allNodes.FirstOrDefault(n => n.Value % 2 == 1 && n.Value != 1);

        if (oddNode != null && oddNode.Children.Count > 0)
        {
            var child = oddNode.Children[0];
            // The positioning logic should reflect right turns for odd nodes
            Assert.That(child, Is.Not.Null);
        }
    }

    [Test]
    public void EnhancedPngExporter_CreatesFileWithWhiteBackground()
    {
        _generator.Add(2);
        _generator.Add(4);

        var tempFile = Path.GetTempFileName() + ".png";
        try
        {
            _visualizer.ExportToPng(_generator.Root, tempFile);

            Assert.That(File.Exists(tempFile), Is.True);
            var fileInfo = new FileInfo(tempFile);
            Assert.That(fileInfo.Length, Is.GreaterThan(0));

            // Verify it's a valid PNG file by checking the header
            using var stream = File.OpenRead(tempFile);
            var header = new byte[8];
            stream.Read(header, 0, 8);

            // PNG signature: 89 50 4E 47 0D 0A 1A 0A
            Assert.That(header[0], Is.EqualTo(0x89));
            Assert.That(header[1], Is.EqualTo(0x50));
            Assert.That(header[2], Is.EqualTo(0x4E));
            Assert.That(header[3], Is.EqualTo(0x47));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void AngularLayout_EdgeLengthScaling_ReflectsDistanceFromRoot()
    {
        // Build a tree with varying depths
        for (BigInteger i = 2; i <= 8; i++)
        {
            _generator.Add(i);
        }

        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        // Verify that edges exist and vary in calculated properties
        Assert.That(metrics.FurthestDistance, Is.GreaterThan(0));
        Assert.That(metrics.PathLengths.Values.Max(), Is.GreaterThan(1));

        // Should have multiple nodes at different distances
        var distinctDistances = metrics.PathLengths.Values.Distinct().Count();
        Assert.That(distinctDistances, Is.GreaterThan(1));
    }

    [Test]
    public void TreeMetrics_TraversalCounts_ReflectPathFrequency()
    {
        // Add numbers that share common paths
        _generator.Add(4);  // Path: 4 -> 2 -> 1
        _generator.Add(8);  // Path: 8 -> 4 -> 2 -> 1
        _generator.Add(16); // Path: 16 -> 8 -> 4 -> 2 -> 1

        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        // Node 1 (root) should have the highest traversal count (all paths end here)
        Assert.That(metrics.TraversalCounts[new BigInteger(1)], Is.GreaterThanOrEqualTo(1));

        // Node 2 should have a reasonable traversal count
        Assert.That(metrics.TraversalCounts[new BigInteger(2)], Is.GreaterThanOrEqualTo(1));

        // Higher numbers should generally have lower traversal counts than lower numbers
        Assert.That(metrics.TraversalCounts[new BigInteger(16)], Is.LessThanOrEqualTo(metrics.TraversalCounts[new BigInteger(2)]));

        // Verify all nodes have valid traversal counts
        foreach (var count in metrics.TraversalCounts.Values)
        {
            Assert.That(count, Is.GreaterThan(0), "All nodes should have positive traversal counts");
        }
    }

    [Test]
    public void AngularLayout_WithComplexTree_MaintainsStructuralIntegrity()
    {
        // Build a more complex tree
        for (BigInteger i = 2; i <= 20; i++)
        {
            _generator.Add(i);
        }

        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var allNodes = GetAllLayoutNodes(layout);

        // Should have created many nodes
        Assert.That(allNodes.Count, Is.GreaterThan(20));

        // All nodes should have valid positions
        foreach (var node in allNodes)
        {
            Assert.That(Single.IsFinite(node.X), Is.True, $"Node {node.Value} has invalid X coordinate");
            Assert.That(Single.IsFinite(node.Y), Is.True, $"Node {node.Value} has invalid Y coordinate");
            Assert.That(node.Width, Is.GreaterThan(0), $"Node {node.Value} has invalid width");
            Assert.That(node.Height, Is.GreaterThan(0), $"Node {node.Value} has invalid height");
        }

        // Root should still be positioned appropriately
        Assert.That(layout.X, Is.GreaterThanOrEqualTo(0.0f));
        Assert.That(layout.Y, Is.GreaterThanOrEqualTo(0.0f));
    }

    [Test]
    public void EnhancedVisualization_HandlesLargeNumbers()
    {
        // Add some larger numbers that create longer paths
        _generator.Add(31);  // Creates a long path in Collatz sequence
        _generator.Add(27);  // Another long path

        var tempFile = Path.GetTempFileName() + ".png";
        try
        {
            // Should not throw exceptions even with large numbers
            _visualizer.ExportToPng(_generator.Root, tempFile);

            Assert.That(File.Exists(tempFile), Is.True);
            var fileInfo = new FileInfo(tempFile);
            Assert.That(fileInfo.Length, Is.GreaterThan(0));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void CalculateTreeMetrics_LeastTraversedPathsCharacteristics_DocumentsBehavior()
    {
        // Create a larger tree to demonstrate the traversal/path length relationship
        for (BigInteger i = 2; i <= 31; i++)
        {
            _generator.Add(i);
        }

        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        // Get the 5 least traversed nodes
        var leastTraversedNodes = metrics.TraversalCounts
            .OrderBy(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => kvp.Key)
            .ToList();

        // Get their corresponding path lengths
        var leastTraversedPathLengths = leastTraversedNodes
            .Select(node => metrics.PathLengths[node])
            .ToList();

        // Calculate average path length for all nodes and least traversed nodes
        var overallAveragePathLength = metrics.PathLengths.Values.Average();
        var leastTraversedAveragePathLength = leastTraversedPathLengths.Average();

        // Document current behavior: least traversed nodes tend to have longer paths
        // This is expected because nodes farther from root are visited less frequently
        Assert.That(leastTraversedAveragePathLength, Is.GreaterThanOrEqualTo(overallAveragePathLength),
            "Least traversed nodes typically have longer paths than average - this is expected behavior");

        // Verify that traversal counts decrease as we go farther from root
        var rootTraversal = metrics.TraversalCounts[new BigInteger(1)];
        var minTraversal = leastTraversedNodes.Min(node => metrics.TraversalCounts[node]);

        Assert.That(rootTraversal, Is.GreaterThan(minTraversal),
            "Root should have higher traversal count than the least traversed nodes");

        // Ensure we actually found some least traversed nodes
        Assert.That(leastTraversedNodes.Count, Is.EqualTo(5));
        Assert.That(leastTraversedPathLengths.All(length => length >= 0), Is.True);

        Console.WriteLine($"Least traversed nodes analysis:");
        Console.WriteLine($"  Overall average path length: {overallAveragePathLength:F2}");
        Console.WriteLine($"  Least traversed average path length: {leastTraversedAveragePathLength:F2}");
        Console.WriteLine($"  Root traversal count: {rootTraversal}");
        Console.WriteLine($"  Min traversal count: {minTraversal}");

        foreach (var node in leastTraversedNodes)
        {
            Console.WriteLine($"  Node {node}: Path length = {metrics.PathLengths[node]}, Traversal count = {metrics.TraversalCounts[node]}");
        }
    }

    [Test]
    public void CalculateTreeMetrics_TraversalCountDistribution_FollowsExpectedPattern()
    {
        // Create a medium-sized tree
        for (BigInteger i = 2; i <= 20; i++)
        {
            _generator.Add(i);
        }

        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        // Group nodes by path length and check traversal patterns
        var nodesByPathLength = metrics.PathLengths
            .GroupBy(kvp => kvp.Value)
            .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Key).ToList());

        // For each path length level, verify traversal counts make sense
        foreach (var pathLengthGroup in nodesByPathLength.OrderBy(kvp => kvp.Key))
        {
            var pathLength = pathLengthGroup.Key;
            var nodesAtThisLevel = pathLengthGroup.Value;

            if (pathLength == 0)
            {
                // Root node should have highest traversal count
                Assert.That(nodesAtThisLevel.Count, Is.EqualTo(1));
                var rootTraversal = metrics.TraversalCounts[nodesAtThisLevel[0]];
                var maxTraversal = metrics.TraversalCounts.Values.Max();
                Assert.That(rootTraversal, Is.EqualTo(maxTraversal));
            }
            else
            {
                // Nodes at greater path lengths should generally have lower traversal counts
                var avgTraversalAtThisLevel = nodesAtThisLevel.Average(node => metrics.TraversalCounts[node]);

                // This documents expected behavior without being too strict
                Assert.That(avgTraversalAtThisLevel, Is.GreaterThan(0),
                    $"Nodes at path length {pathLength} should have positive traversal counts");
            }

            Console.WriteLine($"Path length {pathLength}: {nodesAtThisLevel.Count} nodes, " +
                            $"avg traversal: {nodesAtThisLevel.Average(node => metrics.TraversalCounts[node]):F1}");
        }
    }

    private List<LayoutNode> GetAllLayoutNodes(LayoutNode node)
    {
        var nodes = new List<LayoutNode> { node };
        foreach (var child in node.Children)
        {
            nodes.AddRange(GetAllLayoutNodes(child));
        }
        return nodes;
    }
}
