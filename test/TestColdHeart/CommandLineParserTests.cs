using System;
using ColdHeart;
using libColdHeart;

namespace TestColdHeart;

public class CommandLineOptionsTests
{
    [Test]
    public void DefaultConstructor_SetsDefaultLoadFile()
    {
        var options = new CommandLineOptions();

        Assert.That(options.LoadFile, Is.Null);
    }

    [Test]
    public void DefaultConstructor_SetsDefaultSaveFile()
    {
        var options = new CommandLineOptions();

        Assert.That(options.SaveFile, Is.Null);
    }

    [Test]
    public void DefaultConstructor_SetsDefaultSvgFile()
    {
        var options = new CommandLineOptions();

        Assert.That(options.SvgFile, Is.Null);
    }

    [Test]
    public void DefaultConstructor_SetsDefaultPngFile()
    {
        var options = new CommandLineOptions();

        Assert.That(options.PngFile, Is.Null);
    }

    [Test]
    public void DefaultConstructor_SetsDefaultAngularPngFile()
    {
        var options = new CommandLineOptions();

        Assert.That(options.AngularPngFile, Is.Null);
    }

    [Test]
    public void DefaultConstructor_SetsDefaultAngularNodeStyle()
    {
        var options = new CommandLineOptions();

        Assert.That(options.AngularNodeStyle, Is.EqualTo(NodeStyle.Circle));
    }

    [Test]
    public void DefaultConstructor_SetsDefaultMaxSequences()
    {
        var options = new CommandLineOptions();

        Assert.That(options.MaxSequences, Is.EqualTo(1000));
    }

    [Test]
    public void DefaultConstructor_SetsDefaultShowHelp()
    {
        var options = new CommandLineOptions();

        Assert.That(options.ShowHelp, Is.False);
    }

    [Test]
    public void DefaultConstructor_CreatesAngularConfig()
    {
        var options = new CommandLineOptions();

        Assert.That(options.AngularConfig, Is.Not.Null);
    }
}

public class CommandLineParserTests
{
    private CommandLineParser _parser;

    [SetUp]
    public void Setup()
    {
        _parser = new CommandLineParser();
    }

    [Test]
    public void Parse_WithEmptyArgs_ReturnsDefaultOptions()
    {
        var args = Array.Empty<String>();

        var options = _parser.Parse(args);

        Assert.That(options.LoadFile, Is.Null);
    }

    [Test]
    public void Parse_WithLoadArgument_SetsLoadFile()
    {
        var args = new[] { "--load", "test.json" };

        var options = _parser.Parse(args);

        Assert.That(options.LoadFile, Is.EqualTo("test.json"));
    }

    [Test]
    public void Parse_WithSaveArgument_SetsSaveFile()
    {
        var args = new[] { "--save", "output.json" };

        var options = _parser.Parse(args);

        Assert.That(options.SaveFile, Is.EqualTo("output.json"));
    }

    [Test]
    public void Parse_WithSvgArgument_SetsSvgFile()
    {
        var args = new[] { "--svg", "tree.svg" };

        var options = _parser.Parse(args);

        Assert.That(options.SvgFile, Is.EqualTo("tree.svg"));
    }

    [Test]
    public void Parse_WithPngArgument_SetsPngFile()
    {
        var args = new[] { "--png", "tree.png" };

        var options = _parser.Parse(args);

        Assert.That(options.PngFile, Is.EqualTo("tree.png"));
    }

    [Test]
    public void Parse_WithAngularPngArgument_SetsAngularPngFile()
    {
        var args = new[] { "--png-angular", "angular.png" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularPngFile, Is.EqualTo("angular.png"));
    }

    [Test]
    public void Parse_WithHelpArgument_SetsShowHelp()
    {
        var args = new[] { "--help" };

        var options = _parser.Parse(args);

        Assert.That(options.ShowHelp, Is.True);
    }

    [Test]
    public void Parse_WithShortHelpArgument_SetsShowHelp()
    {
        var args = new[] { "-h" };

        var options = _parser.Parse(args);

        Assert.That(options.ShowHelp, Is.True);
    }

    [Test]
    public void Parse_WithSequencesArgument_SetsMaxSequences()
    {
        var args = new[] { "--sequences", "2000" };

        var options = _parser.Parse(args);

        Assert.That(options.MaxSequences, Is.EqualTo(2000));
    }

    [Test]
    public void Parse_WithAngularLeftTurnArgument_SetsLeftTurnAngle()
    {
        var args = new[] { "--angular-left-turn", "-15.5" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.LeftTurnAngle, Is.EqualTo(-15.5f));
    }

    [Test]
    public void Parse_WithAngularRightTurnArgument_SetsRightTurnAngle()
    {
        var args = new[] { "--angular-right-turn", "25.0" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.RightTurnAngle, Is.EqualTo(25.0f));
    }

    [Test]
    public void Parse_WithAngularThicknessImpactArgument_SetsThicknessImpact()
    {
        var args = new[] { "--angular-thickness-impact", "2.5" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.ThicknessImpact, Is.EqualTo(2.5f));
    }

    [Test]
    public void Parse_WithAngularColorImpactArgument_SetsColorImpact()
    {
        var args = new[] { "--angular-color-impact", "1.8" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.ColorImpact, Is.EqualTo(1.8f));
    }

    [Test]
    public void Parse_WithAngularMaxLineWidthArgument_SetsMaxLineWidth()
    {
        var args = new[] { "--angular-max-line-width", "12.0" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.MaxLineWidth, Is.EqualTo(12.0f));
    }

    [Test]
    public void Parse_WithAngularRenderLongestArgument_SetsRenderLongestPaths()
    {
        var args = new[] { "--angular-render-longest", "50" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.RenderLongestPaths, Is.EqualTo(50));
    }

    [Test]
    public void Parse_WithAngularRenderMostTraversedArgument_SetsRenderMostTraversedPaths()
    {
        var args = new[] { "--angular-render-most-traversed", "100" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.RenderMostTraversedPaths, Is.EqualTo(100));
    }

    [Test]
    public void Parse_WithAngularRenderLeastTraversedArgument_SetsRenderLeastTraversedPaths()
    {
        var args = new[] { "--angular-render-least-traversed", "25" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.RenderLeastTraversedPaths, Is.EqualTo(25));
    }

    [Test]
    public void Parse_WithAngularRenderRandomArgument_SetsRenderRandomPaths()
    {
        var args = new[] { "--angular-render-random", "75" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.RenderRandomPaths, Is.EqualTo(75));
    }

    [Test]
    public void Parse_WithAngularNodeStyleCircle_SetsNodeStyleCircle()
    {
        var args = new[] { "--angular-node-style", "circle" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularNodeStyle, Is.EqualTo(NodeStyle.Circle));
    }

    [Test]
    public void Parse_WithAngularNodeStyleRectangle_SetsNodeStyleRectangle()
    {
        var args = new[] { "--angular-node-style", "rectangle" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularNodeStyle, Is.EqualTo(NodeStyle.Rectangle));
    }

    [Test]
    public void Parse_WithAngularDrawOrderTree_SetsDrawingOrderTree()
    {
        var args = new[] { "--angular-draw-order", "tree" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.DrawingOrder, Is.EqualTo(DrawingOrder.TreeOrder));
    }

    [Test]
    public void Parse_WithAngularDrawOrderLeastToMost_SetsDrawingOrderLeastToMost()
    {
        var args = new[] { "--angular-draw-order", "least-to-most-traversed" };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.DrawingOrder, Is.EqualTo(DrawingOrder.LeastToMostTraversed));
    }

    [Test]
    public void Parse_WithLoadArgumentMissingValue_ThrowsArgumentException()
    {
        var args = new[] { "--load" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("--load requires a filename"));
    }

    [Test]
    public void Parse_WithSaveArgumentMissingValue_ThrowsArgumentException()
    {
        var args = new[] { "--save" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("--save requires a filename"));
    }

    [Test]
    public void Parse_WithSvgArgumentMissingValue_ThrowsArgumentException()
    {
        var args = new[] { "--svg" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("--svg requires a filename"));
    }

    [Test]
    public void Parse_WithPngArgumentMissingValue_ThrowsArgumentException()
    {
        var args = new[] { "--png" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("--png requires a filename"));
    }

    [Test]
    public void Parse_WithAngularPngArgumentMissingValue_ThrowsArgumentException()
    {
        var args = new[] { "--png-angular" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("--png-angular requires a filename"));
    }

    [Test]
    public void Parse_WithSequencesArgumentMissingValue_ThrowsArgumentException()
    {
        var args = new[] { "--sequences" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("--sequences requires a positive integer"));
    }

    [Test]
    public void Parse_WithSequencesArgumentInvalidValue_ThrowsArgumentException()
    {
        var args = new[] { "--sequences", "invalid" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid number of sequences"));
    }

    [Test]
    public void Parse_WithSequencesArgumentNegativeValue_ThrowsArgumentException()
    {
        var args = new[] { "--sequences", "-10" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid number of sequences"));
    }

    [Test]
    public void Parse_WithSequencesArgumentZeroValue_ThrowsArgumentException()
    {
        var args = new[] { "--sequences", "0" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid number of sequences"));
    }

    [Test]
    public void Parse_WithAngularLeftTurnArgumentInvalidValue_ThrowsArgumentException()
    {
        var args = new[] { "--angular-left-turn", "not-a-number" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid left turn angle"));
    }

    [Test]
    public void Parse_WithAngularMaxLineWidthArgumentInvalidValue_ThrowsArgumentException()
    {
        var args = new[] { "--angular-max-line-width", "invalid" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid maximum line width"));
    }

    [Test]
    public void Parse_WithAngularMaxLineWidthArgumentNegativeValue_ThrowsArgumentException()
    {
        var args = new[] { "--angular-max-line-width", "-5.0" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid maximum line width"));
    }

    [Test]
    public void Parse_WithAngularMaxLineWidthArgumentZeroValue_ThrowsArgumentException()
    {
        var args = new[] { "--angular-max-line-width", "0.0" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid maximum line width"));
    }

    [Test]
    public void Parse_WithAngularNodeStyleInvalidValue_ThrowsArgumentException()
    {
        var args = new[] { "--angular-node-style", "invalid" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid node style"));
    }

    [Test]
    public void Parse_WithAngularDrawOrderInvalidValue_ThrowsArgumentException()
    {
        var args = new[] { "--angular-draw-order", "invalid" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Invalid drawing order"));
    }

    [Test]
    public void Parse_WithUnknownArgument_ThrowsArgumentException()
    {
        var args = new[] { "--unknown-argument" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("Unknown argument"));
    }

    [TestCaseSource(nameof(CaseInsensitiveTestCases))]
    public void Parse_WithCaseInsensitiveValues_ParsesCorrectly(String[] args, Object expectedResult)
    {
        var options = _parser.Parse(args);

        if (args[0] == "--angular-node-style")
        {
            Assert.That(options.AngularNodeStyle, Is.EqualTo(expectedResult));
        }
        else if (args[0] == "--angular-draw-order")
        {
            Assert.That(options.AngularConfig.DrawingOrder, Is.EqualTo(expectedResult));
        }
    }

    private static System.Collections.IEnumerable CaseInsensitiveTestCases()
    {
        yield return new TestCaseData(new[] { "--angular-node-style", "CIRCLE" }, NodeStyle.Circle);
        yield return new TestCaseData(new[] { "--angular-node-style", "Circle" }, NodeStyle.Circle);
        yield return new TestCaseData(new[] { "--angular-node-style", "RECTANGLE" }, NodeStyle.Rectangle);
        yield return new TestCaseData(new[] { "--angular-node-style", "Rectangle" }, NodeStyle.Rectangle);
        yield return new TestCaseData(new[] { "--angular-draw-order", "TREE" }, DrawingOrder.TreeOrder);
        yield return new TestCaseData(new[] { "--angular-draw-order", "Tree" }, DrawingOrder.TreeOrder);
        yield return new TestCaseData(new[] { "--angular-draw-order", "LEAST-TO-MOST-TRAVERSED" }, DrawingOrder.LeastToMostTraversed);
    }

    [Test]
    public void Parse_WithMultipleArguments_SetsAllValues()
    {
        var args = new[] {
            "--load", "input.json",
            "--save", "output.json",
            "--svg", "tree.svg",
            "--png", "tree.png",
            "--png-angular", "angular.png",
            "--sequences", "500"
        };

        var options = _parser.Parse(args);

        Assert.Multiple(() =>
        {
            Assert.That(options.LoadFile, Is.EqualTo("input.json"));
            Assert.That(options.SaveFile, Is.EqualTo("output.json"));
            Assert.That(options.SvgFile, Is.EqualTo("tree.svg"));
            Assert.That(options.PngFile, Is.EqualTo("tree.png"));
            Assert.That(options.AngularPngFile, Is.EqualTo("angular.png"));
            Assert.That(options.MaxSequences, Is.EqualTo(500));
        });
    }

    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("abc")]
    public void Parse_WithInvalidAngularRenderLeastTraversedValue_ThrowsArgumentException(string invalidValue)
    {
        var args = new[] { "--angular-render-least-traversed", invalidValue };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("least traversed paths count"));
    }

    [Test]
    public void Parse_WithMissingAngularRenderLeastTraversedValue_ThrowsArgumentException()
    {
        var args = new[] { "--angular-render-least-traversed" };

        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(args));
        Assert.That(ex.Message, Does.Contain("--angular-render-least-traversed requires a positive integer"));
    }

    [TestCase(1)]
    [TestCase(100)]
    [TestCase(50000)]
    public void Parse_WithValidAngularRenderLeastTraversedValues_SetsCorrectValue(int expectedValue)
    {
        var args = new[] { "--angular-render-least-traversed", expectedValue.ToString() };

        var options = _parser.Parse(args);

        Assert.That(options.AngularConfig.RenderLeastTraversedPaths, Is.EqualTo(expectedValue));
    }
}
