using HelpBoard.Repositories;
using HelpBoard.Repositories.Data;
using HelpBoard.Services;
using HelpBoard.Api.FeatureToggles;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("HelpBoard")
    ?? throw new InvalidOperationException("Connection string 'HelpBoard' is not configured.");

builder.Services.AddRepositories(connectionString);

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddServices();
builder.Services.AddFeatureManagement();

// ── Web API ───────────────────────────────────────────────────────────────────
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    var knownProxy = builder.Configuration["ReverseProxy:KnownProxy"];
    if (!string.IsNullOrWhiteSpace(knownProxy))
    {
        options.KnownProxies.Add(System.Net.IPAddress.Parse(knownProxy));
    }
});

// ── OpenAPI / Scalar ──────────────────────────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new()
        {
            Title = "HELP-Board API",
            Version = "v1",
            Description = "REST API for the HELP-Board application."
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseHealthChecks("/healthz");

var featureManager = app.Services.GetRequiredService<IFeatureManager>();
var applyMigrationsAtStartup = await featureManager.IsEnabledAsync(FeatureFlagKeys.DatabaseMigrationsApplyAtStartup);

if (applyMigrationsAtStartup)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ── Static files for hosted Blazor WASM ──────────────────────────────────────
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// ── Exception handling ────────────────────────────────────────────────────────
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "HELP-Board API";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Fallback serves the Blazor WASM shell for any non-API route
app.MapFallbackToFile("index.html");

app.Run();

// Expose Program for integration testing
public partial class Program { }
