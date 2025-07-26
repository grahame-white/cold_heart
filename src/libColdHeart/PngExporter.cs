using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace libColdHeart;

public class PngExporter
{
    private const Single DefaultFontSize = 12.0f;
    private const Single NodeStrokeWidth = 2.0f;
    private const Single LineStrokeWidth = 2.0f;
    private const Single CornerRadius = 5.0f;

    private static readonly SKColor NodeFillColor = SKColor.Parse("#e6f3ff");
    private static readonly SKColor NodeStrokeColor = SKColor.Parse("#0066cc");
    private static readonly SKColor TextColor = SKColor.Parse("#000000");
    private static readonly SKColor LineColor = SKColor.Parse("#666666");
    private static readonly SKColor BackgroundColor = SKColor.Parse("#ffffff");

    public void ExportToPng(LayoutNode rootLayout, String filePath)
    {
        var bounds = CalculateBounds(rootLayout);

        // Add margins
        Single margin = 20.0f;
        Int32 imageWidth = (Int32)(bounds.Width + (2 * margin));
        Int32 imageHeight = (Int32)(bounds.Height + (2 * margin));

        using var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));
        var canvas = surface.Canvas;

        // Clear background
        canvas.Clear(BackgroundColor);

        // Save and translate canvas to account for margin and bounds
        canvas.Save();
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
            Single parentCenterX = node.X + (node.Width / 2.0f);
            Single parentCenterY = node.Y + (node.Height / 2.0f);
            Single childCenterX = child.X + (child.Width / 2.0f);
            Single childCenterY = child.Y + (child.Height / 2.0f);

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

        Single textX = node.X + (node.Width / 2.0f);
        Single textY = node.Y + (node.Height / 2.0f) + (DefaultFontSize / 3.0f); // Adjust for baseline

        canvas.DrawText(node.Value.ToString(), textX, textY, textPaint);

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawNodes(canvas, child);
        }
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
