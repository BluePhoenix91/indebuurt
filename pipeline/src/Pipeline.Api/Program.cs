using Microsoft.EntityFrameworkCore;
using Pipeline.Core.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<PipelineDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()
    ));

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PipelineDbContext>();

// Swagger (dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Redirect root to Swagger in development
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.MapGet("/", () => "Pipeline API - see /health");
}

// Health endpoint
app.MapHealthChecks("/health");

app.Run();
