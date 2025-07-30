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
    public String? RadialPngFile { get; set; }
    public NodeStyle AngularNodeStyle { get; set; } = NodeStyle.Circle;
    public NodeStyle RadialNodeStyle { get; set; } = NodeStyle.Circle;
    public AngularVisualizationConfig AngularConfig { get; set; } = new();
    public Int32 MaxSequences { get; set; } = 1000;
    public Boolean ShowHelp { get; set; }
}
