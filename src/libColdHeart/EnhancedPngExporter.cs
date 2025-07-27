using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using SkiaSharp;

namespace libColdHeart;

public class EnhancedPngExporter
{
    private const Single BaseLineStrokeWidth = 1.0f;
    private const Single MaxStrokeWidth = 8.0f;

    private static readonly SKColor BackgroundColor = SKColor.Parse("#ffffff"); // White background as required

    public void ExportToPng(LayoutNode rootLayout, TreeMetrics metrics, String filePath, Action<String>? progressCallback = null)
    {
        progressCallback?.Invoke("Calculating image bounds...");
        var bounds = CalculateBounds(rootLayout);

        // Add margins
        Single margin = 50.0f;
        Single originalWidth = bounds.Width + (2 * margin);
        Single originalHeight = bounds.Height + (2 * margin);

        // Check for reasonable image size limits and apply scaling if necessary
        const Int32 MAX_DIMENSION = 32767;
        const Int64 MAX_PIXELS = 100_000_000;

        Single scale = 1.0f;

        // Calculate scale to fit within dimension limits
        if (originalWidth > MAX_DIMENSION)
        {
            scale = Math.Min(scale, MAX_DIMENSION / originalWidth);
        }
        if (originalHeight > MAX_DIMENSION)
        {
            scale = Math.Min(scale, MAX_DIMENSION / originalHeight);
        }

        // Calculate scale to fit within pixel count limits
        Int64 totalPixels = (Int64)(originalWidth * originalHeight);
        if (totalPixels > MAX_PIXELS)
        {
            Single pixelScale = System.MathF.Sqrt((Single)MAX_PIXELS / totalPixels);
            scale = System.Math.Min(scale, pixelScale);
        }

        Int32 imageWidth = (Int32)(originalWidth * scale);
        Int32 imageHeight = (Int32)(originalHeight * scale);

        if (imageWidth <= 0 || imageHeight <= 0)
        {
            throw new InvalidOperationException($"Invalid image dimensions after scaling: {imageWidth}x{imageHeight}");
        }

        progressCallback?.Invoke("Creating image surface...");
        using var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));
        if (surface == null)
        {
            throw new InvalidOperationException($"Failed to create surface with dimensions {imageWidth}x{imageHeight}. This may be due to insufficient memory or graphics limitations.");
        }

        var canvas = surface.Canvas;

        // Clear background to white
        canvas.Clear(BackgroundColor);

        // Save and apply scaling and translation
        canvas.Save();
        canvas.Scale(scale);
        canvas.Translate(margin - bounds.MinX, margin - bounds.MinY);

        // Draw connections first (so they appear behind nodes)
        progressCallback?.Invoke("Drawing connections...");
        DrawEnhancedConnections(canvas, rootLayout, metrics);

        // Draw nodes
        progressCallback?.Invoke("Drawing nodes...");
        DrawEnhancedNodes(canvas, rootLayout, metrics);

        canvas.Restore();

        // Save to file
        progressCallback?.Invoke("Saving image to file...");
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    private void DrawEnhancedConnections(SKCanvas canvas, LayoutNode node, TreeMetrics metrics)
    {
        foreach (var child in node.Children)
        {
            // Calculate line properties based on path length and traversal frequency
            var lineColor = CalculateLineColor(child.Value, metrics);
            var lineWidth = CalculateLineWidth(child.Value, metrics);

            using var paint = new SKPaint
            {
                Color = lineColor,
                StrokeWidth = lineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            // Draw line from parent center to child center
            Single parentCenterX = node.X + (node.Width / 2.0f);
            Single parentCenterY = node.Y + (node.Height / 2.0f);
            Single childCenterX = child.X + (child.Width / 2.0f);
            Single childCenterY = child.Y + (child.Height / 2.0f);

            canvas.DrawLine(parentCenterX, parentCenterY, childCenterX, childCenterY, paint);

            // Recursively draw connections for children
            DrawEnhancedConnections(canvas, child, metrics);
        }
    }

    private void DrawEnhancedNodes(SKCanvas canvas, LayoutNode node, TreeMetrics metrics)
    {
        // Calculate node properties based on path length and traversal frequency
        var nodeColor = CalculateNodeColor(node.Value, metrics);
        var nodeRadius = CalculateNodeRadius(node.Value, metrics);

        // Draw simplified node as a circle at the center of the node area
        using var paint = new SKPaint
        {
            Color = nodeColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        Single centerX = node.X + (node.Width / 2.0f);
        Single centerY = node.Y + (node.Height / 2.0f);

        canvas.DrawCircle(centerX, centerY, nodeRadius, paint);

        // Draw a subtle border around the circle for better visibility
        using var borderPaint = new SKPaint
        {
            Color = nodeColor.WithAlpha(180), // Slightly transparent border
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.0f,
            IsAntialias = true
        };

        canvas.DrawCircle(centerX, centerY, nodeRadius, borderPaint);

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawEnhancedNodes(canvas, child, metrics);
        }
    }

    private SKColor CalculateLineColor(BigInteger nodeValue, TreeMetrics metrics)
    {
        // Color depends linearly on log(longest path)
        var pathLength = metrics.PathLengths.GetValueOrDefault(nodeValue, 0);
        var maxPathLength = metrics.LongestPath;

        if (maxPathLength <= 1)
        {
            return SKColor.Parse("#666666"); // Default gray
        }

        // Use logarithmic scaling for color intensity
        Single logPathLength = (Single)Math.Log(pathLength + 1);
        Single logMaxPathLength = (Single)Math.Log(maxPathLength + 1);
        Single normalizedIntensity = logPathLength / logMaxPathLength;

        // Interpolate between blue (low intensity) and red (high intensity)
        Byte red = (Byte)(normalizedIntensity * 255);
        Byte blue = (Byte)((1.0f - normalizedIntensity) * 255);
        Byte green = 64; // Keep some green for visual distinction

        return new SKColor(red, green, blue);
    }

    private Single CalculateLineWidth(BigInteger nodeValue, TreeMetrics metrics)
    {
        // Thickness depends linearly on how often the path is traversed
        var traversalCount = metrics.TraversalCounts.GetValueOrDefault(nodeValue, 1);
        var maxTraversalCount = metrics.TraversalCounts.Values.Max();

        if (maxTraversalCount <= 1)
        {
            return BaseLineStrokeWidth;
        }

        // Linear scaling based on traversal frequency
        Single normalizedThickness = (Single)traversalCount / maxTraversalCount;
        return BaseLineStrokeWidth + (normalizedThickness * (MaxStrokeWidth - BaseLineStrokeWidth));
    }

    private SKColor CalculateNodeColor(BigInteger nodeValue, TreeMetrics metrics)
    {
        // Color depends on log(longest path) - same as line color but slightly different for nodes
        var pathLength = metrics.PathLengths.GetValueOrDefault(nodeValue, 0);
        var maxPathLength = metrics.LongestPath;

        if (maxPathLength <= 1)
        {
            return SKColor.Parse("#4a90e2"); // Default blue
        }

        // Use logarithmic scaling for color intensity
        Single logPathLength = (Single)Math.Log(pathLength + 1);
        Single logMaxPathLength = (Single)Math.Log(maxPathLength + 1);
        Single normalizedIntensity = logPathLength / logMaxPathLength;

        // Interpolate between blue (low intensity) and red (high intensity)
        Byte red = (Byte)(normalizedIntensity * 255);
        Byte blue = (Byte)((1.0f - normalizedIntensity) * 255);
        Byte green = 80; // Keep some green for visual distinction

        return new SKColor(red, green, blue);
    }

    private Single CalculateNodeRadius(BigInteger nodeValue, TreeMetrics metrics)
    {
        // Radius depends on traversal frequency
        var traversalCount = metrics.TraversalCounts.GetValueOrDefault(nodeValue, 1);
        var maxTraversalCount = metrics.TraversalCounts.Values.Max();

        const Single baseRadius = 3.0f;
        const Single maxRadius = 8.0f;

        if (maxTraversalCount <= 1)
        {
            return baseRadius;
        }

        // Scale radius based on traversal frequency
        Single normalizedSize = (Single)traversalCount / maxTraversalCount;
        return baseRadius + (normalizedSize * (maxRadius - baseRadius));
    }

    private (Single MinX, Single MinY, Single Width, Single Height) CalculateBounds(LayoutNode rootLayout)
    {
        var allNodes = GetAllNodes(rootLayout);

        if (!allNodes.Any())
            return (0, 0, 0, 0);

        Single minX = allNodes.Min(n => n.X);
        Single maxX = allNodes.Max(n => n.X + n.Width);
        Single minY = allNodes.Min(n => n.Y);
        Single maxY = allNodes.Max(n => n.Y + n.Height);

        return (minX, minY, maxX - minX, maxY - minY);
    }

    private List<LayoutNode> GetAllNodes(LayoutNode node)
    {
        var nodes = new List<LayoutNode> { node };

        foreach (var child in node.Children)
        {
            nodes.AddRange(GetAllNodes(child));
        }

        return nodes;
    }
}
