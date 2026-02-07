using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Pipeline.Core.Services.Statbel;

namespace Pipeline.Cli.Commands;

/// <summary>
/// CLI command to import statistics from Statbel (Belgian Statistics Office).
/// </summary>
public static class ImportStatbelCommand
{
    public static Command Create(IServiceProvider services)
    {
        var yearOption = new Option<int?>("--year")
        {
            Description = "Target year (auto-detects latest if not specified)"
        };

        var datasetOption = new Option<string?>("--dataset")
        {
            Description = "Import only a specific dataset: population, house-prices"
        };

        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Show what would be imported without writing to database"
        };

        var command = new Command("import-statbel", "Import statistics from Statbel (population, house prices)")
        {
            yearOption,
            datasetOption,
            dryRunOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var year = parseResult.GetValue(yearOption);
            var dataset = parseResult.GetValue(datasetOption);
            var dryRun = parseResult.GetValue(dryRunOption);

            return await ExecuteAsync(services, year, dataset, dryRun, cancellationToken);
        });

        return command;
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider services,
        int? year,
        string? dataset,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine("Starting Statbel statistics import...");
        Console.WriteLine();

        if (dryRun)
        {
            Console.WriteLine("  [DRY RUN] No changes will be written to database");
            Console.WriteLine();
        }

        try
        {
            using var scope = services.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IStatbelImportService>();

            // Validate dataset if specified
            if (!string.IsNullOrEmpty(dataset))
            {
                if (!importService.AvailableDatasets.Contains(dataset))
                {
                    Console.WriteLine($"Error: Unknown dataset '{dataset}'");
                    Console.WriteLine();
                    Console.WriteLine($"Available datasets: {string.Join(", ", importService.AvailableDatasets)}");
                    return 1;
                }
                Console.WriteLine($"  Importing dataset: {dataset}");
                Console.WriteLine();
            }

            var progress = new Progress<string>(message => Console.WriteLine($"  {message}"));

            var result = await importService.ImportAsync(year, dataset, dryRun, progress, cancellationToken);

            // Summary
            Console.WriteLine();
            Console.WriteLine("=" + new string('=', 59));
            Console.WriteLine($"Summary for year {result.Year}:");
            Console.WriteLine("=" + new string('=', 59));

            if (result.PopulationResult != null)
            {
                Console.WriteLine();
                Console.WriteLine("Population:");
                Console.WriteLine($"  Rows processed:       {result.PopulationResult.RowsProcessed,8:N0}");
                Console.WriteLine($"  Neighborhoods updated:{result.PopulationResult.NeighborhoodsUpdated,8:N0}");
                if (result.PopulationResult.NeighborhoodsSkipped > 0)
                {
                    Console.WriteLine($"  Neighborhoods skipped:{result.PopulationResult.NeighborhoodsSkipped,8:N0}");
                }
            }

            if (result.HousePriceResult != null)
            {
                Console.WriteLine();
                Console.WriteLine("House Prices:");
                Console.WriteLine($"  Municipalities processed: {result.HousePriceResult.RowsProcessed,5:N0}");
                Console.WriteLine($"  Neighborhoods updated:    {result.HousePriceResult.NeighborhoodsUpdated,5:N0}");
                if (result.HousePriceResult.NeighborhoodsSkipped > 0)
                {
                    Console.WriteLine($"  Municipalities skipped:   {result.HousePriceResult.NeighborhoodsSkipped,5:N0}");
                }
            }

            if (result.HasWarnings)
            {
                Console.WriteLine();
                Console.WriteLine("Warnings:");
                foreach (var warning in result.AllWarnings)
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
                Console.WriteLine("Reminder: Run 'refresh-views' to update materialized views.");
            }

            return 0;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: Failed to download from Statbel");
            Console.WriteLine($"  {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Hint: Check your internet connection and try again later.");
            Console.WriteLine("      Statbel servers may be temporarily unavailable.");
            return 1;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Could not detect"))
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Hint: Try specifying the year explicitly with --year 2024");
            return 1;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Could not find"))
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Hint: The file format may have changed. Check the Statbel downloads.");
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
