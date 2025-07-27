using System;
using System.IO;
using System.Numerics;
using libColdHeart;

namespace TestlibColdHeart;

public class SvgExporterTests
{
    private SequenceGenerator _generator;
    private TreeMapVisualizer _visualizer;
    private SvgExporter _svgExporter;

    [SetUp]
    public void Setup()
    {
        _generator = new SequenceGenerator();
        _visualizer = new TreeMapVisualizer();
        _svgExporter = new SvgExporter();
    }

    [Test]
    public void ExportToSvg_WithSingleNode_GeneratesValidSvg()
    {
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
    }

    [Test]
    public void ExportToSvg_WithSingleNode_ContainsSvgElement()
    {
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("<svg"));
    }

    [Test]
    public void ExportToSvg_WithSingleNode_ContainsTransformGroup()
    {
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("<g transform="));
    }

    [Test]
    public void ExportToSvg_WithSingleNode_ContainsRootNodeValue()
    {
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("1"));
    }

    [Test]
    public void ExportToSvg_WithSingleNode_ContainsRectangleElement()
    {
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("<rect"));
    }

    [Test]
    public void ExportToSvg_WithSingleNode_ContainsTextElement()
    {
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("<text"));
    }

    [Test]
    public void ExportToSvg_WithMultipleNodes_ContainsConnections()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("<line"));
    }

    [Test]
    public void ExportToSvg_WithMultipleNodes_ContainsAllNodeValues()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("1"));
        Assert.That(svg, Does.Contain("2"));
        Assert.That(svg, Does.Contain("4"));
    }

    [Test]
    public void ExportToSvg_WithMultipleNodes_ContainsMultipleRectangles()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        var rectCount = CountOccurrences(svg, "<rect");
        Assert.That(rectCount, Is.GreaterThanOrEqualTo(3)); // Root + at least 2 children
    }

    [Test]
    public void ExportToSvg_WithComplexTree_GeneratesValidStructure()
    {
        for (BigInteger i = 2; i <= 8; i++)
        {
            _generator.Add(i);
        }
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.StartWith("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
        Assert.That(svg.Trim(), Does.EndWith("</svg>"));
    }

    [Test]
    public void ExportToSvg_WithComplexTree_ContainsExpectedElements()
    {
        for (BigInteger i = 2; i <= 8; i++)
        {
            _generator.Add(i);
        }
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("xmlns=\"http://www.w3.org/2000/svg\""));
        Assert.That(svg, Does.Contain("</g>"));
        Assert.That(svg, Does.Contain("</svg>"));
    }

    [Test]
    public void ExportToSvg_WithLargeNumbers_RendersCorrectly()
    {
        var largeNumber = BigInteger.Parse("12345678901234567890");
        var node = new TreeNode(largeNumber);
        var layout = new LayoutNode(largeNumber) { X = 0, Y = 0, Width = 100, Height = 30 };

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("12345678901234567890"));
    }

    [Test]
    public void ExportToSvg_WithNegativeCoordinates_HandlesTransformCorrectly()
    {
        var layout = new LayoutNode(new BigInteger(1)) { X = -50, Y = -30, Width = 100, Height = 30 };

        var svg = _svgExporter.ExportToSvg(layout);

        Assert.That(svg, Does.Contain("transform="));
        Assert.That(svg, Does.Contain("<svg"));
    }

    [Test]
    public void ExportToSvg_ResultCanBeWrittenToFile()
    {
        _generator.Add(2);
        var layout = _visualizer.CalculateLayout(_generator.Root);
        var tempFile = Path.GetTempFileName() + ".svg";

        try
        {
            var svg = _svgExporter.ExportToSvg(layout);
            File.WriteAllText(tempFile, svg);

            Assert.That(File.Exists(tempFile), Is.True);
            var content = File.ReadAllText(tempFile);
            Assert.That(content, Is.EqualTo(svg));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [TestCaseSource(nameof(NodeCountTestCases))]
    public void ExportToSvg_WithVariousNodeCounts_GeneratesCorrectElementCount(Int32 maxValue, Int32 expectedMinimumNodes)
    {
        for (BigInteger i = 2; i <= maxValue; i++)
        {
            _generator.Add(i);
        }
        var layout = _visualizer.CalculateLayout(_generator.Root);

        var svg = _svgExporter.ExportToSvg(layout);

        var rectCount = CountOccurrences(svg, "<rect");
        var textCount = CountOccurrences(svg, "<text");
        
        Assert.That(rectCount, Is.GreaterThanOrEqualTo(expectedMinimumNodes));
        Assert.That(textCount, Is.GreaterThanOrEqualTo(expectedMinimumNodes));
        Assert.That(rectCount, Is.EqualTo(textCount)); // Each rectangle should have corresponding text
    }

    private static System.Collections.IEnumerable NodeCountTestCases()
    {
        yield return new TestCaseData(2, 2); // At least root + node 2
        yield return new TestCaseData(4, 3); // Root + node 2 + node 4
        yield return new TestCaseData(8, 4); // More complex tree structure
    }

    private static Int32 CountOccurrences(String text, String substring)
    {
        Int32 count = 0;
        Int32 index = 0;
        while ((index = text.IndexOf(substring, index)) != -1)
        {
            count++;
            index += substring.Length;
        }
        return count;
    }
}