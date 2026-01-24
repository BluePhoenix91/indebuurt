using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pipeline.Core.Data;

namespace Pipeline.Cli.Commands;

/// <summary>
/// CLI command to refresh GIS materialized views.
/// Run this after OSM data imports to update POI counts and nearest POI data.
/// </summary>
public static class RefreshViewsCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("refresh-views", "Refresh GIS materialized views for value card generation");

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            return await ExecuteAsync(services, cancellationToken);
        });

        return command;
    }

    private static async Task<int> ExecuteAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PipelineDbContext>();

        Console.WriteLine("Refreshing GIS materialized views...");
        Console.WriteLine();

        try
        {
            Console.Write("  Refreshing mv_neighborhood_poi_counts... ");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await db.Database.ExecuteSqlRawAsync(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY mv_neighborhood_poi_counts",
                cancellationToken);
            Console.WriteLine($"done ({sw.ElapsedMilliseconds}ms)");

            Console.Write("  Refreshing mv_neighborhood_nearest_pois... ");
            sw.Restart();
            await db.Database.ExecuteSqlRawAsync(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY mv_neighborhood_nearest_pois",
                cancellationToken);
            Console.WriteLine($"done ({sw.ElapsedMilliseconds}ms)");

            Console.WriteLine();
            Console.WriteLine("Views refreshed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("FAILED");
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Hint: If views don't exist, run 'dotnet ef database update' first.");
            return 1;
        }
    }
}
