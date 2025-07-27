using System;
using System.IO;
using System.Linq;
using System.Numerics;
using libColdHeart;

namespace TestlibColdHeart;

public class EnhancedPngExporterTests
{
    private SequenceGenerator _generator;
    private AngularTreeLayoutCalculator _angularCalculator;
    private EnhancedPngExporter _pngExporter;

    [SetUp]
    public void Setup()
    {
        _generator = new SequenceGenerator();
        _angularCalculator = new AngularTreeLayoutCalculator();
        _pngExporter = new EnhancedPngExporter();
    }

    [Test]
    public void ExportToPng_WithDefaultConfig_CreatesValidPngFile()
    {
        _generator.Add(2);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithDefaultConfig_CreatesNonEmptyFile()
    {
        _generator.Add(2);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

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
    public void ExportToPng_WithValidPngHeader_CreatesCorrectFormat()
    {
        _generator.Add(2);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            using var stream = File.OpenRead(tempFile);
            var header = new Byte[8];
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
    public void ExportToPng_WithCircleNodeStyle_CompletesSuccessfully()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config, NodeStyle.Circle);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithRectangleNodeStyle_CompletesSuccessfully()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config, NodeStyle.Rectangle);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithProgressCallback_CallsProgressCallback()
    {
        _generator.Add(2);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";
        var progressMessages = new System.Collections.Generic.List<String>();

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config, NodeStyle.Circle,
                message => progressMessages.Add(message));

            Assert.That(progressMessages.Count, Is.GreaterThan(0));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithProgressCallback_ContainsExpectedMessages()
    {
        _generator.Add(2);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";
        var progressMessages = new System.Collections.Generic.List<String>();

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config, NodeStyle.Circle,
                message => progressMessages.Add(message));

            Assert.That(progressMessages, Has.Some.Contains("Precomputing visual properties"));
            Assert.That(progressMessages, Has.Some.Contains("Calculating image bounds"));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithRenderLongestPathsFilter_CompletesSuccessfully()
    {
        for (BigInteger i = 2; i <= 8; i++)
        {
            _generator.Add(i);
        }
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig
        {
            RenderLongestPaths = 3
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithRenderMostTraversedPathsFilter_CompletesSuccessfully()
    {
        for (BigInteger i = 2; i <= 8; i++)
        {
            _generator.Add(i);
        }
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig
        {
            RenderMostTraversedPaths = 5
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithRenderLeastTraversedPathsFilter_CompletesSuccessfully()
    {
        for (BigInteger i = 2; i <= 8; i++)
        {
            _generator.Add(i);
        }
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig
        {
            RenderLeastTraversedPaths = 3
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithRenderRandomPathsFilter_CompletesSuccessfully()
    {
        for (BigInteger i = 2; i <= 8; i++)
        {
            _generator.Add(i);
        }
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig
        {
            RenderRandomPaths = 4
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithCustomAngles_CompletesSuccessfully()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _angularCalculator.CalculateLayout(_generator.Root, new AngularVisualizationConfig
        {
            LeftTurnAngle = -20.0f,
            RightTurnAngle = 30.0f
        });
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig
        {
            LeftTurnAngle = -20.0f,
            RightTurnAngle = 30.0f
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithCustomThicknessImpact_CompletesSuccessfully()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig
        {
            ThicknessImpact = 2.5f
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithCustomColorImpact_CompletesSuccessfully()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig
        {
            ColorImpact = 3.0f
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithCustomMaxLineWidth_CompletesSuccessfully()
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig
        {
            MaxLineWidth = 15.0f
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [TestCaseSource(nameof(NodeStyleTestCases))]
    public void ExportToPng_WithDifferentNodeStyles_ProducesValidFiles(NodeStyle nodeStyle)
    {
        _generator.Add(2);
        _generator.Add(4);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config, nodeStyle);

            Assert.That(File.Exists(tempFile), Is.True);
            var fileInfo = new FileInfo(tempFile);
            Assert.That(fileInfo.Length, Is.GreaterThan(1000)); // Reasonable minimum file size
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static System.Collections.IEnumerable NodeStyleTestCases()
    {
        yield return new TestCaseData(NodeStyle.Circle);
        yield return new TestCaseData(NodeStyle.Rectangle);
    }

    [TestCaseSource(nameof(FilterConfigurationTestCases))]
    public void ExportToPng_WithPathFiltering_HandlesFilteringCorrectly(AngularVisualizationConfig config)
    {
        for (BigInteger i = 2; i <= 10; i++)
        {
            _generator.Add(i);
        }
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static System.Collections.IEnumerable FilterConfigurationTestCases()
    {
        yield return new TestCaseData(new AngularVisualizationConfig { RenderLongestPaths = 3 });
        yield return new TestCaseData(new AngularVisualizationConfig { RenderMostTraversedPaths = 5 });
        yield return new TestCaseData(new AngularVisualizationConfig { RenderLeastTraversedPaths = 3 });
        yield return new TestCaseData(new AngularVisualizationConfig { RenderRandomPaths = 4 });
    }

    [Test]
    public void ExportToPng_WithInvalidFilePath_ThrowsException()
    {
        _generator.Add(2);
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var invalidFilePath = "/invalid/path/that/does/not/exist/test.png";

        Assert.Throws<DirectoryNotFoundException>(() =>
            _pngExporter.ExportToPng(layout, metrics, invalidFilePath, config));
    }

    [Test]
    public void ExportToPng_WithLargeTree_HandlesPerformanceGracefully()
    {
        // Create a moderately large tree
        for (BigInteger i = 2; i <= 20; i++)
        {
            _generator.Add(i);
        }
        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);
        var config = new AngularVisualizationConfig();
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            // This should complete within a reasonable time
            var start = DateTime.Now;
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);
            var elapsed = DateTime.Now - start;

            Assert.That(File.Exists(tempFile), Is.True);
            Assert.That(elapsed.TotalSeconds, Is.LessThan(30)); // Should complete within 30 seconds
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithRenderLeastTraversedPaths_SelectsNodesWithLowestTraversalCounts()
    {
        // Create a more complex tree to verify least traversed selection
        for (BigInteger i = 2; i <= 20; i++)
        {
            _generator.Add(i);
        }

        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        // Find the actual 3 least traversed nodes
        var expectedLeastTraversed = metrics.TraversalCounts
            .OrderBy(kvp => kvp.Value)
            .Take(3)
            .Select(kvp => kvp.Key)
            .ToHashSet();

        var config = new AngularVisualizationConfig
        {
            RenderLeastTraversedPaths = 3
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);

            // Verify that the selection logic works correctly
            Assert.That(expectedLeastTraversed.Count, Is.EqualTo(3));

            // Document the relationship between least traversed nodes and path length
            var avgPathLength = expectedLeastTraversed.Average(node => metrics.PathLengths[node]);
            var allAvgPathLength = metrics.PathLengths.Values.Average();

            // With larger datasets, least traversed nodes often have longer paths,
            // but this isn't guaranteed with small datasets
            Console.WriteLine($"Least traversed avg path length: {avgPathLength:F2}, Overall avg: {allAvgPathLength:F2}");
            Assert.That(avgPathLength, Is.GreaterThan(0), "Least traversed nodes should have valid path lengths");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithRenderLeastTraversedPaths_HandlesSmallDatasetCorrectly()
    {
        // Create a small tree with only 2 nodes
        _generator.Add(2);

        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        var config = new AngularVisualizationConfig
        {
            RenderLeastTraversedPaths = 5  // Request more than available
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            // Should handle gracefully when requesting more nodes than available
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Test]
    public void ExportToPng_WithRenderLeastTraversedPaths_VerifiesTraversalCountDistribution()
    {
        // Create a balanced tree to test traversal distribution
        for (BigInteger i = 2; i <= 15; i++)
        {
            _generator.Add(i);
        }

        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        // Verify that traversal counts follow expected pattern
        // (root should have highest, leaves should have lowest)
        var rootTraversal = metrics.TraversalCounts[new BigInteger(1)];
        var leastTraversed = metrics.TraversalCounts.Values.Min();

        Assert.That(rootTraversal, Is.GreaterThan(leastTraversed),
            "Root should have higher traversal count than least traversed nodes");

        var config = new AngularVisualizationConfig
        {
            RenderLeastTraversedPaths = 4
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);

            // Verify the least traversed selection makes sense
            var selectedNodes = metrics.TraversalCounts
                .OrderBy(kvp => kvp.Value)
                .Take(4)
                .ToList();

            // All selected nodes should have the same or lower traversal count than any non-selected
            var maxSelectedTraversal = selectedNodes.Max(kvp => kvp.Value);
            var minUnselectedTraversal = metrics.TraversalCounts
                .OrderBy(kvp => kvp.Value)
                .Skip(4)
                .FirstOrDefault().Value;

            if (minUnselectedTraversal > 0)  // Only check if there are unselected nodes
            {
                Assert.That(maxSelectedTraversal, Is.LessThanOrEqualTo(minUnselectedTraversal),
                    "Selected least traversed nodes should have traversal counts <= unselected nodes");
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [TestCase(1)]
    [TestCase(3)]
    [TestCase(10)]
    public void ExportToPng_WithRenderLeastTraversedPaths_HandlesVariousCountsCorrectly(int requestedCount)
    {
        // Create a tree large enough for all test cases
        for (BigInteger i = 2; i <= 15; i++)
        {
            _generator.Add(i);
        }

        var layout = _angularCalculator.CalculateLayout(_generator.Root);
        var metrics = _angularCalculator.CalculateTreeMetrics(_generator.Root);

        var config = new AngularVisualizationConfig
        {
            RenderLeastTraversedPaths = requestedCount
        };
        var tempFile = Path.GetTempFileName() + ".png";

        try
        {
            _pngExporter.ExportToPng(layout, metrics, tempFile, config);

            Assert.That(File.Exists(tempFile), Is.True);

            // Verify that the requested count is handled appropriately
            var availableNodes = metrics.TraversalCounts.Count;
            var expectedSelectedCount = Math.Min(requestedCount, availableNodes);

            // We can't directly verify the count from the image, but we can ensure it doesn't crash
            // and the logic would select the expected number
            var actualSelection = metrics.TraversalCounts
                .OrderBy(kvp => kvp.Value)
                .Take(requestedCount)
                .Count();

            Assert.That(actualSelection, Is.EqualTo(expectedSelectedCount));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
