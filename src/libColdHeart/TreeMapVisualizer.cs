using System;
using System.IO;
using System.Threading.Tasks;

namespace libColdHeart;

public class TreeMapVisualizer
{
    private readonly TreeLayoutCalculator _layoutCalculator;
    private readonly AngularTreeLayoutCalculator _angularLayoutCalculator;
    private readonly SvgExporter _svgExporter;
    private readonly PngExporter _pngExporter;
    private readonly EnhancedPngExporter _enhancedPngExporter;

    public TreeMapVisualizer()
    {
        _layoutCalculator = new TreeLayoutCalculator();
        _angularLayoutCalculator = new AngularTreeLayoutCalculator();
        _svgExporter = new SvgExporter();
        _pngExporter = new PngExporter();
        _enhancedPngExporter = new EnhancedPngExporter();
    }

    public LayoutNode CalculateLayout(TreeNode root)
    {
        return _layoutCalculator.CalculateLayout(root);
    }

    public LayoutNode CalculateAngularLayout(TreeNode root)
    {
        return _angularLayoutCalculator.CalculateLayout(root);
    }

    public async Task ExportToSvgAsync(TreeNode root, String filePath)
    {
        var layout = CalculateLayout(root);
        var svg = _svgExporter.ExportToSvg(layout);
        await File.WriteAllTextAsync(filePath, svg);
    }

    public void ExportToPng(TreeNode root, String filePath)
    {
        var layout = CalculateLayout(root);
        _pngExporter.ExportToPng(layout, filePath);
    }

    public void ExportToAngularPng(TreeNode root, String filePath)
    {
        var layout = CalculateAngularLayout(root);
        var metrics = new AngularTreeLayoutCalculator().CalculateTreeMetrics(root);
        _enhancedPngExporter.ExportToPng(layout, metrics, filePath);
    }

    public async Task ExportToSvgAsync(LayoutNode layout, String filePath)
    {
        var svg = _svgExporter.ExportToSvg(layout);
        await File.WriteAllTextAsync(filePath, svg);
    }

    public void ExportToPng(LayoutNode layout, String filePath)
    {
        // For legacy compatibility - use traditional PNG export when TreeMetrics are not available
        _pngExporter.ExportToPng(layout, filePath);
    }

    public void ExportEnhancedToPng(LayoutNode layout, TreeMetrics metrics, String filePath)
    {
        _enhancedPngExporter.ExportToPng(layout, metrics, filePath);
    }
}
