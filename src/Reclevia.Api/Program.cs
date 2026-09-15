using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Reclevia.Api.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Reclevia.Api.Health;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgresql", tags: new[] { "ready" });
builder.Services.AddDbContext<RecleviaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Reclevia");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("connection string 'Reclevia' is missing");

    options.UseNpgsql(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();

public partial class Program
{

}
