using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components;
using PgmqAdminUI.Components.UI;
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

// Add notification service
builder.Services.AddSingleton<NotificationService>();

// Add feature services with connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("pgmq")
    ?? throw new InvalidOperationException("Connection string 'pgmq' not found.");
builder.Services.AddSingleton(sp => new QueueService(
    connectionString,
    sp.GetRequiredService<ILogger<QueueService>>()));
builder.Services.AddSingleton(sp => new MessageService(
    connectionString,
    sp.GetRequiredService<ILogger<MessageService>>()));

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

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
