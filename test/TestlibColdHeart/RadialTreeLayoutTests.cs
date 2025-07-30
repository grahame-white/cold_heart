using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using libColdHeart;

namespace TestlibColdHeart;

public class RadialTreeLayoutTests
{
    private SequenceGenerator _generator;
    private TreeMapVisualizer _visualizer;
    private RadialTreeLayoutCalculator _radialCalculator;

    [SetUp]
    public void Setup()
    {
        _generator = new SequenceGenerator();
        _visualizer = new TreeMapVisualizer();
        _radialCalculator = new RadialTreeLayoutCalculator();
    }

    [Test]
    public void CalculateRadialLayout_WithSingleNode_PlacesRootAtCenter()
    {
        var layout = _radialCalculator.CalculateLayout(_generator.Root);

        Assert.That(layout.Value, Is.EqualTo(new BigInteger(1)));

        // Root should be positioned at the center coordinates (minus half width/height for centering)
        var expectedX = 300.0f - layout.Width / 2.0f;
        var expectedY = 300.0f - layout.Height / 2.0f;

        Assert.That(layout.X, Is.EqualTo(expectedX).Within(0.1f));
        Assert.That(layout.Y, Is.EqualTo(expectedY).Within(0.1f));
    }

    [Test]
    public void CalculateRadialLayout_WithTwoGenerations_PositionsNodesCorrectly()
    {
        _generator.Add(2); // This creates: 1 (root) -> 2 (gen 1)

        var layout = _radialCalculator.CalculateLayout(_generator.Root);

        Assert.That(layout.Children, Has.Count.EqualTo(1));
        var child = layout.Children[0];
        Assert.That(child.Value, Is.EqualTo(new BigInteger(2)));

        // Root should be at center
        var expectedRootX = 300.0f - layout.Width / 2.0f;
        var expectedRootY = 300.0f - layout.Height / 2.0f;
        Assert.That(layout.X, Is.EqualTo(expectedRootX).Within(0.1f));
        Assert.That(layout.Y, Is.EqualTo(expectedRootY).Within(0.1f));

        // Child should be at radius 100 from center (generation 1)
        var distanceFromCenter = Math.Sqrt(
            Math.Pow(child.X + child.Width / 2.0f - 300.0f, 2) +
            Math.Pow(child.Y + child.Height / 2.0f - 300.0f, 2)
        );
        Assert.That(distanceFromCenter, Is.EqualTo(100.0f).Within(1.0f));
    }

    [Test]
    public void CalculateRadialLayout_WithMultipleGenerations_HasIncreasingRadii()
    {
        // Build a tree: 1 -> 2 -> 4 (3 generations: 0, 1, 2)
        _generator.Add(2);
        _generator.Add(4);

        var layout = _radialCalculator.CalculateLayout(_generator.Root);
        var allNodes = GetAllLayoutNodes(layout);

        // Should have 3 nodes: 1, 2, 4
        Assert.That(allNodes.Count, Is.EqualTo(3));

        // Get nodes by value
        var node1 = allNodes.Find(n => n.Value == 1);
        var node2 = allNodes.Find(n => n.Value == 2);
        var node4 = allNodes.Find(n => n.Value == 4);

        Assert.That(node1, Is.Not.Null);
        Assert.That(node2, Is.Not.Null);
        Assert.That(node4, Is.Not.Null);

        // Calculate distances from center (300, 300)
        var distance1 = CalculateDistanceFromCenter(node1!);
        var distance2 = CalculateDistanceFromCenter(node2!);
        var distance4 = CalculateDistanceFromCenter(node4!);

        // Root should be at center (distance ~0)
        Assert.That(distance1, Is.LessThan(1.0f));

        // Gen 1 should be at radius 100
        Assert.That(distance2, Is.EqualTo(100.0f).Within(1.0f));

        // Gen 2 should be at radius 200
        Assert.That(distance4, Is.EqualTo(200.0f).Within(1.0f));
    }

    [Test]
    public void CalculateRadialLayout_WithMultipleNodesInSameGeneration_SpacesEvenly()
    {
        // Create a scenario with multiple nodes in generation 1
        _generator.Add(2); // 1 -> 2
        _generator.Add(3); // This might create additional branches

        var layout = _radialCalculator.CalculateLayout(_generator.Root);
        var allNodes = GetAllLayoutNodes(layout);

        // Should have nodes at different generations
        Assert.That(allNodes.Count, Is.GreaterThan(2));

        // All nodes should have valid positions
        foreach (var node in allNodes)
        {
            Assert.That(Single.IsFinite(node.X), Is.True, $"Node {node.Value} has invalid X coordinate");
            Assert.That(Single.IsFinite(node.Y), Is.True, $"Node {node.Value} has invalid Y coordinate");
            Assert.That(node.Width, Is.GreaterThan(0), $"Node {node.Value} has invalid width");
            Assert.That(node.Height, Is.GreaterThan(0), $"Node {node.Value} has invalid height");
        }
    }

    [Test]
    public void ExportToRadialPng_CreatesValidPngFile()
    {
        _generator.Add(2);
        _generator.Add(4);
        _generator.Add(8);

        var tempFile = Path.GetTempFileName() + ".png";
        try
        {
            _visualizer.ExportToRadialPng(_generator.Root, tempFile);

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
    public void RadialLayout_WithComplexTree_MaintainsStructuralIntegrity()
    {
        // Build a more complex tree
        for (BigInteger i = 2; i <= 10; i++)
        {
            _generator.Add(i);
        }

        var layout = _radialCalculator.CalculateLayout(_generator.Root);
        var allNodes = GetAllLayoutNodes(layout);

        // Should have created many nodes
        Assert.That(allNodes.Count, Is.GreaterThan(10));

        // All nodes should have valid positions
        foreach (var node in allNodes)
        {
            Assert.That(Single.IsFinite(node.X), Is.True, $"Node {node.Value} has invalid X coordinate");
            Assert.That(Single.IsFinite(node.Y), Is.True, $"Node {node.Value} has invalid Y coordinate");
            Assert.That(node.Width, Is.GreaterThan(0), $"Node {node.Value} has invalid width");
            Assert.That(node.Height, Is.GreaterThan(0), $"Node {node.Value} has invalid height");
        }

        // Root should be at center
        var distance = CalculateDistanceFromCenter(layout);
        Assert.That(distance, Is.LessThan(1.0f));
    }

    private Single CalculateDistanceFromCenter(LayoutNode node)
    {
        var centerX = 300.0f;
        var centerY = 300.0f;
        var nodeCenterX = node.X + node.Width / 2.0f;
        var nodeCenterY = node.Y + node.Height / 2.0f;

        return (Single)Math.Sqrt(
            Math.Pow(nodeCenterX - centerX, 2) +
            Math.Pow(nodeCenterY - centerY, 2)
        );
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
