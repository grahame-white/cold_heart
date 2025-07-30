using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using libColdHeart;

namespace ColdHeart;

internal class Program
{
    static async Task<Int32> Main(String[] args)
    {
        try
        {
            var parser = new CommandLineParser();
            CommandLineOptions options;

            try
            {
                options = parser.Parse(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                parser.PrintUsage();
                return 1;
            }

            if (options.ShowHelp)
            {
                parser.PrintUsage();
                return 0;
            }

            SequenceGenerator generator;

            // Load from file if specified
            if (options.LoadFile != null && !File.Exists(options.LoadFile))
            {
                Console.WriteLine($"Error: File '{options.LoadFile}' does not exist");
                return 1;
            }

            if (options.LoadFile != null)
            {
                Console.WriteLine($"Loading sequence from '{options.LoadFile}'...");
                generator = await SequenceGenerator.LoadFromFileAsync(options.LoadFile);
                Console.WriteLine("Sequence loaded successfully.");
            }
            else
            {
                // Generate sequence
                Console.WriteLine($"Generating sequence for numbers 1 to {options.MaxSequences - 1}...");
                generator = new SequenceGenerator();
                for (BigInteger i = 1; i < options.MaxSequences; i++)
                {
                    generator.Add(i);
                }
                Console.WriteLine("Sequence generation completed.");
            }

            // Save to file if specified
            if (options.SaveFile != null)
            {
                Console.WriteLine($"Saving sequence to '{options.SaveFile}'...");
                await generator.SaveToFileAsync(options.SaveFile);
                Console.WriteLine("Sequence saved successfully.");
            }

            // Export visualizations if specified
            if (options.SvgFile != null || options.PngFile != null || options.AngularPngFile != null || options.RadialPngFile != null)
            {
                // Validate angular configuration if angular PNG is requested
                if (options.AngularPngFile != null)
                {
                    try
                    {
                        options.AngularConfig.Validate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: Invalid angular visualization configuration - {ex.Message}");
                        return 1;
                    }
                }

                var visualizer = new TreeMapVisualizer();

                if (options.SvgFile != null)
                {
                    Console.WriteLine($"Exporting tree visualization to SVG '{options.SvgFile}'...");
                    await visualizer.ExportToSvgAsync(generator.Root, options.SvgFile);
                    Console.WriteLine("SVG export completed successfully.");
                }

                if (options.PngFile != null)
                {
                    Console.WriteLine($"Exporting tree visualization to PNG '{options.PngFile}'...");
                    visualizer.ExportToPng(generator.Root, options.PngFile);
                    Console.WriteLine("PNG export completed successfully.");
                }

                if (options.AngularPngFile != null)
                {
                    Console.WriteLine($"Exporting angular tree visualization to PNG '{options.AngularPngFile}'...");
                    visualizer.ExportToAngularPng(generator.Root, options.AngularPngFile, options.AngularConfig, options.AngularNodeStyle, progress =>
                    {
                        Console.WriteLine($"  {progress}");
                    });
                    Console.WriteLine("Angular PNG export completed successfully.");
                }

                if (options.RadialPngFile != null)
                {
                    Console.WriteLine($"Exporting radial tree visualization to PNG '{options.RadialPngFile}'...");
                    visualizer.ExportToRadialPng(generator.Root, options.RadialPngFile, options.RadialNodeStyle, progress =>
                    {
                        Console.WriteLine($"  {progress}");
                    });
                    Console.WriteLine("Radial PNG export completed successfully.");
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
}
