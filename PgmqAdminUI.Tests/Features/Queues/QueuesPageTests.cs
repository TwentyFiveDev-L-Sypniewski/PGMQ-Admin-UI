using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Features.Queues;
using PgmqAdminUI.Tests.Components;

namespace PgmqAdminUI.Tests.Features.Queues;

[Category("Component")]
public class QueuesPageTests : FluentTestBase
{
    private readonly QueueService _queueService;
    private readonly IMessageService _messageService;
    private readonly ILogger<PgmqAdminUI.Components.Pages.Queues> _logger;

    public QueuesPageTests()
    {
        _queueService = A.Fake<QueueService>(x => x.WithArgumentsForConstructor(new object[] { "dummy-conn-string", A.Fake<ILogger<QueueService>>() }));
        _messageService = A.Fake<IMessageService>();
        _logger = A.Fake<ILogger<PgmqAdminUI.Components.Pages.Queues>>();

        Services.AddSingleton(_queueService);
        Services.AddSingleton(_messageService);
        Services.AddSingleton(_logger);

        EnsureMenuProviderRendered();
    }

    [Test]
    public void Renders_DesktopAndMobileViews()
    {
        // Arrange
        var queues = new List<QueueDto>
        {
            new() { Name = "test-queue", TotalMessages = 10, InFlightMessages = 2, ArchivedMessages = 0 }
        };

        A.CallTo(() => _queueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult<IEnumerable<QueueDto>>(queues));

        // Act
        var cut = Render<PgmqAdminUI.Components.Pages.Queues>();

        // Assert
        // Check for desktop view
        cut.Find(".desktop-view").Should().NotBeNull();

        // Check for mobile view
        cut.Find(".mobile-view").Should().NotBeNull();

        // Check that mobile view contains the card
        cut.Find(".mobile-view .queue-card").Should().NotBeNull();
        cut.Find(".mobile-view .queue-card").TextContent.Should().Contain("test-queue");

        // Check FluentDataGrid configuration
        var grid = cut.FindComponent<FluentDataGrid<QueueDto>>();
        grid.Instance.MultiLine.Should().BeTrue();
        grid.Instance.GridTemplateColumns.Should().Be("minmax(150px, 2fr) minmax(80px, 1fr) minmax(80px, 1fr) minmax(80px, 1fr) minmax(200px, 2fr)");
    }
}
