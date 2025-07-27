using System;

namespace libColdHeart;

public enum DrawingOrder
{
    TreeOrder,
    LeastToMostTraversed
}

public class AngularVisualizationConfig
{
    public Single LeftTurnAngle { get; set; } = -8.65f;
    public Single RightTurnAngle { get; set; } = 16.0f;
    public Single ThicknessImpact { get; set; } = 1.0f;
    public Single ColorImpact { get; set; } = 1.0f;
    public Single MaxLineWidth { get; set; } = 8.0f;
    public DrawingOrder DrawingOrder { get; set; } = DrawingOrder.TreeOrder;

    // Path filtering options - only one should be set
    public Int32? RenderLongestPaths { get; set; }
    public Int32? RenderMostTraversedPaths { get; set; }
    public Int32? RenderRandomPaths { get; set; }

    public AngularVisualizationConfig()
    {
    }

    public AngularVisualizationConfig(Single leftTurnAngle, Single rightTurnAngle, Single thicknessImpact, Single colorImpact)
    {
        LeftTurnAngle = leftTurnAngle;
        RightTurnAngle = rightTurnAngle;
        ThicknessImpact = thicknessImpact;
        ColorImpact = colorImpact;
    }

    /// <summary>
    /// Validates that only one path filtering option is set
    /// </summary>
    public void Validate()
    {
        var filterCount = 0;
        if (RenderLongestPaths.HasValue) filterCount++;
        if (RenderMostTraversedPaths.HasValue) filterCount++;
        if (RenderRandomPaths.HasValue) filterCount++;

        if (filterCount > 1)
        {
            throw new InvalidOperationException("Only one path filtering option can be specified at a time.");
        }

        if (ThicknessImpact < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ThicknessImpact), "Thickness impact must be non-negative.");
        }

        if (ColorImpact <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ColorImpact), "Color impact must be positive.");
        }

        if (MaxLineWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxLineWidth), "Maximum line width must be positive.");
        }
    }
}
