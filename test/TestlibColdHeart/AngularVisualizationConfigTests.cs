using System;
using libColdHeart;

namespace TestlibColdHeart;

public class AngularVisualizationConfigTests
{
    [Test]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.LeftTurnAngle, Is.EqualTo(-8.65f));
    }

    [Test]
    public void DefaultConstructor_SetsDefaultRightTurnAngle()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.RightTurnAngle, Is.EqualTo(16.0f));
    }

    [Test]
    public void DefaultConstructor_SetsDefaultThicknessImpact()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.ThicknessImpact, Is.EqualTo(1.0f));
    }

    [Test]
    public void DefaultConstructor_SetsDefaultColorImpact()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.ColorImpact, Is.EqualTo(1.0f));
    }

    [Test]
    public void DefaultConstructor_SetsDefaultMaxLineWidth()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.MaxLineWidth, Is.EqualTo(8.0f));
    }

    [Test]
    public void DefaultConstructor_SetsDefaultDrawingOrder()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.DrawingOrder, Is.EqualTo(DrawingOrder.TreeOrder));
    }

    [Test]
    public void DefaultConstructor_SetsRenderLongestPathsToNull()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.RenderLongestPaths, Is.Null);
    }

    [Test]
    public void DefaultConstructor_SetsRenderMostTraversedPathsToNull()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.RenderMostTraversedPaths, Is.Null);
    }

    [Test]
    public void DefaultConstructor_SetsRenderRandomPathsToNull()
    {
        var config = new AngularVisualizationConfig();

        Assert.That(config.RenderRandomPaths, Is.Null);
    }

    [Test]
    public void ParameterizedConstructor_SetsLeftTurnAngle()
    {
        var config = new AngularVisualizationConfig(-15.0f, 20.0f, 2.0f, 1.5f);

        Assert.That(config.LeftTurnAngle, Is.EqualTo(-15.0f));
    }

    [Test]
    public void ParameterizedConstructor_SetsRightTurnAngle()
    {
        var config = new AngularVisualizationConfig(-15.0f, 20.0f, 2.0f, 1.5f);

        Assert.That(config.RightTurnAngle, Is.EqualTo(20.0f));
    }

    [Test]
    public void ParameterizedConstructor_SetsThicknessImpact()
    {
        var config = new AngularVisualizationConfig(-15.0f, 20.0f, 2.0f, 1.5f);

        Assert.That(config.ThicknessImpact, Is.EqualTo(2.0f));
    }

    [Test]
    public void ParameterizedConstructor_SetsColorImpact()
    {
        var config = new AngularVisualizationConfig(-15.0f, 20.0f, 2.0f, 1.5f);

        Assert.That(config.ColorImpact, Is.EqualTo(1.5f));
    }

    [Test]
    public void Validate_WithValidDefaults_DoesNotThrow()
    {
        var config = new AngularVisualizationConfig();

        Assert.DoesNotThrow(() => config.Validate());
    }

    [Test]
    public void Validate_WithMultiplePathFilters_ThrowsInvalidOperationException()
    {
        var config = new AngularVisualizationConfig
        {
            RenderLongestPaths = 10,
            RenderMostTraversedPaths = 20
        };

        var ex = Assert.Throws<InvalidOperationException>(() => config.Validate());
        Assert.That(ex.Message, Is.EqualTo("Only one path filtering option can be specified at a time."));
    }

    [Test]
    public void Validate_WithAllThreePathFilters_ThrowsInvalidOperationException()
    {
        var config = new AngularVisualizationConfig
        {
            RenderLongestPaths = 10,
            RenderMostTraversedPaths = 20,
            RenderRandomPaths = 30
        };

        var ex = Assert.Throws<InvalidOperationException>(() => config.Validate());
        Assert.That(ex.Message, Is.EqualTo("Only one path filtering option can be specified at a time."));
    }

    [Test]
    public void Validate_WithNegativeThicknessImpact_ThrowsArgumentOutOfRangeException()
    {
        var config = new AngularVisualizationConfig
        {
            ThicknessImpact = -1.0f
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("ThicknessImpact"));
    }

    [Test]
    public void Validate_WithZeroColorImpact_ThrowsArgumentOutOfRangeException()
    {
        var config = new AngularVisualizationConfig
        {
            ColorImpact = 0.0f
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("ColorImpact"));
    }

    [Test]
    public void Validate_WithNegativeColorImpact_ThrowsArgumentOutOfRangeException()
    {
        var config = new AngularVisualizationConfig
        {
            ColorImpact = -1.0f
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("ColorImpact"));
    }

    [Test]
    public void Validate_WithZeroMaxLineWidth_ThrowsArgumentOutOfRangeException()
    {
        var config = new AngularVisualizationConfig
        {
            MaxLineWidth = 0.0f
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("MaxLineWidth"));
    }

    [Test]
    public void Validate_WithNegativeMaxLineWidth_ThrowsArgumentOutOfRangeException()
    {
        var config = new AngularVisualizationConfig
        {
            MaxLineWidth = -5.0f
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("MaxLineWidth"));
    }

    [TestCaseSource(nameof(SinglePathFilterTestCases))]
    public void Validate_WithSinglePathFilter_DoesNotThrow(Int32? longestPaths, Int32? mostTraversedPaths, Int32? randomPaths)
    {
        var config = new AngularVisualizationConfig
        {
            RenderLongestPaths = longestPaths,
            RenderMostTraversedPaths = mostTraversedPaths,
            RenderRandomPaths = randomPaths
        };

        Assert.DoesNotThrow(() => config.Validate());
    }

    private static System.Collections.IEnumerable SinglePathFilterTestCases()
    {
        yield return new TestCaseData(10, null, null);
        yield return new TestCaseData(null, 20, null);
        yield return new TestCaseData(null, null, 30);
    }

    [TestCaseSource(nameof(ValidBoundaryTestCases))]
    public void Validate_WithBoundaryValues_DoesNotThrow(Single thicknessImpact, Single colorImpact, Single maxLineWidth)
    {
        var config = new AngularVisualizationConfig
        {
            ThicknessImpact = thicknessImpact,
            ColorImpact = colorImpact,
            MaxLineWidth = maxLineWidth
        };

        Assert.DoesNotThrow(() => config.Validate());
    }

    private static System.Collections.IEnumerable ValidBoundaryTestCases()
    {
        yield return new TestCaseData(0.0f, 0.001f, 0.001f); // Zero thickness impact allowed
        yield return new TestCaseData(100.0f, 10.0f, 50.0f); // High values allowed
        yield return new TestCaseData(0.0f, Single.MaxValue, Single.MaxValue); // Maximum values
    }
}
