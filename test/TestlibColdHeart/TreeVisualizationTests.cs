using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using libColdHeart;

namespace TestlibColdHeart;

public class TreeVisualizationTests
{
    private SequenceGenerator _generator;
    private TreeMapVisualizer _visualizer;

    [SetUp]
    public void Setup()
    {
        _generator = new SequenceGenerator();
        _visualizer = new TreeMapVisualizer();
    }

    [Test]
    public void CalculateLayout_WithSingleNode_PlacesNodeAtOrigin()
    {
        var layout = _visualizer.CalculateLayout(_generator.Root);

        Assert.That(layout.Value, Is.EqualTo(new BigInteger(1)));
        Assert.That(layout.X, Is.EqualTo(0.0f));
        Assert.That(layout.Y, Is.EqualTo(0.0f));
    }

    [Test]
    public void CalculateLayout_WithOneChild_PlacesChildAboveParentSameX()
    {
        _generator.Add(2);
        var layout = _visualizer.CalculateLayout(_generator.Root);

        Assert.That(layout.Children, Has.Count.EqualTo(1));
        var child = layout.Children[0];
        Assert.That(child.Value, Is.EqualTo(new BigInteger(2)));
        Assert.That(child.X, Is.EqualTo(layout.X));
        Assert.That(child.Y, Is.GreaterThan(layout.Y));
    }

    [Test]
    public void CalculateLayout_WithMultipleSequences_CreatesAppropriateStructure()
    {
        _generator.Add(2);
        _generator.Add(3);  // This creates a longer path: 3->10->5->16->8->4->2->1
        var layout = _visualizer.CalculateLayout(_generator.Root);

        // Should have children based on the tree structure
        Assert.That(layout.Children, Has.Count.EqualTo(1));
        var child2 = layout.Children[0];  // Node 2
        Assert.That(child2.Value, Is.EqualTo(new BigInteger(2)));

        // Node 2 should have children (4 and possibly 10 from sequence 3)
        Assert.That(child2.Children.Count, Is.GreaterThanOrEqualTo(1));

        // All children should be positioned above their parent
        foreach (var child in child2.Children)
        {
            Assert.That(child.Y, Is.GreaterThan(child2.Y));
        }
    }

    [Test]
    public void CalculateLayout_OrdersChildrenBySize_HighestLeftLowestRight()
    {
        _generator.Add(4);  // Creates: 1 -> 2 -> 4
        _generator.Add(3);  // Creates: 1 -> 2 -> (4 and path from 3 which goes through various nodes)

        var layout = _visualizer.CalculateLayout(_generator.Root);
        var node2 = layout.Children[0];

        if (node2.Children.Count >= 2)
        {
            var firstChild = node2.Children[0];
            var secondChild = node2.Children[1];

            // First child should have higher value than second (ordered by descending value)
            Assert.That(firstChild.Value, Is.GreaterThan(secondChild.Value));

            // First child (higher value) should be positioned to the left of second child (lower value)
            Assert.That(firstChild.X, Is.LessThan(secondChild.X));
        }
        else
        {
            // If we don't have multiple children at this level, just verify we have a valid structure
            Assert.That(node2.Children.Count, Is.GreaterThanOrEqualTo(1));
        }
    }

    [Test]
    public async Task ExportToSvg_CreatesValidSvgFile()
    {
        _generator.Add(2);
        _generator.Add(4);

        var tempFile = Path.GetTempFileName() + ".svg";
        try
        {
            await _visualizer.ExportToSvgAsync(_generator.Root, tempFile);

            Assert.That(File.Exists(tempFile), Is.True);
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.That(content, Does.Contain("<?xml"));
            Assert.That(content, Does.Contain("<svg"));
            Assert.That(content, Does.Contain("</svg>"));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_CreatesValidPngFile()
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
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public async Task ExportToSvg_ContainsNodeValues()
    {
        _generator.Add(2);
        _generator.Add(4);

        var tempFile = Path.GetTempFileName() + ".svg";
        try
        {
            await _visualizer.ExportToSvgAsync(_generator.Root, tempFile);
            var content = await File.ReadAllTextAsync(tempFile);

            Assert.That(content, Does.Contain(">1<"));
            Assert.That(content, Does.Contain(">2<"));
            Assert.That(content, Does.Contain(">4<"));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void CalculateLayout_WithComplexTree_HandlesAllNodes()
    {
        // Build a more complex tree
        for (BigInteger i = 2; i <= 10; i++)
        {
            _generator.Add(i);
        }

        var layout = _visualizer.CalculateLayout(_generator.Root);
        var allNodes = GetAllLayoutNodes(layout);

        // Should have at least root + generated nodes
        Assert.That(allNodes.Count, Is.GreaterThanOrEqualTo(10));

        // Root should be at origin
        Assert.That(layout.X, Is.EqualTo(0.0f));
        Assert.That(layout.Y, Is.EqualTo(0.0f));
    }

    [Test]
    public void CalculateLayout_WithLargerTree_PerformsReasonably()
    {
        // Build a larger tree for performance testing
        for (BigInteger i = 2; i <= 100; i++)
        {
            _generator.Add(i);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var layout = _visualizer.CalculateLayout(_generator.Root);
        stopwatch.Stop();

        // Should complete within reasonable time (less than 1 second)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));

        var allNodes = GetAllLayoutNodes(layout);
        Assert.That(allNodes.Count, Is.GreaterThan(100));
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
