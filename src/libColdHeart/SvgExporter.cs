using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace libColdHeart;

public class SvgExporter
{
    private const int DefaultFontSize = 12;
    private const string DefaultFontFamily = "Arial, sans-serif";
    private const string NodeFillColor = "#e6f3ff";
    private const string NodeStrokeColor = "#0066cc";
    private const string TextColor = "#000000";
    private const string LineColor = "#666666";

    public string ExportToSvg(LayoutNode rootLayout)
    {
        var bounds = CalculateBounds(rootLayout);
        var sb = new StringBuilder();

        // Add margins
        float margin = 20.0f;
        float svgWidth = bounds.Width + (2 * margin);
        float svgHeight = bounds.Height + (2 * margin);

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
            float parentCenterX = node.X + (node.Width / 2.0f);
            float parentCenterY = node.Y + (node.Height / 2.0f);
            float childCenterX = child.X + (child.Width / 2.0f);
            float childCenterY = child.Y + (child.Height / 2.0f);

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
        float textX = node.X + (node.Width / 2.0f);
        float textY = node.Y + (node.Height / 2.0f) + (DefaultFontSize / 3.0f); // Adjust for baseline

        sb.AppendLine($"<text x=\"{textX}\" y=\"{textY}\" " +
                     $"font-family=\"{DefaultFontFamily}\" font-size=\"{DefaultFontSize}\" " +
                     $"fill=\"{TextColor}\" text-anchor=\"middle\">{node.Value}</text>");

        // Recursively draw child nodes
        foreach (var child in node.Children)
        {
            DrawNodes(sb, child);
        }
    }

    private (float MinX, float MinY, float Width, float Height) CalculateBounds(LayoutNode rootLayout)
    {
        var allNodes = GetAllNodes(rootLayout);
        
        if (!allNodes.Any())
            return (0, 0, 0, 0);

        float minX = allNodes.Min(n => n.X);
        float maxX = allNodes.Max(n => n.X + n.Width);
        float minY = allNodes.Min(n => n.Y);
        float maxY = allNodes.Max(n => n.Y + n.Height);

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