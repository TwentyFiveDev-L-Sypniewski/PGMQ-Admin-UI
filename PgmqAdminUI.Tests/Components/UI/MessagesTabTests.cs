using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using PgmqAdminUI.Features.Messages;
using PgmqAdminUI.Features.Queues;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
public class MessagesTabTests : FluentTestBase
{
    private readonly QueueService _fakeQueueService;
    private readonly MessageService _fakeMessageService;
    private readonly IMessageService _fakeNotificationService;

    public MessagesTabTests()
    {
        _fakeQueueService = A.Fake<QueueService>();
        _fakeMessageService = A.Fake<MessageService>();
        _fakeNotificationService = A.Fake<IMessageService>();

        Services.AddSingleton(_fakeQueueService);
        Services.AddSingleton(_fakeMessageService);
        Services.AddSingleton<IMessageService>(_fakeNotificationService);
    }

    [Test]
    public async Task RendersMessagesTabTitle()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(new QueueDetailDto
            {
                QueueName = "test-queue",
                Messages = [],
                TotalCount = 0,
                PageSize = 20,
                CurrentPage = 1
            }));

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var title = cut.Find("h3");
            title.TextContent.Should().Contain("Messages");
        });
    }

    [Test]
    public async Task ShowsLoadingIndicator_WhenLoadingMessages()
    {
        // Arrange
        var tcs = new TaskCompletionSource<QueueDetailDto>();
        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Returns(tcs.Task);

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert - should show loading indicator while waiting
        cut.FindAll("fluent-progress-ring").Count.Should().BeGreaterThan(0);

        // Cleanup - complete the task
        tcs.SetResult(new QueueDetailDto
        {
            QueueName = "test-queue",
            Messages = [],
            TotalCount = 0,
            PageSize = 20,
            CurrentPage = 1
        });
    }

    [Test]
    public async Task DisplaysMessageGrid_WhenMessagesExist()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new() { MsgId = 1, Message = "{\"test\": \"data1\"}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 0 },
            new() { MsgId = 2, Message = "{\"test\": \"data2\"}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 1 }
        };

        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(new QueueDetailDto
            {
                QueueName = "test-queue",
                Messages = messages,
                TotalCount = 2,
                PageSize = 20,
                CurrentPage = 1
            }));

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert - wait for component to finish loading by checking for grid
        // FluentDataGrid renders as a table element
        await cut.WaitForStateAsync(
            () => cut.Markup.Contains("table") || cut.Markup.Contains("role=\"grid\""),
            TimeSpan.FromSeconds(3));

        // Verify the grid is rendered (table or div with grid role)
        var markup = cut.Markup;
        (markup.Contains("table") || markup.Contains("role=\"grid\"")).Should().BeTrue(
            $"Expected a table or grid element. Markup: {markup}");
    }

    [Test]
    public async Task ShowsInfoMessage_WhenNoMessagesExist()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(new QueueDetailDto
            {
                QueueName = "test-queue",
                Messages = [],
                TotalCount = 0,
                PageSize = 20,
                CurrentPage = 1
            }));

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert - wait for component to finish loading
        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        // FluentMessageBar renders with class "fluent-messagebar" not as <fluent-message-bar> custom element
        cut.Markup.Should().Contain("fluent-messagebar");
    }

    [Test]
    public async Task ShowsRefreshButton()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(new QueueDetailDto
            {
                QueueName = "test-queue",
                Messages = [],
                TotalCount = 0,
                PageSize = 20,
                CurrentPage = 1
            }));

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var refreshButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Refresh"));
            refreshButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task DeleteButton_ClickingDelete_ShouldCallDeleteService()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new() { MsgId = 1, Message = "{\"test\": \"data1\"}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 0 }
        };

        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(new QueueDetailDto
            {
                QueueName = "test-queue",
                Messages = messages,
                TotalCount = 1,
                PageSize = 20,
                CurrentPage = 1
            }));

        A.CallTo(() => _fakeMessageService.DeleteMessageAsync(
            A<string>._,
            A<long>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(true));

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Wait for component to load
        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        // Click the delete button (identified by title attribute)
        var deleteButton = cut.Find("fluent-button[title='Delete message']");
        deleteButton.Click();

        // Assert - verify delete was called
        A.CallTo(() => _fakeMessageService.DeleteMessageAsync("test-queue", 1, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ArchiveButton_ClickingArchive_ShouldCallArchiveService()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new() { MsgId = 1, Message = "{\"test\": \"data1\"}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 0 }
        };

        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(new QueueDetailDto
            {
                QueueName = "test-queue",
                Messages = messages,
                TotalCount = 1,
                PageSize = 20,
                CurrentPage = 1
            }));

        A.CallTo(() => _fakeMessageService.ArchiveMessageAsync(
            A<string>._,
            A<long>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(true));

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Wait for component to load
        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        // Click the archive button (identified by title attribute)
        var archiveButton = cut.Find("fluent-button[title='Archive message']");
        archiveButton.Click();

        // Assert - verify archive was called
        A.CallTo(() => _fakeMessageService.ArchiveMessageAsync("test-queue", 1, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ShowsErrorMessage_WhenLoadMessagesFails()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Throws(new Exception("Database connection failed"));

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert - verify error notification was shown
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeNotificationService.ShowMessageBar(A<Action<MessageOptions>>._))
                .MustHaveHappened();
        });
    }
}
