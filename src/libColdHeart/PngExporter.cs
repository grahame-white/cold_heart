using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using SkiaSharp;

namespace libColdHeart;

public class PngExporter
{
    private const System.Single DefaultFontSize = 12.0f;
    private const System.Single NodeStrokeWidth = 2.0f;
    private const System.Single LineStrokeWidth = 2.0f;
    private const System.Single CornerRadius = 5.0f;

    private static readonly SKColor NodeFillColor = SKColor.Parse("#e6f3ff");
    private static readonly SKColor NodeStrokeColor = SKColor.Parse("#0066cc");
    private static readonly SKColor TextColor = SKColor.Parse("#000000");
    private static readonly SKColor LineColor = SKColor.Parse("#666666");
    private static readonly SKColor BackgroundColor = SKColor.Parse("#ffffff");

    public void ExportToPng(LayoutNode rootLayout, System.String filePath)
    {
        var bounds = CalculateBounds(rootLayout);

        // Add margins
        System.Single margin = 20.0f;
        System.Single originalWidth = bounds.Width + (2 * margin);
        System.Single originalHeight = bounds.Height + (2 * margin);

        // Check for reasonable image size limits and apply scaling if necessary
        const System.Int32 MAX_DIMENSION = 32767; // Maximum dimension for most image formats
        const System.Int64 MAX_PIXELS = 100_000_000; // 100 million pixels max

        System.Single scale = 1.0f;

        // Calculate scale to fit within dimension limits
        if (originalWidth > MAX_DIMENSION)
        {
            scale = System.Math.Min(scale, MAX_DIMENSION / originalWidth);
        }
        if (originalHeight > MAX_DIMENSION)
        {
            scale = System.Math.Min(scale, MAX_DIMENSION / originalHeight);
        }

        // Calculate scale to fit within pixel count limits
        System.Int64 totalPixels = (System.Int64)(originalWidth * originalHeight);
        if (totalPixels > MAX_PIXELS)
        {
            System.Single pixelScale = System.MathF.Sqrt((System.Single)MAX_PIXELS / totalPixels);
            scale = System.Math.Min(scale, pixelScale);
        }

        System.Int32 imageWidth = (System.Int32)(originalWidth * scale);
        System.Int32 imageHeight = (System.Int32)(originalHeight * scale);

        if (imageWidth <= 0 || imageHeight <= 0)
        {
            throw new System.InvalidOperationException($"Invalid image dimensions after scaling: {imageWidth}x{imageHeight}");
        }

        using var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));
        if (surface == null)
        {
            throw new System.InvalidOperationException($"Failed to create surface with dimensions {imageWidth}x{imageHeight}. This may be due to insufficient memory or graphics limitations.");
        }

        var canvas = surface.Canvas;

        // Clear background
        canvas.Clear(BackgroundColor);

        // Save and apply scaling and translation
        canvas.Save();
        canvas.Scale(scale);
        canvas.Translate(margin - bounds.MinX, margin - bounds.MinY);

        // Draw connections first (so they appear behind nodes)
        DrawConnections(canvas, rootLayout);

        // Draw nodes
        DrawNodes(canvas, rootLayout);

        canvas.Restore();

        // Save to file
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    private void DrawConnections(SKCanvas canvas, LayoutNode node)
    {
        using var paint = new SKPaint
        {
            Color = LineColor,
            StrokeWidth = LineStrokeWidth,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        foreach (var child in node.Children)
        {
            // Draw line from parent center to child center
            System.Single parentCenterX = node.X + (node.Width / 2.0f);
            System.Single parentCenterY = node.Y + (node.Height / 2.0f);
            System.Single childCenterX = child.X + (child.Width / 2.0f);
            System.Single childCenterY = child.Y + (child.Height / 2.0f);

            canvas.DrawLine(parentCenterX, parentCenterY, childCenterX, childCenterY, paint);

            // Recursively draw connections for children
            DrawConnections(canvas, child);
        }
    }

    private void DrawNodes(SKCanvas canvas, LayoutNode node)
    {
        // Draw node rectangle
        using var fillPaint = new SKPaint
        {
            Color = NodeFillColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var strokePaint = new SKPaint
        {
            Color = NodeStrokeColor,
            StrokeWidth = NodeStrokeWidth,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        var rect = new SKRect(node.X, node.Y, node.X + node.Width, node.Y + node.Height);
        using var roundRect = new SKRoundRect(rect, CornerRadius, CornerRadius);

        canvas.DrawRoundRect(roundRect, fillPaint);
        canvas.DrawRoundRect(roundRect, strokePaint);

        // Draw node text (value)
        using var textPaint = new SKPaint
        {
            Color = TextColor,
            TextSize = DefaultFontSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial")
        };

        System.Single textX = node.X + (node.Width / 2.0f);
        System.Single textY = node.Y + (node.Height / 2.0f) + (DefaultFontSize / 3.0f); // Adjust for baseline

        canvas.DrawText(node.Value.ToString(), textX, textY, textPaint);

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawNodes(canvas, child);
        }
    }

    private (System.Single MinX, System.Single MinY, System.Single Width, System.Single Height) CalculateBounds(LayoutNode rootLayout)
    {
        var allNodes = GetAllNodes(rootLayout);

        if (!allNodes.Any())
            return (0, 0, 0, 0);

        System.Single minX = allNodes.Min(n => n.X);
        System.Single maxX = allNodes.Max(n => n.X + n.Width);
        System.Single minY = allNodes.Min(n => n.Y);
        System.Single maxY = allNodes.Max(n => n.Y + n.Height);

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
