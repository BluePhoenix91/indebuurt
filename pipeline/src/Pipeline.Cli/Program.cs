using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pipeline.Cli.Commands;
using Pipeline.Core.Data;
using Pipeline.Core.Repositories;
using Pipeline.Core.Services;
using Pipeline.Core.Services.PoiImport;

var builder = Host.CreateApplicationBuilder(args);

// Explicitly add appsettings.json from executable directory (handles dotnet run working dir issues)
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

// Database (same config as API)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PipelineDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.UseNetTopologySuite()
    ));

// Overpass API configuration (Story O1)
builder.Services.Configure<OverpassOptions>(
    builder.Configuration.GetSection(OverpassOptions.SectionName));

// HTTP client for Overpass API
builder.Services.AddHttpClient<IOverpassClient, OverpassClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OverpassOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// POI Import services (Story O1)
builder.Services.AddScoped<OverpassToPoisConverter>();
builder.Services.AddScoped<IPoiStagingRepository>(sp => new PoiStagingRepository(
    connectionString!,
    sp.GetRequiredService<ILogger<PoiStagingRepository>>()));
builder.Services.AddScoped<IPoiImportService, PoiImportService>();

// Services (Story N3)
builder.Services.AddScoped<IGisRepository, GisRepository>();
builder.Services.AddScoped<ValueCardBuilder>();

var host = builder.Build();

// Root command
var rootCommand = new RootCommand("Pipeline CLI for data imports and batch processing");

// Placeholder command (replaced by Epic O)
var helloCommand = new Command("hello", "Test command to verify CLI works");
helloCommand.SetAction(async (parseResult, cancellationToken) =>
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PipelineDbContext>();
    var canConnect = await db.Database.CanConnectAsync(cancellationToken);
    Console.WriteLine(canConnect ? "Database connection: OK" : "Database connection: FAILED");
    return canConnect ? 0 : 1;
});
rootCommand.Add(helloCommand);

// Migrate content command (Story N2)
rootCommand.Add(MigrateContentCommand.Create(host.Services));

// Refresh views command (Story N3)
rootCommand.Add(RefreshViewsCommand.Create(host.Services));

// Import OSM command (Story O1)
rootCommand.Add(ImportOsmCommand.Create(host.Services));

return await rootCommand.Parse(args).InvokeAsync();
