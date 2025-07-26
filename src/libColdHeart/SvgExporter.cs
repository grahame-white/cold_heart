using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libColdHeart;

public class SvgExporter
{
    private const Int32 DefaultFontSize = 12;
    private const String DefaultFontFamily = "Arial, sans-serif";
    private const String NodeFillColor = "#e6f3ff";
    private const String NodeStrokeColor = "#0066cc";
    private const String TextColor = "#000000";
    private const String LineColor = "#666666";

    public String ExportToSvg(LayoutNode rootLayout)
    {
        var bounds = CalculateBounds(rootLayout);
        var sb = new StringBuilder();

        // Add margins
        Single margin = 20.0f;
        Single svgWidth = bounds.Width + (2 * margin);
        Single svgHeight = bounds.Height + (2 * margin);

        // SVG header
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine($"<svg width=\"{svgWidth}\" height=\"{svgHeight}\" xmlns=\"http://www.w3.org/2000/svg\">");

        // Transform to move origin and account for margin
        sb.AppendLine($"<g transform=\"translate({margin - bounds.MinX}, {margin - bounds.MinY})\">");

        // Draw connections first (so they appear behind nodes)
        DrawConnections(sb, rootLayout);

        // Draw nodes
        DrawNodes(sb, rootLayout);

        sb.AppendLine("</g>");
        sb.AppendLine("</svg>");

        return sb.ToString();
    }

    private void DrawConnections(StringBuilder sb, LayoutNode node)
    {
        foreach (var child in node.Children)
        {
            // Draw line from parent center to child center
            Single parentCenterX = node.X + (node.Width / 2.0f);
            Single parentCenterY = node.Y + (node.Height / 2.0f);
            Single childCenterX = child.X + (child.Width / 2.0f);
            Single childCenterY = child.Y + (child.Height / 2.0f);

            sb.AppendLine($"<line x1=\"{parentCenterX}\" y1=\"{parentCenterY}\" " +
                $"x2=\"{childCenterX}\" y2=\"{childCenterY}\" " +
                $"stroke=\"{LineColor}\" stroke-width=\"2\"/>");

            // Recursively draw connections for children
            DrawConnections(sb, child);
        }
    }

    private void DrawNodes(StringBuilder sb, LayoutNode node)
    {
        // Draw node rectangle
        sb.AppendLine($"<rect x=\"{node.X}\" y=\"{node.Y}\" " +
            $"width=\"{node.Width}\" height=\"{node.Height}\" " +
            $"fill=\"{NodeFillColor}\" stroke=\"{NodeStrokeColor}\" stroke-width=\"2\" rx=\"5\"/>");

        // Draw node text (value)
        Single textX = node.X + (node.Width / 2.0f);
        Single textY = node.Y + (node.Height / 2.0f) + (DefaultFontSize / 3.0f); // Adjust for baseline

        sb.AppendLine($"<text x=\"{textX}\" y=\"{textY}\" " +
            $"font-family=\"{DefaultFontFamily}\" font-size=\"{DefaultFontSize}\" " +
            $"fill=\"{TextColor}\" text-anchor=\"middle\">{node.Value}</text>");

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawNodes(sb, child);
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
