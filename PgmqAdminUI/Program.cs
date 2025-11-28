using Microsoft.FluentUI.AspNetCore.Components;
using Npgsql;
using PgmqAdminUI.Components;
using PgmqAdminUI.Features.Queues;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (telemetry, service discovery, resilience)
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient (required for Blazor Server + Fluent UI)
builder.Services.AddHttpClient();

// Add Fluent UI Blazor components
builder.Services.AddFluentUIComponents();

// Add feature services with connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("pgmq")
    ?? builder.Configuration.GetConnectionString("postgres")
    ?? throw new InvalidOperationException("Connection string 'pgmq' or 'postgres' not found.");
builder.Services.AddSingleton(sp => new QueueService(
    connectionString,
    sp.GetRequiredService<ILogger<QueueService>>()));
builder.Services.AddSingleton(sp => new MessageService(
    connectionString,
    sp.GetRequiredService<ILogger<MessageService>>()));

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Initialize PGMQ extension on startup
await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS pgmq CASCADE;", connection);
        await command.ExecuteNonQueryAsync();

        logger.LogInformation("PGMQ extension initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize PGMQ extension");
        throw;
    }
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
