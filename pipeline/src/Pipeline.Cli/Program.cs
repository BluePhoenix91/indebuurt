using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pipeline.Core.Data;

var builder = Host.CreateApplicationBuilder(args);

// Database (same config as API)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PipelineDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.UseNetTopologySuite()
    ));

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

return await rootCommand.Parse(args).InvokeAsync();
