using System;
using libColdHeart;

namespace ColdHeart;

public class CommandLineOptions
{
    public String? LoadFile { get; set; }
    public String? SaveFile { get; set; }
    public String? SvgFile { get; set; }
    public String? PngFile { get; set; }
    public String? AngularPngFile { get; set; }
    public NodeStyle AngularNodeStyle { get; set; } = NodeStyle.Circle;
    public AngularVisualizationConfig AngularConfig { get; set; } = new();
    public Boolean ShowHelp { get; set; }
}