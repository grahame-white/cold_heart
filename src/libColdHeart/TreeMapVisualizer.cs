using System.IO;
using System.Threading.Tasks;

namespace libColdHeart;

public class TreeMapVisualizer
{
    private readonly TreeLayoutCalculator _layoutCalculator;
    private readonly SvgExporter _svgExporter;
    private readonly PngExporter _pngExporter;

    public TreeMapVisualizer()
    {
        _layoutCalculator = new TreeLayoutCalculator();
        _svgExporter = new SvgExporter();
        _pngExporter = new PngExporter();
    }

    public LayoutNode CalculateLayout(TreeNode root)
    {
        return _layoutCalculator.CalculateLayout(root);
    }

    public async Task ExportToSvgAsync(TreeNode root, System.String filePath)
    {
        var layout = CalculateLayout(root);
        var svg = _svgExporter.ExportToSvg(layout);
        await File.WriteAllTextAsync(filePath, svg);
    }

    public void ExportToPng(TreeNode root, System.String filePath)
    {
        var layout = CalculateLayout(root);
        _pngExporter.ExportToPng(layout, filePath);
    }

    public async Task ExportToSvgAsync(LayoutNode layout, System.String filePath)
    {
        var svg = _svgExporter.ExportToSvg(layout);
        await File.WriteAllTextAsync(filePath, svg);
    }

    public void ExportToPng(LayoutNode layout, System.String filePath)
    {
        _pngExporter.ExportToPng(layout, filePath);
    }
}
