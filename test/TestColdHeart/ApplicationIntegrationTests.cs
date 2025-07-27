using System;
using ColdHeart;
using libColdHeart;

namespace TestColdHeart;

public class ApplicationIntegrationTests
{
    [Test]
    public void CommandLineParsing_WithComplexConfiguration_ParsesCorrectly()
    {
        var parser = new CommandLineParser();
        var args = new[]
        {
            "--png-angular", "output.png",
            "--angular-left-turn", "-12.5",
            "--angular-right-turn", "18.0",
            "--angular-thickness-impact", "2.0",
            "--angular-color-impact", "1.5",
            "--angular-max-line-width", "10.0",
            "--angular-render-longest", "25",
            "--angular-node-style", "rectangle",
            "--angular-draw-order", "least-to-most-traversed",
            "--sequences", "2000"
        };

        var options = parser.Parse(args);

        Assert.Multiple(() =>
        {
            Assert.That(options.AngularPngFile, Is.EqualTo("output.png"));
            Assert.That(options.AngularConfig.LeftTurnAngle, Is.EqualTo(-12.5f));
            Assert.That(options.AngularConfig.RightTurnAngle, Is.EqualTo(18.0f));
            Assert.That(options.AngularConfig.ThicknessImpact, Is.EqualTo(2.0f));
            Assert.That(options.AngularConfig.ColorImpact, Is.EqualTo(1.5f));
            Assert.That(options.AngularConfig.MaxLineWidth, Is.EqualTo(10.0f));
            Assert.That(options.AngularConfig.RenderLongestPaths, Is.EqualTo(25));
            Assert.That(options.AngularNodeStyle, Is.EqualTo(NodeStyle.Rectangle));
            Assert.That(options.AngularConfig.DrawingOrder, Is.EqualTo(DrawingOrder.LeastToMostTraversed));
            Assert.That(options.MaxSequences, Is.EqualTo(2000));
        });
    }
}
