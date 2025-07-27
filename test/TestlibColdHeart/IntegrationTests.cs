using System;
using System.IO;
using System.Numerics;
using libColdHeart;

namespace TestlibColdHeart;

public class IntegrationTests
{
    [Test]
    public void FullPipeline_WithAngularVisualization_CompletesSuccessfully()
    {
        var generator = new SequenceGenerator();
        var visualizer = new TreeMapVisualizer();
        var tempPngFile = Path.GetTempFileName() + ".png";

        try
        {
            // Generate sequence
            for (BigInteger i = 2; i <= 10; i++)
            {
                generator.Add(i);
            }

            // Export angular PNG
            var config = new AngularVisualizationConfig();
            visualizer.ExportToAngularPng(generator.Root, tempPngFile, config);

            Assert.That(File.Exists(tempPngFile), Is.True);
        }
        finally
        {
            if (File.Exists(tempPngFile))
                File.Delete(tempPngFile);
        }
    }

    [Test]
    public void ConfigurationValidation_WithComplexConfig_ValidatesSuccessfully()
    {
        var config = new AngularVisualizationConfig
        {
            LeftTurnAngle = -45.0f,
            RightTurnAngle = 90.0f,
            ThicknessImpact = 3.0f,
            ColorImpact = 2.5f,
            MaxLineWidth = 20.0f,
            RenderMostTraversedPaths = 100,
            DrawingOrder = DrawingOrder.LeastToMostTraversed
        };

        Assert.DoesNotThrow(() => config.Validate());
    }

    [Test]
    public void JsonSerialization_WithLargeNumbers_RoundTripsCorrectly()
    {
        var converter = new BigIntegerConverter();
        var options = new System.Text.Json.JsonSerializerOptions();
        options.Converters.Add(converter);

        var largeNumbers = new[]
        {
            BigInteger.Parse("999999999999999999999999"),
            new BigInteger(long.MaxValue) * 1000,
            BigInteger.Parse("-123456789012345678901234567890")
        };

        foreach (var number in largeNumbers)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(number, options);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<BigInteger>(json, options);

            Assert.That(deserialized, Is.EqualTo(number));
        }
    }

    [Test]
    public void MultipleExportFormats_WithSameData_ProduceValidFiles()
    {
        var generator = new SequenceGenerator();
        var visualizer = new TreeMapVisualizer();
        var svgExporter = new SvgExporter();

        var tempSvgFile = Path.GetTempFileName() + ".svg";
        var tempPngFile = Path.GetTempFileName() + ".png";
        var tempAngularFile = Path.GetTempFileName() + ".png";

        try
        {
            // Generate test data
            for (BigInteger i = 2; i <= 6; i++)
            {
                generator.Add(i);
            }

            var layout = visualizer.CalculateLayout(generator.Root);

            // Test SVG export
            var svg = svgExporter.ExportToSvg(layout);
            File.WriteAllText(tempSvgFile, svg);

            // Test traditional PNG export
            visualizer.ExportToPng(generator.Root, tempPngFile);

            // Test angular PNG export
            var angularConfig = new AngularVisualizationConfig();
            visualizer.ExportToAngularPng(generator.Root, tempAngularFile, angularConfig);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(tempSvgFile), Is.True);
                Assert.That(File.Exists(tempPngFile), Is.True);
                Assert.That(File.Exists(tempAngularFile), Is.True);

                Assert.That(new FileInfo(tempSvgFile).Length, Is.GreaterThan(0));
                Assert.That(new FileInfo(tempPngFile).Length, Is.GreaterThan(0));
                Assert.That(new FileInfo(tempAngularFile).Length, Is.GreaterThan(0));
            });
        }
        finally
        {
            if (File.Exists(tempSvgFile)) File.Delete(tempSvgFile);
            if (File.Exists(tempPngFile)) File.Delete(tempPngFile);
            if (File.Exists(tempAngularFile)) File.Delete(tempAngularFile);
        }
    }

    [Test]
    public void AngularLayout_WithDifferentConfigurations_ProducesVariedResults()
    {
        var generator = new SequenceGenerator();
        var calculator = new AngularTreeLayoutCalculator();

        for (BigInteger i = 2; i <= 8; i++)
        {
            generator.Add(i);
        }

        var defaultConfig = new AngularVisualizationConfig();
        var customConfig = new AngularVisualizationConfig
        {
            LeftTurnAngle = -30.0f,
            RightTurnAngle = 45.0f
        };

        var defaultLayout = calculator.CalculateLayout(generator.Root, defaultConfig);
        var customLayout = calculator.CalculateLayout(generator.Root, customConfig);

        // Layouts should be different with different angle configurations
        Assert.That(defaultLayout.Children.Count, Is.EqualTo(customLayout.Children.Count));

        // At least some nodes should have different positions due to different angles
        var positionsDiffer = false;
        var defaultNodes = GetAllNodes(defaultLayout);
        var customNodes = GetAllNodes(customLayout);

        for (Int32 i = 0; i < Math.Min(defaultNodes.Count, customNodes.Count); i++)
        {
            if (Math.Abs(defaultNodes[i].X - customNodes[i].X) > 0.1f ||
                Math.Abs(defaultNodes[i].Y - customNodes[i].Y) > 0.1f)
            {
                positionsDiffer = true;
                break;
            }
        }

        Assert.That(positionsDiffer, Is.True, "Different angle configurations should produce different node positions");
    }

    private System.Collections.Generic.List<LayoutNode> GetAllNodes(LayoutNode root)
    {
        var nodes = new System.Collections.Generic.List<LayoutNode> { root };
        var queue = new System.Collections.Generic.Queue<LayoutNode>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in current.Children)
            {
                nodes.Add(child);
                queue.Enqueue(child);
            }
        }

        return nodes;
    }
}
