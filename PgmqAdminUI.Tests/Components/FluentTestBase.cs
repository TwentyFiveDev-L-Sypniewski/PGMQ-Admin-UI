using Bunit;

using Microsoft.FluentUI.AspNetCore.Components;

namespace PgmqAdminUI.Tests.Components;

/// <summary>
/// Base class for bUnit component tests that use Fluent UI components.
/// Registers required Fluent UI services and implements async disposal.
/// </summary>
public abstract class FluentTestBase : BunitContext, IAsyncDisposable
{
    protected FluentTestBase()
    {
        // Register Fluent UI services required for component rendering
        Services.AddFluentUIComponents();

        // Configure JSInterop to handle Fluent UI JavaScript calls
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public new async ValueTask DisposeAsync()
    {
        // FluentUI components use services implementing IAsyncDisposable (e.g., BodyFont)
        // Must use async disposal to properly clean up resources
        if (Services is IAsyncDisposable asyncDisposableServiceProvider)
        {
            await asyncDisposableServiceProvider.DisposeAsync();
        }
        else
        {
            Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
