using System;
using System.Collections.Generic;
using libColdHeart;

namespace ColdHeart;

public class CommandLineParser
{
    private static readonly Dictionary<String, String> HelpText = new()
    {
        ["--load"] = "Load a previously serialized sequence from file",
        ["--save"] = "Save the generated sequence to file",
        ["--svg"] = "Export tree visualization to SVG format",
        ["--png"] = "Export tree visualization to PNG format (traditional layout)",
        ["--png-angular"] = "Export tree visualization to PNG format (angular layout)",
        ["--angular-node-style"] = "Node style for angular PNG (circle or rectangle, default: circle)",
        ["--angular-left-turn"] = "Left turn angle for even nodes (default: -8.65)",
        ["--angular-right-turn"] = "Right turn angle for odd nodes (default: 16.0)",
        ["--angular-thickness-impact"] = "Impact of traversals on line thickness (default: 1.0, 0 = no impact)",
        ["--angular-color-impact"] = "Impact of path length on color (default: 1.0, higher = greater change)",
        ["--angular-max-line-width"] = "Maximum line width for angular visualization (default: 8.0)",
        ["--angular-render-longest"] = "Render only top N longest paths",
        ["--angular-render-most-traversed"] = "Render only top N most traversed paths",
        ["--angular-render-random"] = "Render only N random paths from the entire set",
        ["--angular-draw-order"] = "Drawing order: tree (default) or least-to-most-traversed",
        ["--sequences"] = "Maximum number of sequences to calculate (default: 1000)",
        ["--help"] = "Show this help message"
    };

    public CommandLineOptions Parse(String[] args)
    {
        var options = new CommandLineOptions();

        for (Int32 i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--load":
                    options.LoadFile = GetRequiredArgument(args, ref i, "--load requires a filename");
                    break;
                case "--save":
                    options.SaveFile = GetRequiredArgument(args, ref i, "--save requires a filename");
                    break;
                case "--svg":
                    options.SvgFile = GetRequiredArgument(args, ref i, "--svg requires a filename");
                    break;
                case "--png":
                    options.PngFile = GetRequiredArgument(args, ref i, "--png requires a filename");
                    break;
                case "--png-angular":
                    options.AngularPngFile = GetRequiredArgument(args, ref i, "--png-angular requires a filename");
                    break;
                case "--angular-node-style":
                    ParseNodeStyle(args, ref i, options);
                    break;
                case "--angular-left-turn":
                    options.AngularConfig.LeftTurnAngle = ParseFloat(args, ref i, "--angular-left-turn", "left turn angle");
                    break;
                case "--angular-right-turn":
                    options.AngularConfig.RightTurnAngle = ParseFloat(args, ref i, "--angular-right-turn", "right turn angle");
                    break;
                case "--angular-thickness-impact":
                    options.AngularConfig.ThicknessImpact = ParseFloat(args, ref i, "--angular-thickness-impact", "thickness impact");
                    break;
                case "--angular-color-impact":
                    options.AngularConfig.ColorImpact = ParseFloat(args, ref i, "--angular-color-impact", "color impact");
                    break;
                case "--angular-render-longest":
                    options.AngularConfig.RenderLongestPaths = ParsePositiveInt(args, ref i, "--angular-render-longest", "longest paths count");
                    break;
                case "--angular-render-most-traversed":
                    options.AngularConfig.RenderMostTraversedPaths = ParsePositiveInt(args, ref i, "--angular-render-most-traversed", "most traversed paths count");
                    break;
                case "--angular-render-random":
                    options.AngularConfig.RenderRandomPaths = ParsePositiveInt(args, ref i, "--angular-render-random", "random paths count");
                    break;
                case "--angular-draw-order":
                    ParseDrawingOrder(args, ref i, options);
                    break;
                case "--angular-max-line-width":
                    options.AngularConfig.MaxLineWidth = ParsePositiveFloat(args, ref i, "--angular-max-line-width", "maximum line width");
                    break;
                case "--sequences":
                    options.MaxSequences = ParsePositiveInt(args, ref i, "--sequences", "number of sequences");
                    break;
                case "--help":
                case "-h":
                    options.ShowHelp = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[i]}'");
            }
        }

        return options;
    }

    private String GetRequiredArgument(String[] args, ref Int32 index, String errorMessage)
    {
        if (index + 1 < args.Length)
        {
            return args[++index];
        }
        throw new ArgumentException($"Error: {errorMessage}");
    }

    private Single ParseFloat(String[] args, ref Int32 index, String option, String parameterName)
    {
        var value = GetRequiredArgument(args, ref index, $"{option} requires a numeric value");
        if (Single.TryParse(value, out var result))
        {
            return result;
        }
        throw new ArgumentException($"Error: Invalid {parameterName} '{value}'. Must be a number.");
    }

    private Int32 ParsePositiveInt(String[] args, ref Int32 index, String option, String parameterName)
    {
        var value = GetRequiredArgument(args, ref index, $"{option} requires a positive integer");
        if (Int32.TryParse(value, out var result) && result > 0)
        {
            return result;
        }
        throw new ArgumentException($"Error: Invalid {parameterName} '{value}'. Must be a positive integer.");
    }

    private Single ParsePositiveFloat(String[] args, ref Int32 index, String option, String parameterName)
    {
        var value = GetRequiredArgument(args, ref index, $"{option} requires a positive number");
        if (Single.TryParse(value, out var result) && result > 0)
        {
            return result;
        }
        throw new ArgumentException($"Error: Invalid {parameterName} '{value}'. Must be a positive number.");
    }

    private void ParseNodeStyle(String[] args, ref Int32 index, CommandLineOptions options)
    {
        var styleArg = GetRequiredArgument(args, ref index, "--angular-node-style requires a style (circle or rectangle)");
        switch (styleArg.ToLowerInvariant())
        {
            case "circle":
                options.AngularNodeStyle = NodeStyle.Circle;
                break;
            case "rectangle":
                options.AngularNodeStyle = NodeStyle.Rectangle;
                break;
            default:
                throw new ArgumentException($"Error: Invalid node style '{styleArg}'. Valid options are: circle, rectangle");
        }
    }

    private void ParseDrawingOrder(String[] args, ref Int32 index, CommandLineOptions options)
    {
        var orderArg = GetRequiredArgument(args, ref index, "--angular-draw-order requires an order (tree or least-to-most-traversed)");
        switch (orderArg.ToLowerInvariant())
        {
            case "tree":
                options.AngularConfig.DrawingOrder = DrawingOrder.TreeOrder;
                break;
            case "least-to-most-traversed":
                options.AngularConfig.DrawingOrder = DrawingOrder.LeastToMostTraversed;
                break;
            default:
                throw new ArgumentException($"Error: Invalid drawing order '{orderArg}'. Valid options are: tree, least-to-most-traversed");
        }
    }

    public void PrintUsage()
    {
        Console.WriteLine("ColdHeart - Collatz sequence generator and serializer");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  ColdHeart [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");

        foreach (var (option, description) in HelpText)
        {
            var spacing = option.Length < 40 ? new String(' ', 40 - option.Length) : "  ";
            Console.WriteLine($"  {option}{spacing}{description}");
        }

        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ColdHeart --save sequence.json");
        Console.WriteLine("  ColdHeart --load sequence.json --svg tree.svg");
        Console.WriteLine("  ColdHeart --svg tree.svg --png tree.png");
        Console.WriteLine("  ColdHeart --png-angular angular_tree.png");
        Console.WriteLine("  ColdHeart --png-angular angular_tree.png --angular-node-style rectangle");
        Console.WriteLine("  ColdHeart --png-angular angular_tree.png --angular-draw-order least-to-most-traversed");
        Console.WriteLine("  ColdHeart --sequences 2000 --png-angular angular_tree.png");
        Console.WriteLine("  ColdHeart --png-angular angular_tree.png --angular-max-line-width 12.0");
        Console.WriteLine("  ColdHeart --load old.json --save new.json --svg tree.svg");
    }
}
