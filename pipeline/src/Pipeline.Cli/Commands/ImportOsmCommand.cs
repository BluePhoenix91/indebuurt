using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Pipeline.Core.Services;

namespace Pipeline.Cli.Commands;

/// <summary>
/// CLI command to import POIs from OpenStreetMap via Overpass API.
/// </summary>
public static class ImportOsmCommand
{
    public static Command Create(IServiceProvider services)
    {
        var domainOption = new Option<string?>("--domain")
        {
            Description = "Import only a specific domain (pets, shopping, healthcare, education, transport, green)"
        };

        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Show what would be imported without writing to database"
        };

        var forceOption = new Option<bool>("--force")
        {
            Description = "Skip count validation warnings and proceed anyway"
        };

        var command = new Command("import-osm", "Import POIs from OpenStreetMap via Overpass API")
        {
            domainOption,
            dryRunOption,
            forceOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var domain = parseResult.GetValue(domainOption);
            var dryRun = parseResult.GetValue(dryRunOption);
            var force = parseResult.GetValue(forceOption);

            return await ExecuteAsync(services, domain, dryRun, force, cancellationToken);
        });

        return command;
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider services,
        string? domain,
        bool dryRun,
        bool force,
        CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine("Starting OSM POI import from Overpass API...");
        Console.WriteLine("Bbox: 50.68,2.54,51.51,5.92 (Flanders + Brussels)");
        Console.WriteLine();

        if (dryRun)
        {
            Console.WriteLine("  [DRY RUN] No changes will be written to database");
            Console.WriteLine();
        }

        try
        {
            using var scope = services.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IPoiImportService>();

            // Validate domain if specified
            IEnumerable<string>? domains = null;
            if (!string.IsNullOrEmpty(domain))
            {
                if (!importService.AvailableDomains.Contains(domain))
                {
                    Console.WriteLine($"Error: Unknown domain '{domain}'");
                    Console.WriteLine();
                    Console.WriteLine($"Available domains: {string.Join(", ", importService.AvailableDomains)}");
                    return 1;
                }
                domains = [domain];
                Console.WriteLine($"  Importing domain: {domain}");
                Console.WriteLine();
            }

            var progress = new Progress<string>(message => Console.WriteLine($"  {message}"));

            Console.WriteLine("Fetching domains:");
            var result = await importService.ImportAsync(domains, dryRun, force, progress, cancellationToken);

            Console.WriteLine();
            Console.WriteLine("Summary:");
            Console.WriteLine($"  Total POIs: {result.TotalImported:N0} (was {result.PreviousCount:N0}, {FormatDelta(result.TotalImported - result.PreviousCount)})");
            Console.WriteLine();
            Console.WriteLine("  By category:");

            // Get all categories from both old and new
            var allCategories = result.CountsByCategory.Keys
                .Union(result.PreviousCountsByCategory.Keys)
                .OrderBy(c => c);

            foreach (var category in allCategories)
            {
                var newCount = result.CountsByCategory.GetValueOrDefault(category, 0);
                var oldCount = result.PreviousCountsByCategory.GetValueOrDefault(category, 0);
                var delta = newCount - oldCount;
                Console.WriteLine($"    {category,-15} {newCount,8:N0} (was {oldCount,8:N0}, {FormatDelta(delta)})");
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
                Console.WriteLine("Reminder: Run 'refresh-views' to update materialized views.");
            }

            return 0;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: Failed to connect to Overpass API");
            Console.WriteLine($"  {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Hint: Check your internet connection and try again later.");
            Console.WriteLine("      Overpass API may be temporarily unavailable.");
            return 1;
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: Overpass API request timed out");
            Console.WriteLine($"  {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Hint: The Overpass API may be under heavy load.");
            Console.WriteLine("      Try again later or increase TimeoutSeconds in appsettings.json.");
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

    private static string FormatDelta(int delta)
    {
        if (delta > 0) return $"+{delta:N0}";
        if (delta < 0) return $"{delta:N0}";
        return "no change";
    }
}
