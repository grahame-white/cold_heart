using System;
using System.Numerics;
using libColdHeart;

namespace TestlibColdHeart;

public class TreeNodeTests
{
    [Test]
    public void Constructor_WithBigIntegerValue_SetsValue()
    {
        var value = new BigInteger(42);

        var node = new TreeNode(value);

        Assert.That(node.Value, Is.EqualTo(value));
    }

    [Test]
    public void Constructor_WithBigIntegerValue_InitializesChildrenAsNull()
    {
        var value = new BigInteger(42);

        var node = new TreeNode(value);

        Assert.That(node.LeftChild, Is.Null);
    }

    [Test]
    public void Constructor_WithBigIntegerValue_InitializesRightChildAsNull()
    {
        var value = new BigInteger(42);

        var node = new TreeNode(value);

        Assert.That(node.RightChild, Is.Null);
    }

    [Test]
    public void DefaultConstructor_InitializesValueToZero()
    {
        var node = new TreeNode();

        Assert.That(node.Value, Is.EqualTo(BigInteger.Zero));
    }

    [Test]
    public void DefaultConstructor_InitializesLeftChildAsNull()
    {
        var node = new TreeNode();

        Assert.That(node.LeftChild, Is.Null);
    }

    [Test]
    public void DefaultConstructor_InitializesRightChildAsNull()
    {
        var node = new TreeNode();

        Assert.That(node.RightChild, Is.Null);
    }

    [Test]
    public void Constructor_WithLargeBigInteger_HandlesLargeValues()
    {
        var largeValue = BigInteger.Parse("123456789012345678901234567890");

        var node = new TreeNode(largeValue);

        Assert.That(node.Value, Is.EqualTo(largeValue));
    }

    [Test]
    public void Constructor_WithNegativeBigInteger_HandlesNegativeValues()
    {
        var negativeValue = new BigInteger(-999);

        var node = new TreeNode(negativeValue);

        Assert.That(node.Value, Is.EqualTo(negativeValue));
    }

    [Test]
    public void Constructor_WithMaxLongValue_HandlesMaxValue()
    {
        var maxValue = new BigInteger(long.MaxValue);

        var node = new TreeNode(maxValue);

        Assert.That(node.Value, Is.EqualTo(maxValue));
    }

    [Test]
    public void Constructor_WithMinLongValue_HandlesMinValue()
    {
        var minValue = new BigInteger(long.MinValue);

        var node = new TreeNode(minValue);

        Assert.That(node.Value, Is.EqualTo(minValue));
    }

    [TestCaseSource(nameof(VariousBigIntegerValues))]
    public void Constructor_WithVariousValues_SetsValueCorrectly(BigInteger value)
    {
        var node = new TreeNode(value);

        Assert.That(node.Value, Is.EqualTo(value));
    }

    private static System.Collections.IEnumerable VariousBigIntegerValues()
    {
        yield return new TestCaseData(BigInteger.Zero);
        yield return new TestCaseData(BigInteger.One);
        yield return new TestCaseData(new BigInteger(int.MaxValue));
        yield return new TestCaseData(new BigInteger(int.MinValue));
        yield return new TestCaseData(BigInteger.Parse("999999999999999999999999999999"));
        yield return new TestCaseData(BigInteger.Parse("-999999999999999999999999999999"));
    }
}

public class LayoutNodeTests
{
    [Test]
    public void Constructor_WithBigIntegerValue_SetsValue()
    {
        var value = new BigInteger(123);

        var node = new LayoutNode(value);

        Assert.That(node.Value, Is.EqualTo(value));
    }

    [Test]
    public void Constructor_WithBigIntegerValue_InitializesDefaultPosition()
    {
        var value = new BigInteger(123);

        var node = new LayoutNode(value);

        Assert.That(node.X, Is.EqualTo(0.0f));
    }

    [Test]
    public void Constructor_WithBigIntegerValue_InitializesDefaultY()
    {
        var value = new BigInteger(123);

        var node = new LayoutNode(value);

        Assert.That(node.Y, Is.EqualTo(0.0f));
    }

    [Test]
    public void Constructor_WithBigIntegerValue_InitializesDefaultWidth()
    {
        var value = new BigInteger(123);

        var node = new LayoutNode(value);

        Assert.That(node.Width, Is.EqualTo(60.0f));
    }

    [Test]
    public void Constructor_WithBigIntegerValue_InitializesDefaultHeight()
    {
        var value = new BigInteger(123);

        var node = new LayoutNode(value);

        Assert.That(node.Height, Is.EqualTo(30.0f));
    }

    [Test]
    public void Constructor_WithBigIntegerValue_InitializesEmptyChildren()
    {
        var value = new BigInteger(123);

        var node = new LayoutNode(value);

        Assert.That(node.Children, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithBigIntegerValue_ChildrenListIsEmpty()
    {
        var value = new BigInteger(123);

        var node = new LayoutNode(value);

        Assert.That(node.Children.Count, Is.EqualTo(0));
    }

    [Test]
    public void Properties_CanBeSetAndRetrieved()
    {
        var node = new LayoutNode(new BigInteger(456))
        {
            X = 10.5f,
            Y = 20.7f,
            Width = 100.0f,
            Height = 50.0f
        };

        Assert.Multiple(() =>
        {
            Assert.That(node.X, Is.EqualTo(10.5f));
            Assert.That(node.Y, Is.EqualTo(20.7f));
            Assert.That(node.Width, Is.EqualTo(100.0f));
            Assert.That(node.Height, Is.EqualTo(50.0f));
        });
    }

    [Test]
    public void Children_CanAddChildNodes()
    {
        var parent = new LayoutNode(new BigInteger(1));
        var child = new LayoutNode(new BigInteger(2));

        parent.Children.Add(child);

        Assert.That(parent.Children.Count, Is.EqualTo(1));
    }

    [Test]
    public void Children_AddedChildHasCorrectValue()
    {
        var parent = new LayoutNode(new BigInteger(1));
        var child = new LayoutNode(new BigInteger(2));

        parent.Children.Add(child);

        Assert.That(parent.Children[0].Value, Is.EqualTo(new BigInteger(2)));
    }
}

public class EdgeCaseTests
{
    [Test]
    public void BigIntegerConverter_WithLargeDecimalString_HandlesCorrectly()
    {
        var converter = new BigIntegerConverter();
        var options = new System.Text.Json.JsonSerializerOptions();
        options.Converters.Add(converter);
        var veryLargeNumber = "12345678901234567890123456789012345678901234567890";
        var json = $"\"{veryLargeNumber}\"";

        var result = System.Text.Json.JsonSerializer.Deserialize<BigInteger>(json, options);

        Assert.That(result, Is.EqualTo(BigInteger.Parse(veryLargeNumber)));
    }

    [Test]
    public void AngularVisualizationConfig_WithExtremeValues_ValidatesCorrectly()
    {
        var config = new AngularVisualizationConfig
        {
            LeftTurnAngle = -180.0f,
            RightTurnAngle = 180.0f,
            ThicknessImpact = 100.0f,
            ColorImpact = 100.0f,
            MaxLineWidth = 1000.0f
        };

        Assert.DoesNotThrow(() => config.Validate());
    }

    [Test]
    public void AngularVisualizationConfig_WithZeroThicknessImpact_IsValid()
    {
        var config = new AngularVisualizationConfig
        {
            ThicknessImpact = 0.0f
        };

        Assert.DoesNotThrow(() => config.Validate());
    }

    [TestCaseSource(nameof(InvalidConfigTestCases))]
    public void AngularVisualizationConfig_WithInvalidValues_ThrowsExpectedException(
        Single thicknessImpact, Single colorImpact, Single maxLineWidth, Type expectedExceptionType)
    {
        var config = new AngularVisualizationConfig
        {
            ThicknessImpact = thicknessImpact,
            ColorImpact = colorImpact,
            MaxLineWidth = maxLineWidth
        };

        Assert.Throws(expectedExceptionType, () => config.Validate());
    }

    private static System.Collections.IEnumerable InvalidConfigTestCases()
    {
        yield return new TestCaseData(-0.1f, 1.0f, 8.0f, typeof(ArgumentOutOfRangeException)); // Negative thickness
        yield return new TestCaseData(1.0f, 0.0f, 8.0f, typeof(ArgumentOutOfRangeException)); // Zero color impact
        yield return new TestCaseData(1.0f, -1.0f, 8.0f, typeof(ArgumentOutOfRangeException)); // Negative color impact
        yield return new TestCaseData(1.0f, 1.0f, 0.0f, typeof(ArgumentOutOfRangeException)); // Zero line width
        yield return new TestCaseData(1.0f, 1.0f, -1.0f, typeof(ArgumentOutOfRangeException)); // Negative line width
    }
}

public class DrawingOrderTests
{
    [Test]
    public void DrawingOrder_TreeOrder_HasCorrectValue()
    {
        var order = DrawingOrder.TreeOrder;

        Assert.That((Int32)order, Is.EqualTo(0));
    }

    [Test]
    public void DrawingOrder_LeastToMostTraversed_HasCorrectValue()
    {
        var order = DrawingOrder.LeastToMostTraversed;

        Assert.That((Int32)order, Is.EqualTo(1));
    }
}

public class NodeStyleTests
{
    [Test]
    public void NodeStyle_Circle_HasCorrectValue()
    {
        var style = NodeStyle.Circle;

        Assert.That((Int32)style, Is.EqualTo(0));
    }

    [Test]
    public void NodeStyle_Rectangle_HasCorrectValue()
    {
        var style = NodeStyle.Rectangle;

        Assert.That((Int32)style, Is.EqualTo(1));
    }
}
