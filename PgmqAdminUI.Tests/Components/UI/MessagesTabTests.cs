using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using PgmqAdminUI.Features.Messages;
using PgmqAdminUI.Features.Queues;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
[Obsolete]
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

        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        var title = cut.Find("h3");
        await Assert.That(title.TextContent).Contains("Messages");
    }

    [Test]
    public async Task ShowsLoadingIndicator_WhenLoadingMessages()
    {
        var tcs = new TaskCompletionSource<QueueDetailDto>();
        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Returns(tcs.Task);

        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Assert.That(cut.FindAll("fluent-progress-ring").Count).IsGreaterThan(0);

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

        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        await Assert.That(cut.FindAll("fluent-data-grid").Count).IsEqualTo(1);
    }

    [Test]
    public async Task ShowsInfoMessage_WhenNoMessagesExist()
    {
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

        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        var messageBar = cut.Find("fluent-message-bar");
        await Assert.That(messageBar).IsNotNull();
    }

    [Test]
    public async Task ShowsRefreshButton()
    {
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

        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var refreshButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Refresh"));

        await Assert.That(refreshButton).IsNotNull();
    }

    [Test]
    public async Task CallsDeleteMessage_WhenDeleteButtonClicked()
    {
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

        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        var buttons = cut.FindAll("fluent-button");
        var deleteButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Delete"));
        deleteButton?.Click();

        await Task.Delay(100).ConfigureAwait(false); // Wait for async operation

        A.CallTo(() => _fakeMessageService.DeleteMessageAsync("test-queue", 1, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task CallsArchiveMessage_WhenArchiveButtonClicked()
    {
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

        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        var buttons = cut.FindAll("fluent-button");
        var archiveButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Archive"));
        archiveButton?.Click();

        await Task.Delay(100).ConfigureAwait(false); // Wait for async operation

        A.CallTo(() => _fakeMessageService.ArchiveMessageAsync("test-queue", 1, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ShowsErrorMessage_WhenLoadMessagesFails()
    {
        A.CallTo(() => _fakeQueueService.GetQueueDetailAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._))
            .Throws(new Exception("Database connection failed"));

        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        A.CallTo(() => _fakeNotificationService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }
}
