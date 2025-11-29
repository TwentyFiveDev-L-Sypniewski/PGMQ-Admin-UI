using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using PgmqAdminUI.Features.Messages;
using PgmqAdminUI.Features.Queues;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
public class MessagesTabActionsColumnTests : FluentTestBase
{
    private readonly QueueService _fakeQueueService;
    private readonly MessageService _fakeMessageService;
    private readonly IMessageService _fakeNotificationService;

    public MessagesTabActionsColumnTests()
    {
        _fakeQueueService = A.Fake<QueueService>();
        _fakeMessageService = A.Fake<MessageService>();
        _fakeNotificationService = A.Fake<IMessageService>();

        Services.AddSingleton(_fakeQueueService);
        Services.AddSingleton(_fakeMessageService);
        Services.AddSingleton<IMessageService>(_fakeNotificationService);
    }

    [Test]
    public async Task ActionsColumn_ShouldRenderDeleteAndArchiveButtons()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new() { MsgId = 1, Message = "{\"test\": \"data\"}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 0 }
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

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        // Assert - Should have Delete and Archive buttons with title attributes
        var deleteButtons = cut.FindAll("fluent-button[title='Delete message']");
        var archiveButtons = cut.FindAll("fluent-button[title='Archive message']");

        deleteButtons.Count.Should().BeGreaterThanOrEqualTo(1,
            "Actions column should have a Delete button");
        archiveButtons.Count.Should().BeGreaterThanOrEqualTo(1,
            "Actions column should have an Archive button");
    }

    [Test]
    public async Task ActionsColumn_ClickingDeleteButton_ShouldCallDeleteService()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new() { MsgId = 42, Message = "{\"test\": \"data\"}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 0 }
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

        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        var deleteButton = cut.Find("fluent-button[title='Delete message']");
        deleteButton.Click();

        // Assert
        A.CallTo(() => _fakeMessageService.DeleteMessageAsync("test-queue", 42, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ActionsColumn_ClickingArchiveButton_ShouldCallArchiveService()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new() { MsgId = 42, Message = "{\"test\": \"data\"}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 0 }
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

        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        var archiveButton = cut.Find("fluent-button[title='Archive message']");
        archiveButton.Click();

        // Assert
        A.CallTo(() => _fakeMessageService.ArchiveMessageAsync("test-queue", 42, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ActionsColumn_WithMultipleMessages_EachRowShouldHaveActionButtons()
    {
        // Arrange
        var messages = new List<MessageDto>
        {
            new() { MsgId = 1, Message = "{\"id\": 1}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 0 },
            new() { MsgId = 2, Message = "{\"id\": 2}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 1 },
            new() { MsgId = 3, Message = "{\"id\": 3}", EnqueuedAt = DateTimeOffset.UtcNow, ReadCount = 2 }
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
                TotalCount = 3,
                PageSize = 20,
                CurrentPage = 1
            }));

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        // Assert - each message row should have its own Delete and Archive buttons
        var deleteButtons = cut.FindAll("fluent-button[title='Delete message']");
        var archiveButtons = cut.FindAll("fluent-button[title='Archive message']");

        deleteButtons.Count.Should().Be(3, "Each message row should have a Delete button");
        archiveButtons.Count.Should().Be(3, "Each message row should have an Archive button");
    }
}
