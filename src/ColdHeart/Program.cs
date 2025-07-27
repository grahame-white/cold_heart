using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using libColdHeart;

namespace ColdHeart;

internal class Program
{
    private const Int32 UPPER_LIMIT = 1000;

    static async Task<Int32> Main(String[] args)
    {
        try
        {
            SequenceGenerator generator;
            String? loadFile = null;
            String? saveFile = null;
            String? svgFile = null;
            String? pngFile = null;
            String? angularPngFile = null;
            NodeStyle angularNodeStyle = NodeStyle.Circle; // Default to circle nodes
            var angularConfig = new AngularVisualizationConfig();

            // Parse command line arguments
            for (Int32 i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--load":
                        if (i + 1 < args.Length)
                        {
                            loadFile = args[++i];
                        }
                        else
                        {
                            Console.WriteLine("Error: --load requires a filename");
                            return 1;
                        }
                        break;
                    case "--save":
                        if (i + 1 < args.Length)
                        {
                            saveFile = args[++i];
                        }
                        else
                        {
                            Console.WriteLine("Error: --save requires a filename");
                            return 1;
                        }
                        break;
                    case "--svg":
                        if (i + 1 < args.Length)
                        {
                            svgFile = args[++i];
                        }
                        else
                        {
                            Console.WriteLine("Error: --svg requires a filename");
                            return 1;
                        }
                        break;
                    case "--png":
                        if (i + 1 < args.Length)
                        {
                            pngFile = args[++i];
                        }
                        else
                        {
                            Console.WriteLine("Error: --png requires a filename");
                            return 1;
                        }
                        break;
                    case "--png-angular":
                        if (i + 1 < args.Length)
                        {
                            angularPngFile = args[++i];
                        }
                        else
                        {
                            Console.WriteLine("Error: --png-angular requires a filename");
                            return 1;
                        }
                        break;
                    case "--angular-node-style":
                        if (i + 1 < args.Length)
                        {
                            var styleArg = args[++i].ToLowerInvariant();
                            switch (styleArg)
                            {
                                case "circle":
                                    angularNodeStyle = NodeStyle.Circle;
                                    break;
                                case "rectangle":
                                    angularNodeStyle = NodeStyle.Rectangle;
                                    break;
                                default:
                                    Console.WriteLine($"Error: Invalid node style '{args[i]}'. Valid options are: circle, rectangle");
                                    return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: --angular-node-style requires a style (circle or rectangle)");
                            return 1;
                        }
                        break;
                    case "--angular-left-turn":
                        if (i + 1 < args.Length)
                        {
                            if (Single.TryParse(args[++i], out var leftTurn))
                            {
                                angularConfig.LeftTurnAngle = leftTurn;
                            }
                            else
                            {
                                Console.WriteLine($"Error: Invalid left turn angle '{args[i]}'. Must be a number.");
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: --angular-left-turn requires an angle value");
                            return 1;
                        }
                        break;
                    case "--angular-right-turn":
                        if (i + 1 < args.Length)
                        {
                            if (Single.TryParse(args[++i], out var rightTurn))
                            {
                                angularConfig.RightTurnAngle = rightTurn;
                            }
                            else
                            {
                                Console.WriteLine($"Error: Invalid right turn angle '{args[i]}'. Must be a number.");
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: --angular-right-turn requires an angle value");
                            return 1;
                        }
                        break;
                    case "--angular-thickness-impact":
                        if (i + 1 < args.Length)
                        {
                            if (Single.TryParse(args[++i], out var thicknessImpact))
                            {
                                angularConfig.ThicknessImpact = thicknessImpact;
                            }
                            else
                            {
                                Console.WriteLine($"Error: Invalid thickness impact '{args[i]}'. Must be a number.");
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: --angular-thickness-impact requires a numeric value");
                            return 1;
                        }
                        break;
                    case "--angular-color-impact":
                        if (i + 1 < args.Length)
                        {
                            if (Single.TryParse(args[++i], out var colorImpact))
                            {
                                angularConfig.ColorImpact = colorImpact;
                            }
                            else
                            {
                                Console.WriteLine($"Error: Invalid color impact '{args[i]}'. Must be a number.");
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: --angular-color-impact requires a numeric value");
                            return 1;
                        }
                        break;
                    case "--angular-render-longest":
                        if (i + 1 < args.Length)
                        {
                            if (Int32.TryParse(args[++i], out var longestCount) && longestCount > 0)
                            {
                                angularConfig.RenderLongestPaths = longestCount;
                            }
                            else
                            {
                                Console.WriteLine($"Error: Invalid longest paths count '{args[i]}'. Must be a positive integer.");
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: --angular-render-longest requires a positive integer");
                            return 1;
                        }
                        break;
                    case "--angular-render-most-traversed":
                        if (i + 1 < args.Length)
                        {
                            if (Int32.TryParse(args[++i], out var traversedCount) && traversedCount > 0)
                            {
                                angularConfig.RenderMostTraversedPaths = traversedCount;
                            }
                            else
                            {
                                Console.WriteLine($"Error: Invalid most traversed paths count '{args[i]}'. Must be a positive integer.");
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: --angular-render-most-traversed requires a positive integer");
                            return 1;
                        }
                        break;
                    case "--angular-render-random":
                        if (i + 1 < args.Length)
                        {
                            if (Int32.TryParse(args[++i], out var randomCount) && randomCount > 0)
                            {
                                angularConfig.RenderRandomPaths = randomCount;
                            }
                            else
                            {
                                Console.WriteLine($"Error: Invalid random paths count '{args[i]}'. Must be a positive integer.");
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: --angular-render-random requires a positive integer");
                            return 1;
                        }
                        break;
                    case "--help":
                    case "-h":
                        PrintUsage();
                        return 0;
                    default:
                        Console.WriteLine($"Error: Unknown argument '{args[i]}'");
                        PrintUsage();
                        return 1;
                }
            }

            // Load from file if specified
            if (loadFile != null && !File.Exists(loadFile))
            {
                Console.WriteLine($"Error: File '{loadFile}' does not exist");
                return 1;
            }

            if (loadFile != null)
            {
                Console.WriteLine($"Loading sequence from '{loadFile}'...");
                generator = await SequenceGenerator.LoadFromFileAsync(loadFile);
                Console.WriteLine("Sequence loaded successfully.");
            }
            else
            {
                // Generate sequence
                Console.WriteLine($"Generating sequence for numbers 1 to {UPPER_LIMIT - 1}...");
                generator = new SequenceGenerator();
                for (BigInteger i = 1; i < UPPER_LIMIT; i++)
                {
                    generator.Add(i);
                }
                Console.WriteLine("Sequence generation completed.");
            }

            // Save to file if specified
            if (saveFile != null)
            {
                Console.WriteLine($"Saving sequence to '{saveFile}'...");
                await generator.SaveToFileAsync(saveFile);
                Console.WriteLine("Sequence saved successfully.");
            }

            // Export visualizations if specified
            if (svgFile != null || pngFile != null || angularPngFile != null)
            {
                // Validate angular configuration if angular PNG is requested
                if (angularPngFile != null)
                {
                    try
                    {
                        angularConfig.Validate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: Invalid angular visualization configuration - {ex.Message}");
                        return 1;
                    }
                }

                var visualizer = new TreeMapVisualizer();

                if (svgFile != null)
                {
                    Console.WriteLine($"Exporting tree visualization to SVG '{svgFile}'...");
                    await visualizer.ExportToSvgAsync(generator.Root, svgFile);
                    Console.WriteLine("SVG export completed successfully.");
                }

                if (pngFile != null)
                {
                    Console.WriteLine($"Exporting tree visualization to PNG '{pngFile}'...");
                    visualizer.ExportToPng(generator.Root, pngFile);
                    Console.WriteLine("PNG export completed successfully.");
                }

                if (angularPngFile != null)
                {
                    Console.WriteLine($"Exporting angular tree visualization to PNG '{angularPngFile}'...");
                    visualizer.ExportToAngularPng(generator.Root, angularPngFile, angularConfig, angularNodeStyle, progress =>
                    {
                        Console.WriteLine($"  {progress}");
                    });
                    Console.WriteLine("Angular PNG export completed successfully.");
                }
            }

            return 0;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: File not found - {ex.Message}");
            return 1;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Error: Access denied - {ex.Message}");
            return 1;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error: Invalid JSON format - {ex.Message}");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error: Operation failed - {ex.Message}");
            return 1;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error: I/O operation failed - {ex.Message}");
            return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("ColdHeart - Collatz sequence generator and serializer");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  ColdHeart [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --load <filename>         Load a previously serialized sequence from file");
        Console.WriteLine("  --save <filename>         Save the generated sequence to file");
        Console.WriteLine("  --svg <filename>          Export tree visualization to SVG format");
        Console.WriteLine("  --png <filename>          Export tree visualization to PNG format (traditional layout)");
        Console.WriteLine("  --png-angular <filename>  Export tree visualization to PNG format (angular layout)");
        Console.WriteLine("  --angular-node-style <style>  Node style for angular PNG (circle or rectangle, default: circle)");
        Console.WriteLine("  --angular-left-turn <angle>   Left turn angle for even nodes (default: -8.65)");
        Console.WriteLine("  --angular-right-turn <angle>  Right turn angle for odd nodes (default: 16.0)");
        Console.WriteLine("  --angular-thickness-impact <factor>  Impact of traversals on line thickness (default: 1.0, 0 = no impact)");
        Console.WriteLine("  --angular-color-impact <factor>      Impact of path length on color (default: 1.0, higher = greater change)");
        Console.WriteLine("  --angular-render-longest <count>     Render only top N longest paths");
        Console.WriteLine("  --angular-render-most-traversed <count>  Render only top N most traversed paths");
        Console.WriteLine("  --angular-render-random <count>      Render only N random paths from the entire set");
        Console.WriteLine("  --help, -h                Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ColdHeart --save sequence.json");
        Console.WriteLine("  ColdHeart --load sequence.json --svg tree.svg");
        Console.WriteLine("  ColdHeart --svg tree.svg --png tree.png");
        Console.WriteLine("  ColdHeart --png-angular angular_tree.png");
        Console.WriteLine("  ColdHeart --png-angular angular_tree.png --angular-node-style rectangle");
        Console.WriteLine("  ColdHeart --load old.json --save new.json --svg tree.svg");
    }
}
