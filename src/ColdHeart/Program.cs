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
                    visualizer.ExportToAngularPng(generator.Root, angularPngFile, progress =>
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
        Console.WriteLine("  --load <filename>       Load a previously serialized sequence from file");
        Console.WriteLine("  --save <filename>       Save the generated sequence to file");
        Console.WriteLine("  --svg <filename>        Export tree visualization to SVG format");
        Console.WriteLine("  --png <filename>        Export tree visualization to PNG format (traditional layout)");
        Console.WriteLine("  --png-angular <filename> Export tree visualization to PNG format (angular layout)");
        Console.WriteLine("  --help, -h              Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ColdHeart --save sequence.json");
        Console.WriteLine("  ColdHeart --load sequence.json --svg tree.svg");
        Console.WriteLine("  ColdHeart --svg tree.svg --png tree.png");
        Console.WriteLine("  ColdHeart --png-angular angular_tree.png");
        Console.WriteLine("  ColdHeart --load old.json --save new.json --svg tree.svg");
    }
}
