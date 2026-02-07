using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Pipeline.Core.Services.Boundaries;

namespace Pipeline.Cli.Commands;

/// <summary>
/// CLI command to import neighborhood boundaries from Statbel statistical sectors GeoJSON.
/// </summary>
public static class ImportBoundariesCommand
{
    public static Command Create(IServiceProvider services)
    {
        var fileOption = new Option<string?>("--file")
        {
            Description = "Path to GeoJSON file (downloads from Statbel if not specified)"
        };

        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Show what would be imported without writing to database"
        };

        var command = new Command("import-boundaries",
            "Import neighborhood boundaries from Statbel statistical sectors GeoJSON")
        {
            fileOption,
            dryRunOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var file = parseResult.GetValue(fileOption);
            var dryRun = parseResult.GetValue(dryRunOption);

            return await ExecuteAsync(services, file, dryRun, cancellationToken);
        });

        return command;
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider services,
        string? file,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine("Starting boundary import...");
        Console.WriteLine();

        if (dryRun)
        {
            Console.WriteLine("  [DRY RUN] No changes will be written to database");
            Console.WriteLine();
        }

        try
        {
            using var scope = services.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IBoundaryImportService>();

            var progress = new Progress<string>(message => Console.WriteLine($"  {message}"));

            var result = await importService.ImportAsync(file, dryRun, progress, cancellationToken);

            // Summary
            Console.WriteLine();
            Console.WriteLine("=" + new string('=', 59));
            Console.WriteLine("Summary:");
            Console.WriteLine("=" + new string('=', 59));
            Console.WriteLine();

            Console.WriteLine($"  GeoJSON features:     {result.TotalFeaturesInFile,8:N0}");
            Console.WriteLine($"  After filter:         {result.SectorsAfterFilter,8:N0}  (Flanders + Brussels)");

            if (!dryRun)
            {
                Console.WriteLine($"  Neighborhoods created:{result.NeighborhoodsCreated,8:N0}  (was {result.PreviousNeighborhoodCount:N0})");
                Console.WriteLine($"  Sectors imported:     {result.SectorsImported,8:N0}  (was {result.PreviousSectorCount:N0})");
            }
            else
            {
                Console.WriteLine($"  Expected neighborhoods:{result.NeighborhoodsCreated,8:N0}");
                Console.WriteLine($"  Previous neighborhoods:{result.PreviousNeighborhoodCount,8:N0}");
                Console.WriteLine($"  Previous sectors:      {result.PreviousSectorCount,8:N0}");
            }

            if (result.HasWarnings)
            {
                Console.WriteLine();
                Console.WriteLine("Warnings:");
                foreach (var warning in result.Warnings)
                {
                    Console.WriteLine($"  ! {warning}");
                }
            }

            Console.WriteLine();

            if (dryRun)
            {
                Console.WriteLine("[DRY RUN] No changes were written to database.");
            }
            else
            {
                Console.WriteLine("Import completed successfully.");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("  1. Run 'import-statbel' to load population and house price statistics");
                Console.WriteLine("  2. Run 'import-osm' to import POI data");
                Console.WriteLine("  3. Run 'refresh-views' to update materialized views");
            }

            return 0;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Hint: Use --file to specify a local GeoJSON file path,");
            Console.WriteLine("      or ensure internet access for automatic download from Statbel.");
            return 1;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: Failed to download from Statbel");
            Console.WriteLine($"  {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Hint: Check your internet connection and try again later.");
            Console.WriteLine("      Or use --file to specify a local GeoJSON file.");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine($"Details: {ex}");
            return 1;
        }
    }
}
