using System.CommandLine;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pipeline.Cli.Dtos;
using Pipeline.Cli.Services;
using Pipeline.Core.Data;
using Pipeline.Core.Entities.Content;

namespace Pipeline.Cli.Commands;

public static class MigrateContentCommand
{
    private const string DefaultInputPath = "agents/pipeline-outputs";

    public static Command Create(IServiceProvider services)
    {
        var inputPathOption = new Option<string>("--input-path") { Description = "Path to the pipeline-outputs directory" };
        var dryRunOption = new Option<bool>("--dry-run") { Description = "Preview changes without writing to database" };

        var command = new Command("migrate-content", "Migrate JSON content files to database")
        {
            inputPathOption,
            dryRunOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var inputPath = parseResult.GetValue(inputPathOption) ?? DefaultInputPath;
            var dryRun = parseResult.GetValue(dryRunOption);

            return await ExecuteAsync(services, inputPath, dryRun, cancellationToken);
        });

        return command;
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider services,
        string inputPath,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PipelineDbContext>();
        var titleCaseConverter = new DutchTitleCaseConverter();

        var stats = new MigrationStats();

        // Resolve absolute path
        var absolutePath = Path.GetFullPath(inputPath);
        if (!Directory.Exists(absolutePath))
        {
            Console.WriteLine($"Error: Directory not found: {absolutePath}");
            return 1;
        }

        // Find all 4-brand-reviewer.json files
        var jsonFiles = Directory.GetFiles(absolutePath, "4-brand-reviewer.json", SearchOption.AllDirectories);
        stats.TotalFiles = jsonFiles.Length;

        Console.WriteLine($"Found {stats.TotalFiles} content files to process");
        Console.WriteLine($"Dry run: {dryRun}");
        Console.WriteLine();

        // Get existing NIS codes to skip duplicates
        var existingNisCodes = await db.NeighborhoodProse
            .Select(p => p.NisCode)
            .ToHashSetAsync(cancellationToken);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        foreach (var filePath in jsonFiles)
        {
            var folderName = Path.GetFileName(Path.GetDirectoryName(filePath))!;

            // Validate NIS code (exactly 7 alphanumeric characters)
            if (!IsValidNisCode(folderName))
            {
                stats.InvalidNisCode++;
                stats.Errors.Add($"Invalid NIS code (skipped): {folderName}");
                continue;
            }

            var nisCode = folderName;

            // Check for duplicates
            if (existingNisCodes.Contains(nisCode))
            {
                stats.Skipped++;
                continue;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                var dto = JsonSerializer.Deserialize<BrandReviewerOutputDto>(json, jsonOptions);

                if (dto == null)
                {
                    stats.Errors.Add($"Failed to parse JSON: {filePath}");
                    continue;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.Id) ||
                    string.IsNullOrWhiteSpace(dto.City) ||
                    string.IsNullOrWhiteSpace(dto.Name) ||
                    string.IsNullOrWhiteSpace(dto.Intro) ||
                    string.IsNullOrWhiteSpace(dto.Subtitle))
                {
                    stats.Errors.Add($"Missing required fields: {filePath}");
                    continue;
                }

                // Convert UPPERCASE name to Title Case
                var name = titleCaseConverter.ToTitleCase(dto.Name);

                var prose = new NeighborhoodProse
                {
                    NisCode = nisCode,
                    Slug = dto.Id,
                    City = dto.City,
                    Name = name,
                    Intro = dto.Intro,
                    Subtitle = dto.Subtitle,
                    QualityScore = dto.BrandReview?.QualityScore,
                    SeoQualityScore = dto.SeoReview?.QualityScore,
                    PromptVersion = dto.SchemaVersion,
                    GeneratedAt = DateTime.SpecifyKind(dto.GeneratedAt, DateTimeKind.Utc)
                };

                if (!dryRun)
                {
                    db.NeighborhoodProse.Add(prose);
                }

                stats.Inserted++;
                existingNisCodes.Add(nisCode); // Prevent duplicates within same run
            }
            catch (JsonException ex)
            {
                stats.Errors.Add($"JSON parse error in {filePath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                stats.Errors.Add($"Error processing {filePath}: {ex.Message}");
            }
        }

        if (!dryRun && stats.Inserted > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        PrintStats(stats, dryRun);

        return stats.Errors.Count > 0 ? 1 : 0;
    }

    private static bool IsValidNisCode(string code)
    {
        // Belgian NIS codes are exactly 7 alphanumeric characters
        return code.Length == 7 && code.All(char.IsLetterOrDigit);
    }

    private static void PrintStats(MigrationStats stats, bool dryRun)
    {
        Console.WriteLine("=== Migration Summary ===");
        Console.WriteLine($"Total files found:    {stats.TotalFiles}");
        Console.WriteLine($"Inserted:             {stats.Inserted}{(dryRun ? " (dry run)" : "")}");
        Console.WriteLine($"Skipped (duplicate):  {stats.Skipped}");
        Console.WriteLine($"Invalid NIS code:     {stats.InvalidNisCode}");
        Console.WriteLine($"Errors:               {stats.Errors.Count}");

        if (stats.Errors.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("=== Errors ===");
            foreach (var error in stats.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }

        Console.WriteLine();
        if (dryRun)
        {
            Console.WriteLine("Dry run complete. No changes were made.");
        }
        else
        {
            Console.WriteLine($"Migration complete. {stats.Inserted} records inserted.");
        }
    }

    private class MigrationStats
    {
        public int TotalFiles { get; set; }
        public int Inserted { get; set; }
        public int Skipped { get; set; }
        public int InvalidNisCode { get; set; }
        public List<string> Errors { get; } = [];
    }
}
