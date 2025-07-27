using System;
using ColdHeart;
using libColdHeart;

namespace TestColdHeart;

public class HelpTextConsistencyTests
{
    [Test]
    public void HelpText_DefaultValues_MatchActualDefaults()
    {
        var defaultOptions = new CommandLineOptions();
        var defaultConfig = new AngularVisualizationConfig();

        // Verify that default values in help text match actual defaults

        Assert.Multiple(() =>
        {
            // Verify that default values in help text match actual defaults
            Assert.That(defaultConfig.LeftTurnAngle, Is.EqualTo(-8.65f),
                "Help text mentions default -8.65 for left turn angle");
            Assert.That(defaultConfig.RightTurnAngle, Is.EqualTo(16.0f),
                "Help text mentions default 16.0 for right turn angle");
            Assert.That(defaultConfig.ThicknessImpact, Is.EqualTo(1.0f),
                "Help text mentions default 1.0 for thickness impact");
            Assert.That(defaultConfig.ColorImpact, Is.EqualTo(1.0f),
                "Help text mentions default 1.0 for color impact");
            Assert.That(defaultConfig.MaxLineWidth, Is.EqualTo(8.0f),
                "Help text mentions default 8.0 for max line width");
            Assert.That(defaultOptions.MaxSequences, Is.EqualTo(1000),
                "Help text mentions default 1000 for sequences");
            Assert.That(defaultOptions.AngularNodeStyle, Is.EqualTo(NodeStyle.Circle),
                "Help text mentions default circle for node style");
        });
    }

    [Test]
    public void HelpText_EnumValues_MatchActualEnums()
    {
        // Verify that enum values mentioned in help text actually exist
        Assert.Multiple(() =>
        {
            Assert.That(Enum.IsDefined(typeof(NodeStyle), NodeStyle.Circle), Is.True,
                "Circle node style should be defined");
            Assert.That(Enum.IsDefined(typeof(NodeStyle), NodeStyle.Rectangle), Is.True,
                "Rectangle node style should be defined");
            Assert.That(Enum.IsDefined(typeof(DrawingOrder), DrawingOrder.TreeOrder), Is.True,
                "TreeOrder drawing order should be defined");
            Assert.That(Enum.IsDefined(typeof(DrawingOrder), DrawingOrder.LeastToMostTraversed), Is.True,
                "LeastToMostTraversed drawing order should be defined");
        });
    }

    [Test]
    public void HelpText_ParseableValues_MatchActualParsing()
    {
        var parser = new CommandLineParser();

        // Test that values mentioned in help text can actually be parsed
        Assert.Multiple(() =>
        {
            // Test drawing order parsing
            var treeOrderOptions = parser.Parse(new[] { "--angular-draw-order", "tree" });
            Assert.That(treeOrderOptions.AngularConfig.DrawingOrder, Is.EqualTo(DrawingOrder.TreeOrder),
                "Help text mentions 'tree' which should parse to TreeOrder");

            var traversalOrderOptions = parser.Parse(new[] { "--angular-draw-order", "least-to-most-traversed" });
            Assert.That(traversalOrderOptions.AngularConfig.DrawingOrder, Is.EqualTo(DrawingOrder.LeastToMostTraversed),
                "Help text mentions 'least-to-most-traversed' which should parse correctly");

            // Test node style parsing
            var circleOptions = parser.Parse(new[] { "--angular-node-style", "circle" });
            Assert.That(circleOptions.AngularNodeStyle, Is.EqualTo(NodeStyle.Circle),
                "Help text mentions 'circle' which should parse correctly");

            var rectangleOptions = parser.Parse(new[] { "--angular-node-style", "rectangle" });
            Assert.That(rectangleOptions.AngularNodeStyle, Is.EqualTo(NodeStyle.Rectangle),
                "Help text mentions 'rectangle' which should parse correctly");
        });
    }
}

