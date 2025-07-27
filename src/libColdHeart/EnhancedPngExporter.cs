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
    Rectangle,
    None
}

public class EnhancedPngExporter
{
    private const Single BASE_LINE_STROKE_WIDTH = 1.0f;
    private const String WHITE_COLOR_HEX = "#ffffff";
    private const Byte NODE_GREEN_COLOR_COMPONENT = 80;
    private const Byte LINE_GREEN_COLOR_COMPONENT = 64;
    private const Byte TRANSPARENT_BORDER_ALPHA = 180;
    private const Byte TRANSPARENT_FILL_ALPHA = 100;

    // Progress reporting and layout constants
    private const Int32 CONNECTION_PROGRESS_INTERVAL = 50; // Report progress every 50 connections for frequent updates
    private const Int32 NODE_PROGRESS_INTERVAL = 100; // Report progress every 100 nodes (less frequent than connections)
    private const Single IMAGE_MARGIN = 50.0f; // Margin around the image content

    private static readonly SKColor BackgroundColor = SKColor.Parse(WHITE_COLOR_HEX); // White background as required

    // Cache for expensive calculations to avoid repeated computation
    private readonly ConcurrentDictionary<BigInteger, SKColor> _nodeColorCache = new();
    private readonly ConcurrentDictionary<BigInteger, SKColor> _lineColorCache = new();
    private readonly ConcurrentDictionary<BigInteger, Single> _lineWidthCache = new();
    private readonly ConcurrentDictionary<BigInteger, Single> _nodeRadiusCache = new();

    // Additional caches for optimized processing
    private LayoutNode[]? _flattenedNodes;
    private (Single MinX, Single MinY, Single Width, Single Height)? _cachedBounds;

    public void ExportToPng(LayoutNode rootLayout, TreeMetrics metrics, String filePath, AngularVisualizationConfig config, NodeStyle nodeStyle = NodeStyle.Circle, Action<String>? progressCallback = null)
    {
        // Clear caches for new export
        ClearCaches();

        // Apply path filtering if specified
        var filteredLayout = rootLayout;
        var filteredMetrics = metrics;
        if (config.RenderLongestPaths.HasValue || config.RenderMostTraversedPaths.HasValue || config.RenderLeastTraversedPaths.HasValue || config.RenderRandomPaths.HasValue)
        {
            progressCallback?.Invoke("Filtering paths...");
            (filteredLayout, filteredMetrics) = FilterPaths(rootLayout, metrics, config);
        }

        progressCallback?.Invoke("Precomputing visual properties...");
        var allNodes = GetAllNodesFlattened(filteredLayout);
        PrecomputeVisualPropertiesOptimized(allNodes, filteredMetrics, config);

        progressCallback?.Invoke("Calculating image bounds...");
        var bounds = CalculateBoundsOptimized(filteredLayout);

        // Add margins
        Single margin = IMAGE_MARGIN;
        Single originalWidth = bounds.Width + (2 * margin);
        Single originalHeight = bounds.Height + (2 * margin);

        // Check for reasonable image size limits and apply scaling if necessary
        // MAX_DIMENSION: Maximum width/height for a single image dimension in most graphics systems
        // This prevents memory allocation failures and ensures compatibility across different platforms
        const Int32 MAX_DIMENSION = 32767;
        // MAX_PIXELS: Reasonable limit for total pixel count to prevent excessive memory usage
        // This helps avoid out-of-memory exceptions for very large visualizations
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
        DrawEnhancedConnections(canvas, filteredLayout, filteredMetrics, config, progressCallback);

        // Draw nodes
        progressCallback?.Invoke("Drawing nodes...");
        DrawEnhancedNodes(canvas, filteredLayout, filteredMetrics, config, nodeStyle, progressCallback);

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
        _flattenedNodes = null;
        _cachedBounds = null;
    }

    private void PrecomputeVisualPropertiesOptimized(LayoutNode[] allNodes, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Use parallel processing with array-based access for better cache locality
        Parallel.For(0, allNodes.Length, i =>
        {
            var node = allNodes[i];
            var nodeValue = node.Value;

            // Batch calculate all properties for this node in one go to minimize dictionary lookups
            var nodeColor = CalculateNodeColorDirect(nodeValue, metrics, config);
            var lineColor = CalculateLineColorDirect(nodeValue, metrics, config);
            var lineWidth = CalculateLineWidthDirect(nodeValue, metrics, config);
            var nodeRadius = CalculateNodeRadiusDirect(nodeValue, metrics, config);

            // Update caches atomically
            _nodeColorCache[nodeValue] = nodeColor;
            _lineColorCache[nodeValue] = lineColor;
            _lineWidthCache[nodeValue] = lineWidth;
            _nodeRadiusCache[nodeValue] = nodeRadius;
        });
    }

    private void DrawEnhancedConnections(SKCanvas canvas, LayoutNode node, TreeMetrics metrics, AngularVisualizationConfig config, Action<String>? progressCallback = null)
    {
        if (progressCallback != null)
        {
            var totalNodes = CountNodes(node);
            var processedNodes = 0;
            DrawEnhancedConnectionsBatched(canvas, node, metrics, config, progressCallback, totalNodes, ref processedNodes);
        }
        else
        {
            DrawEnhancedConnectionsOptimized(canvas, node, metrics, config);
        }
    }

    private void DrawEnhancedConnectionsOptimized(SKCanvas canvas, LayoutNode node, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Use a single reusable paint object to minimize allocations
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeJoin = SKStrokeJoin.Round,
            StrokeCap = SKStrokeCap.Round
        };

        if (config.DrawingOrder == DrawingOrder.LeastToMostTraversed)
        {
            DrawConnectionsOrderedByTraversal(canvas, node, metrics, paint);
        }
        else
        {
            DrawConnectionsRecursiveOptimized(canvas, node, paint);
        }
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
            IsAntialias = true,
            StrokeJoin = SKStrokeJoin.Round,
            StrokeCap = SKStrokeCap.Round
        };

        DrawConnectionsWithProgressRecursive(canvas, node, paint, progressCallback, totalNodes, ref processedNodes);
    }

    private void DrawEnhancedConnectionsBatched(SKCanvas canvas, LayoutNode node, TreeMetrics metrics, AngularVisualizationConfig config, Action<String> progressCallback, Int32 totalNodes, ref Int32 processedNodes)
    {
        // Collect all connections and group by similar properties for batch drawing
        var connections = CollectAllConnections(node);

        // Sort connections by traversal frequency if requested
        if (config.DrawingOrder == DrawingOrder.LeastToMostTraversed)
        {
            connections = connections
                .OrderBy(c => metrics.TraversalCounts.GetValueOrDefault(c.Child.Value, 0))
                .ToList();
        }

        // Group connections by line width and color for more efficient drawing
        var groupedConnections = connections
            .GroupBy(c => new { Color = _lineColorCache[c.Child.Value], Width = _lineWidthCache[c.Child.Value] })
            .ToArray();

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeJoin = SKStrokeJoin.Round,
            StrokeCap = SKStrokeCap.Round
        };

        foreach (var group in groupedConnections)
        {
            // Set paint properties once per group
            paint.Color = group.Key.Color;
            paint.StrokeWidth = group.Key.Width;

            var connectionsInGroup = group.ToArray();

            // Draw all connections in this group
            foreach (var connection in connectionsInGroup)
            {
                canvas.DrawLine(connection.StartX, connection.StartY, connection.EndX, connection.EndY, paint);

                processedNodes++;

                // Report progress every 50 nodes
                if (processedNodes % CONNECTION_PROGRESS_INTERVAL == 0 || processedNodes == totalNodes)
                {
                    var percentage = (Int32)((processedNodes / (Single)totalNodes) * 100);
                    progressCallback($"Drawing connections... {processedNodes}/{totalNodes} ({percentage}%)");
                }
            }
        }
    }

    private List<(LayoutNode Parent, LayoutNode Child, Single StartX, Single StartY, Single EndX, Single EndY)> CollectAllConnections(LayoutNode node)
    {
        var connections = new List<(LayoutNode, LayoutNode, Single, Single, Single, Single)>();

        CollectConnectionsRecursive(node, connections);

        return connections;
    }

    private void DrawConnectionsOrderedByTraversal(SKCanvas canvas, LayoutNode node, TreeMetrics metrics, SKPaint paint)
    {
        // Collect all connections first
        var connections = CollectAllConnections(node);

        // Sort by traversal frequency (least to most)
        var sortedConnections = connections
            .OrderBy(c => metrics.TraversalCounts.GetValueOrDefault(c.Child.Value, 0))
            .ToArray();

        // Draw connections in order
        foreach (var connection in sortedConnections)
        {
            // Use cached values for color and width
            paint.Color = _lineColorCache[connection.Child.Value];
            paint.StrokeWidth = _lineWidthCache[connection.Child.Value];

            canvas.DrawLine(connection.StartX, connection.StartY, connection.EndX, connection.EndY, paint);
        }
    }

    private void CollectConnectionsRecursive(LayoutNode node, List<(LayoutNode, LayoutNode, Single, Single, Single, Single)> connections)
    {
        foreach (var child in node.Children)
        {
            Single parentCenterX = node.X + (node.Width / 2.0f);
            Single parentCenterY = node.Y + (node.Height / 2.0f);
            Single childCenterX = child.X + (child.Width / 2.0f);
            Single childCenterY = child.Y + (child.Height / 2.0f);

            connections.Add((node, child, parentCenterX, parentCenterY, childCenterX, childCenterY));

            CollectConnectionsRecursive(child, connections);
        }
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
            if (processedNodes % CONNECTION_PROGRESS_INTERVAL == 0 || processedNodes == totalNodes)
            {
                var percentage = (Int32)((processedNodes / (Single)totalNodes) * 100);
                progressCallback($"Drawing connections... {processedNodes}/{totalNodes} ({percentage}%)");
            }

            // Recursively draw connections for children
            DrawConnectionsWithProgressRecursive(canvas, child, paint, progressCallback, totalNodes, ref processedNodes);
        }
    }

    private void DrawEnhancedNodes(SKCanvas canvas, LayoutNode node, TreeMetrics metrics, AngularVisualizationConfig config, NodeStyle nodeStyle, Action<String>? progressCallback = null)
    {
        // Skip drawing nodes if style is None
        if (nodeStyle == NodeStyle.None)
        {
            return;
        }

        if (config.DrawingOrder == DrawingOrder.LeastToMostTraversed)
        {
            DrawNodesOrderedByTraversal(canvas, node, metrics, nodeStyle, progressCallback);
        }
        else if (nodeStyle == NodeStyle.Circle)
        {
            DrawCircleNodesOptimized(canvas, node, progressCallback);
        }
        else
        {
            DrawRectangleNodesOptimized(canvas, node, progressCallback);
        }
    }

    private void DrawNodesOrderedByTraversal(SKCanvas canvas, LayoutNode rootNode, TreeMetrics metrics, NodeStyle nodeStyle, Action<String>? progressCallback = null)
    {
        // Collect all nodes first
        var allNodes = new List<LayoutNode>();
        CollectAllNodes(rootNode, allNodes);

        // Sort nodes by traversal frequency (least to most traversed)
        var sortedNodes = allNodes
            .OrderBy(n => metrics.TraversalCounts.GetValueOrDefault(n.Value, 0))
            .ToList();

        // Draw nodes in order based on their style
        if (nodeStyle == NodeStyle.Circle)
        {
            DrawCircleNodesFromList(canvas, sortedNodes, progressCallback);
        }
        else
        {
            DrawRectangleNodesFromList(canvas, sortedNodes, progressCallback);
        }
    }

    private void CollectAllNodes(LayoutNode node, List<LayoutNode> nodesList)
    {
        nodesList.Add(node);
        foreach (var child in node.Children)
        {
            CollectAllNodes(child, nodesList);
        }
    }

    private void DrawCircleNodesFromList(SKCanvas canvas, List<LayoutNode> nodes, Action<String>? progressCallback = null)
    {
        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var borderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2.0f,
            IsAntialias = true
        };

        var totalNodes = nodes.Count;
        for (var i = 0; i < totalNodes; i++)
        {
            var node = nodes[i];
            var nodeColor = _nodeColorCache[node.Value];
            var nodeRadius = _nodeRadiusCache[node.Value];

            fillPaint.Color = nodeColor.WithAlpha(TRANSPARENT_FILL_ALPHA);
            borderPaint.Color = nodeColor.WithAlpha(TRANSPARENT_BORDER_ALPHA);

            var centerX = node.X + node.Width / 2;
            var centerY = node.Y + node.Height / 2;

            canvas.DrawCircle(centerX, centerY, nodeRadius, fillPaint);
            canvas.DrawCircle(centerX, centerY, nodeRadius, borderPaint);

            // Report progress periodically
            if (progressCallback != null && (i + 1) % NODE_PROGRESS_INTERVAL == 0)
            {
                progressCallback($"Drawing nodes... {i + 1}/{totalNodes}");
            }
        }
    }

    private void DrawRectangleNodesFromList(SKCanvas canvas, List<LayoutNode> nodes, Action<String>? progressCallback = null)
    {
        const Single DefaultFontSize = 12.0f;
        const Single NodeStrokeWidth = 2.0f;
        const Single CornerRadius = 5.0f;

        using var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = NodeStrokeWidth,
            IsAntialias = true
        };

        using var textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Color = SKColors.Black,
            TextSize = DefaultFontSize,
            Typeface = SKTypeface.Default
        };

        var totalNodes = nodes.Count;
        for (var i = 0; i < totalNodes; i++)
        {
            var node = nodes[i];
            var nodeColor = _nodeColorCache[node.Value];

            fillPaint.Color = nodeColor.WithAlpha(TRANSPARENT_FILL_ALPHA);
            strokePaint.Color = nodeColor.WithAlpha(TRANSPARENT_BORDER_ALPHA);

            var rect = new SKRect(node.X, node.Y, node.X + node.Width, node.Y + node.Height);
            canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, fillPaint);
            canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, strokePaint);

            // Draw text
            var text = node.Value.ToString();
            var textBounds = new SKRect();
            textPaint.MeasureText(text, ref textBounds);

            var textX = node.X + (node.Width - textBounds.Width) / 2;
            var textY = node.Y + (node.Height - textBounds.Height) / 2 - textBounds.Top;
            canvas.DrawText(text, textX, textY, textPaint);

            // Report progress periodically
            if (progressCallback != null && (i + 1) % NODE_PROGRESS_INTERVAL == 0)
            {
                progressCallback($"Drawing nodes... {i + 1}/{totalNodes}");
            }
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
        borderPaint.Color = nodeColor.WithAlpha(TRANSPARENT_BORDER_ALPHA); // Slightly transparent border

        Single centerX = node.X + (node.Width / 2.0f);
        Single centerY = node.Y + (node.Height / 2.0f);

        canvas.DrawCircle(centerX, centerY, nodeRadius, fillPaint);
        canvas.DrawCircle(centerX, centerY, nodeRadius, borderPaint);

        processedNodes++;

        // Report progress every 100 nodes for node drawing (less frequent than connections)
        if (processedNodes % NODE_PROGRESS_INTERVAL == 0 || processedNodes == totalNodes)
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
        borderPaint.Color = nodeColor.WithAlpha(TRANSPARENT_BORDER_ALPHA); // Slightly transparent border

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
        var fillColor = nodeColor.WithAlpha(TRANSPARENT_FILL_ALPHA);

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
        if (processedNodes % NODE_PROGRESS_INTERVAL == 0 || processedNodes == totalNodes)
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
        var fillColor = nodeColor.WithAlpha(TRANSPARENT_FILL_ALPHA);

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

    private SKColor CalculateLineColor(BigInteger nodeValue, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Check cache first for performance
        if (_lineColorCache.TryGetValue(nodeValue, out var cachedColor))
        {
            return cachedColor;
        }

        return CalculateLineColorDirect(nodeValue, metrics, config);
    }

    private Single CalculateLineWidth(BigInteger nodeValue, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Check cache first for performance
        if (_lineWidthCache.TryGetValue(nodeValue, out var cachedWidth))
        {
            return cachedWidth;
        }

        return CalculateLineWidthDirect(nodeValue, metrics, config);
    }

    private SKColor CalculateNodeColor(BigInteger nodeValue, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Check cache first for performance
        if (_nodeColorCache.TryGetValue(nodeValue, out var cachedColor))
        {
            return cachedColor;
        }

        return CalculateNodeColorDirect(nodeValue, metrics, config);
    }

    private Single CalculateNodeRadius(BigInteger nodeValue, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Check cache first for performance
        if (_nodeRadiusCache.TryGetValue(nodeValue, out var cachedRadius))
        {
            return cachedRadius;
        }

        return CalculateNodeRadiusDirect(nodeValue, metrics, config);
    }

    // Direct calculation methods that bypass cache lookup for initial precomputation
    private SKColor CalculateNodeColorDirect(BigInteger nodeValue, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Color depends on log(longest path) with configurable impact
        var pathLength = metrics.PathLengths.GetValueOrDefault(nodeValue, 0);
        var maxPathLength = metrics.LongestPath;

        if (maxPathLength <= 1)
        {
            return SKColor.Parse("#4a90e2"); // Default blue
        }

        // Use logarithmic scaling for color intensity with configurable impact
        Single logPathLength = (Single)Math.Log(pathLength + 1);
        Single logMaxPathLength = (Single)Math.Log(maxPathLength + 1);
        Single normalizedIntensity = logPathLength / logMaxPathLength;

        // Apply color impact - higher values increase color variation for longer paths
        Single impactAdjustedIntensity = (Single)Math.Pow(normalizedIntensity, 1.0f / Math.Max(config.ColorImpact, 0.1f));

        // Interpolate between blue (low intensity) and red (high intensity)
        Byte red = (Byte)(impactAdjustedIntensity * 255);
        Byte blue = (Byte)((1.0f - impactAdjustedIntensity) * 255);
        Byte green = NODE_GREEN_COLOR_COMPONENT; // Keep some green for visual distinction

        return new SKColor(red, green, blue);
    }

    private SKColor CalculateLineColorDirect(BigInteger nodeValue, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Color depends on log(longest path) with configurable impact
        var pathLength = metrics.PathLengths.GetValueOrDefault(nodeValue, 0);
        var maxPathLength = metrics.LongestPath;

        if (maxPathLength <= 1)
        {
            return SKColor.Parse("#666666"); // Default gray
        }

        // Use logarithmic scaling for color intensity with configurable impact
        Single logPathLength = (Single)Math.Log(pathLength + 1);
        Single logMaxPathLength = (Single)Math.Log(maxPathLength + 1);
        Single normalizedIntensity = logPathLength / logMaxPathLength;

        // Apply color impact - higher values increase color variation for longer paths
        Single impactAdjustedIntensity = (Single)Math.Pow(normalizedIntensity, 1.0f / Math.Max(config.ColorImpact, 0.1f));

        // Interpolate between blue (low intensity) and red (high intensity)
        Byte red = (Byte)(impactAdjustedIntensity * 255);
        Byte blue = (Byte)((1.0f - impactAdjustedIntensity) * 255);
        Byte green = LINE_GREEN_COLOR_COMPONENT; // Keep some green for visual distinction

        return new SKColor(red, green, blue);
    }

    private Single CalculateLineWidthDirect(BigInteger nodeValue, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Thickness depends on how often the path is traversed, scaled by impact factor
        var traversalCount = metrics.TraversalCounts.GetValueOrDefault(nodeValue, 1);
        var maxTraversalCount = metrics.TraversalCounts.Values.DefaultIfEmpty(1).Max();

        if (maxTraversalCount <= 1 || Math.Abs(config.ThicknessImpact) < 1e-6f)
        {
            return BASE_LINE_STROKE_WIDTH;
        }

        // Apply impact factor - 0 means no impact, higher values amplify the effect
        Single normalizedThickness = (Single)traversalCount / maxTraversalCount;
        Single impactAdjustedThickness = (Single)Math.Pow(normalizedThickness, 1.0f / Math.Max(config.ThicknessImpact, 0.1f));

        return BASE_LINE_STROKE_WIDTH + (impactAdjustedThickness * (config.MaxLineWidth - BASE_LINE_STROKE_WIDTH));
    }

    private Single CalculateNodeRadiusDirect(BigInteger nodeValue, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        // Radius depends on traversal frequency, scaled by impact factor
        var traversalCount = metrics.TraversalCounts.GetValueOrDefault(nodeValue, 1);
        var maxTraversalCount = metrics.TraversalCounts.Values.DefaultIfEmpty(1).Max();

        const Single baseRadius = 3.0f;
        const Single maxRadius = 8.0f;

        if (maxTraversalCount <= 1 || Math.Abs(config.ThicknessImpact) < 1e-6f)
        {
            return baseRadius;
        }

        // Apply same impact factor as line thickness for consistency
        Single normalizedSize = (Single)traversalCount / maxTraversalCount;
        Single impactAdjustedSize = (Single)Math.Pow(normalizedSize, 1.0f / Math.Max(config.ThicknessImpact, 0.1f));

        return baseRadius + (impactAdjustedSize * (maxRadius - baseRadius));
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

    private (Single MinX, Single MinY, Single Width, Single Height) CalculateBoundsOptimized(LayoutNode rootLayout)
    {
        // Use cached bounds if available
        if (_cachedBounds.HasValue)
            return _cachedBounds.Value;

        var allNodes = GetAllNodesFlattened(rootLayout);

        if (allNodes.Length == 0)
        {
            _cachedBounds = (0, 0, 0, 0);
            return _cachedBounds.Value;
        }

        // Use parallel processing to find min/max values more efficiently
        Single minX = Single.MaxValue, maxX = Single.MinValue;
        Single minY = Single.MaxValue, maxY = Single.MinValue;

        // Process in chunks for better cache performance
        // Chunk size of 1000 provides good balance between parallelization overhead
        // and cache locality for typical tree sizes
        const Int32 chunkSize = 1000;
        var chunks = Enumerable.Range(0, (allNodes.Length + chunkSize - 1) / chunkSize)
            .Select(i => new { Start = i * chunkSize, End = Math.Min((i + 1) * chunkSize, allNodes.Length) });

        var results = chunks.AsParallel().Select(chunk =>
        {
            Single chunkMinX = Single.MaxValue, chunkMaxX = Single.MinValue;
            Single chunkMinY = Single.MaxValue, chunkMaxY = Single.MinValue;

            for (Int32 j = chunk.Start; j < chunk.End; j++)
            {
                var node = allNodes[j];
                chunkMinX = Math.Min(chunkMinX, node.X);
                chunkMaxX = Math.Max(chunkMaxX, node.X + node.Width);
                chunkMinY = Math.Min(chunkMinY, node.Y);
                chunkMaxY = Math.Max(chunkMaxY, node.Y + node.Height);
            }

            return new { MinX = chunkMinX, MaxX = chunkMaxX, MinY = chunkMinY, MaxY = chunkMaxY };
        }).ToArray();

        // Combine results
        foreach (var result in results)
        {
            minX = Math.Min(minX, result.MinX);
            maxX = Math.Max(maxX, result.MaxX);
            minY = Math.Min(minY, result.MinY);
            maxY = Math.Max(maxY, result.MaxY);
        }

        _cachedBounds = (minX, minY, maxX - minX, maxY - minY);
        return _cachedBounds.Value;
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

    private LayoutNode[] GetAllNodesFlattened(LayoutNode rootLayout)
    {
        // Use cached flattened nodes if available
        if (_flattenedNodes != null)
            return _flattenedNodes;

        // Count nodes first to pre-allocate array
        var nodeCount = CountNodes(rootLayout);
        _flattenedNodes = new LayoutNode[nodeCount];

        // Flatten tree into array using iterative approach to avoid stack overflow
        var index = 0;
        var stack = new Stack<LayoutNode>();
        stack.Push(rootLayout);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            _flattenedNodes[index++] = current;

            // Add children in reverse order to maintain left-to-right processing
            for (Int32 i = current.Children.Count - 1; i >= 0; i--)
            {
                stack.Push(current.Children[i]);
            }
        }

        return _flattenedNodes;
    }

    private (LayoutNode filteredLayout, TreeMetrics filteredMetrics) FilterPaths(LayoutNode rootLayout, TreeMetrics metrics, AngularVisualizationConfig config)
    {
        HashSet<BigInteger> selectedValues;

        if (config.RenderLongestPaths.HasValue)
        {
            // Select top N nodes with longest paths
            selectedValues = metrics.PathLengths
                .OrderByDescending(kvp => kvp.Value)
                .Take(config.RenderLongestPaths.Value)
                .Select(kvp => kvp.Key)
                .ToHashSet();
        }
        else if (config.RenderMostTraversedPaths.HasValue)
        {
            // Select top N nodes with highest traversal counts
            selectedValues = metrics.TraversalCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(config.RenderMostTraversedPaths.Value)
                .Select(kvp => kvp.Key)
                .ToHashSet();
        }
        else if (config.RenderLeastTraversedPaths.HasValue)
        {
            // Select top N nodes with lowest traversal counts
            selectedValues = metrics.TraversalCounts
                .OrderBy(kvp => kvp.Value)
                .Take(config.RenderLeastTraversedPaths.Value)
                .Select(kvp => kvp.Key)
                .ToHashSet();
        }
        else if (config.RenderRandomPaths.HasValue)
        {
            // Select N random nodes
            var allValues = metrics.PathLengths.Keys.ToArray();
            var random = new Random();
            selectedValues = allValues
                .OrderBy(_ => random.Next())
                .Take(config.RenderRandomPaths.Value)
                .ToHashSet();
        }
        else
        {
            // No filtering - return original
            return (rootLayout, metrics);
        }

        // Ensure root is always included
        selectedValues.Add(rootLayout.Value);

        // Build filtered layout tree keeping only paths to selected nodes
        var filteredLayout = FilterLayoutTree(rootLayout, selectedValues);

        // Get ALL node values that appear in the filtered tree (not just originally selected ones)
        var allFilteredNodeValues = GetAllNodeValues(filteredLayout);

        // Build filtered metrics - include ALL nodes that appear in the filtered tree
        // This ensures proper color calculation for intermediate nodes in the filtered paths
        var filteredPathLengths = metrics.PathLengths
            .Where(kvp => allFilteredNodeValues.Contains(kvp.Key))
            .ToDictionary();
        var filteredTraversalCounts = metrics.TraversalCounts
            .Where(kvp => allFilteredNodeValues.Contains(kvp.Key))
            .ToDictionary();

        // CRITICAL FIX: Preserve original FurthestDistance and LongestPath for proper color scaling
        // This ensures that colors remain consistent with the original tree, preventing blue nodes
        // from appearing unexpectedly in the middle of what should be red paths
        var filteredMetrics = new TreeMetrics(
            metrics.FurthestDistance,  // Keep original furthest distance
            metrics.LongestPath,       // Keep original longest path
            filteredPathLengths,
            filteredTraversalCounts
        );

        return (filteredLayout, filteredMetrics);
    }

    private LayoutNode FilterLayoutTree(LayoutNode node, HashSet<BigInteger> selectedValues)
    {
        // Create new layout node for current node
        var filteredNode = new LayoutNode(node.Value)
        {
            X = node.X,
            Y = node.Y,
            Width = node.Width,
            Height = node.Height
        };

        // Add children that are either selected or lead to selected nodes
        foreach (var child in node.Children)
        {
            if (HasSelectedDescendant(child, selectedValues))
            {
                filteredNode.Children.Add(FilterLayoutTree(child, selectedValues));
            }
        }

        return filteredNode;
    }

    private Boolean HasSelectedDescendant(LayoutNode node, HashSet<BigInteger> selectedValues)
    {
        // Check if this node is selected
        if (selectedValues.Contains(node.Value))
            return true;

        // Check if any descendant is selected
        return node.Children.Any(child => HasSelectedDescendant(child, selectedValues));
    }

    private HashSet<BigInteger> GetAllNodeValues(LayoutNode node)
    {
        var nodeValues = new HashSet<BigInteger>();
        CollectNodeValues(node, nodeValues);
        return nodeValues;
    }

    private void CollectNodeValues(LayoutNode node, HashSet<BigInteger> nodeValues)
    {
        nodeValues.Add(node.Value);

        foreach (var child in node.Children)
        {
            CollectNodeValues(child, nodeValues);
        }
    }
}
