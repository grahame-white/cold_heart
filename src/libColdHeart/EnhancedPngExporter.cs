using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using SkiaSharp;

namespace libColdHeart;

public enum NodeStyle
{
    Circle,
    Rectangle
}

public class EnhancedPngExporter
{
    private const Single BaseLineStrokeWidth = 1.0f;
    private const Single MaxStrokeWidth = 8.0f;

    private static readonly SKColor BackgroundColor = SKColor.Parse("#ffffff"); // White background as required

    // Cache for expensive calculations to avoid repeated computation
    private readonly ConcurrentDictionary<BigInteger, SKColor> _nodeColorCache = new();
    private readonly ConcurrentDictionary<BigInteger, SKColor> _lineColorCache = new();
    private readonly ConcurrentDictionary<BigInteger, Single> _lineWidthCache = new();
    private readonly ConcurrentDictionary<BigInteger, Single> _nodeRadiusCache = new();

    public void ExportToPng(LayoutNode rootLayout, TreeMetrics metrics, String filePath, NodeStyle nodeStyle = NodeStyle.Circle, Action<String>? progressCallback = null)
    {
        // Clear caches for new export
        ClearCaches();

        progressCallback?.Invoke("Precomputing visual properties...");
        var allNodes = GetAllNodes(rootLayout);
        PrecomputeVisualProperties(allNodes, metrics);

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
        DrawEnhancedConnections(canvas, rootLayout, metrics, progressCallback);

        // Draw nodes
        progressCallback?.Invoke("Drawing nodes...");
        DrawEnhancedNodes(canvas, rootLayout, metrics, nodeStyle, progressCallback);

        canvas.Restore();

        // Save to file
        progressCallback?.Invoke("Saving image to file...");
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    private void ClearCaches()
    {
        _nodeColorCache.Clear();
        _lineColorCache.Clear();
        _lineWidthCache.Clear();
        _nodeRadiusCache.Clear();
    }

    private void PrecomputeVisualProperties(List<LayoutNode> allNodes, TreeMetrics metrics)
    {
        // Use parallel processing to compute all visual properties upfront
        Parallel.ForEach(allNodes, node =>
        {
            var nodeValue = node.Value;
            _nodeColorCache[nodeValue] = CalculateNodeColor(nodeValue, metrics);
            _lineColorCache[nodeValue] = CalculateLineColor(nodeValue, metrics);
            _lineWidthCache[nodeValue] = CalculateLineWidth(nodeValue, metrics);
            _nodeRadiusCache[nodeValue] = CalculateNodeRadius(nodeValue, metrics);
        });
    }

    private void DrawEnhancedConnections(SKCanvas canvas, LayoutNode node, TreeMetrics metrics, Action<String>? progressCallback = null)
    {
        if (progressCallback != null)
        {
            var totalNodes = CountNodes(node);
            var processedNodes = 0;
            DrawEnhancedConnectionsWithProgress(canvas, node, metrics, progressCallback, totalNodes, ref processedNodes);
        }
        else
        {
            DrawEnhancedConnectionsOptimized(canvas, node);
        }
    }

    private void DrawEnhancedConnectionsOptimized(SKCanvas canvas, LayoutNode node)
    {
        // Use a single reusable paint object to minimize allocations
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        DrawConnectionsRecursiveOptimized(canvas, node, paint);
    }

    private void DrawConnectionsRecursiveOptimized(SKCanvas canvas, LayoutNode node, SKPaint paint)
    {
        foreach (var child in node.Children)
        {
            // Use cached values instead of recalculating
            var lineColor = _lineColorCache[child.Value];
            var lineWidth = _lineWidthCache[child.Value];

            // Update paint properties instead of creating new paint
            paint.Color = lineColor;
            paint.StrokeWidth = lineWidth;

            // Draw line from parent center to child center
            Single parentCenterX = node.X + (node.Width / 2.0f);
            Single parentCenterY = node.Y + (node.Height / 2.0f);
            Single childCenterX = child.X + (child.Width / 2.0f);
            Single childCenterY = child.Y + (child.Height / 2.0f);

            canvas.DrawLine(parentCenterX, parentCenterY, childCenterX, childCenterY, paint);

            // Recursively draw connections for children
            DrawConnectionsRecursiveOptimized(canvas, child, paint);
        }
    }

    private void DrawEnhancedConnectionsWithProgress(SKCanvas canvas, LayoutNode node, TreeMetrics metrics, Action<String> progressCallback, Int32 totalNodes, ref Int32 processedNodes)
    {
        // Use a single reusable paint object to minimize allocations
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        DrawConnectionsWithProgressRecursive(canvas, node, paint, progressCallback, totalNodes, ref processedNodes);
    }

    private void DrawConnectionsWithProgressRecursive(SKCanvas canvas, LayoutNode node, SKPaint paint, Action<String> progressCallback, Int32 totalNodes, ref Int32 processedNodes)
    {
        foreach (var child in node.Children)
        {
            // Use cached values instead of recalculating
            var lineColor = _lineColorCache[child.Value];
            var lineWidth = _lineWidthCache[child.Value];

            // Update paint properties instead of creating new paint
            paint.Color = lineColor;
            paint.StrokeWidth = lineWidth;

            // Draw line from parent center to child center
            Single parentCenterX = node.X + (node.Width / 2.0f);
            Single parentCenterY = node.Y + (node.Height / 2.0f);
            Single childCenterX = child.X + (child.Width / 2.0f);
            Single childCenterY = child.Y + (child.Height / 2.0f);

            canvas.DrawLine(parentCenterX, parentCenterY, childCenterX, childCenterY, paint);

            processedNodes++;

            // Report progress every 50 nodes to avoid excessive output
            if (processedNodes % 50 == 0 || processedNodes == totalNodes)
            {
                var percentage = (Int32)((processedNodes / (Single)totalNodes) * 100);
                progressCallback($"Drawing connections... {processedNodes}/{totalNodes} ({percentage}%)");
            }

            // Recursively draw connections for children
            DrawConnectionsWithProgressRecursive(canvas, child, paint, progressCallback, totalNodes, ref processedNodes);
        }
    }

    private void DrawEnhancedNodes(SKCanvas canvas, LayoutNode node, TreeMetrics metrics, NodeStyle nodeStyle, Action<String>? progressCallback = null)
    {
        if (nodeStyle == NodeStyle.Circle)
        {
            DrawCircleNodesOptimized(canvas, node, progressCallback);
        }
        else
        {
            DrawRectangleNodesOptimized(canvas, node, progressCallback);
        }
    }

    private void DrawCircleNodesOptimized(SKCanvas canvas, LayoutNode node, Action<String>? progressCallback = null)
    {
        // Use reusable paint objects to minimize allocations
        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var borderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.0f,
            IsAntialias = true
        };

        if (progressCallback != null)
        {
            var totalNodes = CountNodes(node);
            var processedNodes = 0;
            DrawCircleNodesWithProgress(canvas, node, fillPaint, borderPaint, progressCallback, totalNodes, ref processedNodes);
        }
        else
        {
            DrawCircleNodesRecursive(canvas, node, fillPaint, borderPaint);
        }
    }

    private void DrawCircleNodesWithProgress(SKCanvas canvas, LayoutNode node, SKPaint fillPaint, SKPaint borderPaint, Action<String> progressCallback, Int32 totalNodes, ref Int32 processedNodes)
    {
        // Use cached values
        var nodeColor = _nodeColorCache[node.Value];
        var nodeRadius = _nodeRadiusCache[node.Value];

        // Update paint properties
        fillPaint.Color = nodeColor;
        borderPaint.Color = nodeColor.WithAlpha(180); // Slightly transparent border

        Single centerX = node.X + (node.Width / 2.0f);
        Single centerY = node.Y + (node.Height / 2.0f);

        canvas.DrawCircle(centerX, centerY, nodeRadius, fillPaint);
        canvas.DrawCircle(centerX, centerY, nodeRadius, borderPaint);

        processedNodes++;

        // Report progress every 100 nodes for node drawing (less frequent than connections)
        if (processedNodes % 100 == 0 || processedNodes == totalNodes)
        {
            var percentage = (Int32)((processedNodes / (Single)totalNodes) * 100);
            progressCallback($"Drawing nodes... {processedNodes}/{totalNodes} ({percentage}%)");
        }

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawCircleNodesWithProgress(canvas, child, fillPaint, borderPaint, progressCallback, totalNodes, ref processedNodes);
        }
    }

    private void DrawCircleNodesRecursive(SKCanvas canvas, LayoutNode node, SKPaint fillPaint, SKPaint borderPaint)
    {
        // Use cached values
        var nodeColor = _nodeColorCache[node.Value];
        var nodeRadius = _nodeRadiusCache[node.Value];

        // Update paint properties
        fillPaint.Color = nodeColor;
        borderPaint.Color = nodeColor.WithAlpha(180); // Slightly transparent border

        Single centerX = node.X + (node.Width / 2.0f);
        Single centerY = node.Y + (node.Height / 2.0f);

        canvas.DrawCircle(centerX, centerY, nodeRadius, fillPaint);
        canvas.DrawCircle(centerX, centerY, nodeRadius, borderPaint);

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawCircleNodesRecursive(canvas, child, fillPaint, borderPaint);
        }
    }

    private void DrawRectangleNodesOptimized(SKCanvas canvas, LayoutNode node, Action<String>? progressCallback = null)
    {
        const Single DefaultFontSize = 12.0f;
        const Single NodeStrokeWidth = 2.0f;
        const Single CornerRadius = 5.0f;

        // Use reusable paint objects
        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var strokePaint = new SKPaint
        {
            StrokeWidth = NodeStrokeWidth,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        using var textPaint = new SKPaint
        {
            Color = SKColor.Parse("#000000"),
            TextSize = DefaultFontSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial")
        };

        if (progressCallback != null)
        {
            var totalNodes = CountNodes(node);
            var processedNodes = 0;
            DrawRectangleNodesWithProgress(canvas, node, fillPaint, strokePaint, textPaint, CornerRadius, progressCallback, totalNodes, ref processedNodes);
        }
        else
        {
            DrawRectangleNodesRecursive(canvas, node, fillPaint, strokePaint, textPaint, CornerRadius);
        }
    }

    private void DrawRectangleNodesWithProgress(SKCanvas canvas, LayoutNode node, SKPaint fillPaint, SKPaint strokePaint, SKPaint textPaint, Single cornerRadius, Action<String> progressCallback, Int32 totalNodes, ref Int32 processedNodes)
    {
        // Use cached color
        var nodeColor = _nodeColorCache[node.Value];
        var fillColor = nodeColor.WithAlpha(100);

        // Update paint properties
        fillPaint.Color = fillColor;
        strokePaint.Color = nodeColor;

        var rect = new SKRect(node.X, node.Y, node.X + node.Width, node.Y + node.Height);
        using var roundRect = new SKRoundRect(rect, cornerRadius, cornerRadius);

        canvas.DrawRoundRect(roundRect, fillPaint);
        canvas.DrawRoundRect(roundRect, strokePaint);

        // Draw node text (value)
        Single textX = node.X + (node.Width / 2.0f);
        Single textY = node.Y + (node.Height / 2.0f) + (textPaint.TextSize / 3.0f); // Adjust for baseline

        canvas.DrawText(node.Value.ToString(), textX, textY, textPaint);

        processedNodes++;

        // Report progress every 100 nodes for node drawing
        if (processedNodes % 100 == 0 || processedNodes == totalNodes)
        {
            var percentage = (Int32)((processedNodes / (Single)totalNodes) * 100);
            progressCallback($"Drawing nodes... {processedNodes}/{totalNodes} ({percentage}%)");
        }

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawRectangleNodesWithProgress(canvas, child, fillPaint, strokePaint, textPaint, cornerRadius, progressCallback, totalNodes, ref processedNodes);
        }
    }

    private void DrawRectangleNodesRecursive(SKCanvas canvas, LayoutNode node, SKPaint fillPaint, SKPaint strokePaint, SKPaint textPaint, Single cornerRadius)
    {
        // Use cached color
        var nodeColor = _nodeColorCache[node.Value];
        var fillColor = nodeColor.WithAlpha(100);

        // Update paint properties
        fillPaint.Color = fillColor;
        strokePaint.Color = nodeColor;

        var rect = new SKRect(node.X, node.Y, node.X + node.Width, node.Y + node.Height);
        using var roundRect = new SKRoundRect(rect, cornerRadius, cornerRadius);

        canvas.DrawRoundRect(roundRect, fillPaint);
        canvas.DrawRoundRect(roundRect, strokePaint);

        // Draw node text (value)
        Single textX = node.X + (node.Width / 2.0f);
        Single textY = node.Y + (node.Height / 2.0f) + (textPaint.TextSize / 3.0f); // Adjust for baseline

        canvas.DrawText(node.Value.ToString(), textX, textY, textPaint);

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawRectangleNodesRecursive(canvas, child, fillPaint, strokePaint, textPaint, cornerRadius);
        }
    }

    private Int32 CountNodes(LayoutNode node)
    {
        Int32 count = 1; // Count current node
        foreach (var child in node.Children)
        {
            count += CountNodes(child);
        }
        return count;
    }

    private SKColor CalculateLineColor(BigInteger nodeValue, TreeMetrics metrics)
    {
        // Check cache first for performance
        if (_lineColorCache.TryGetValue(nodeValue, out var cachedColor))
        {
            return cachedColor;
        }

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
        // Check cache first for performance
        if (_lineWidthCache.TryGetValue(nodeValue, out var cachedWidth))
        {
            return cachedWidth;
        }

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
        // Check cache first for performance
        if (_nodeColorCache.TryGetValue(nodeValue, out var cachedColor))
        {
            return cachedColor;
        }

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
        // Check cache first for performance
        if (_nodeRadiusCache.TryGetValue(nodeValue, out var cachedRadius))
        {
            return cachedRadius;
        }

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
