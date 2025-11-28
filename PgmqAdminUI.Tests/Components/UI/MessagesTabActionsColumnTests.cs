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
    public async Task ActionsColumn_ShouldRenderMenuButton()
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

        // Assert
        var menuButtons = cut.FindAll("fluent-menu-button");
        menuButtons.Count.Should().BeGreaterThanOrEqualTo(1,
            "Actions column should use FluentMenuButton for row actions");

        var markup = cut.Markup;
        markup.Should().Contain("fluent-menu-item",
            "FluentMenuButton should contain FluentMenuItem elements for Delete and Archive actions");
    }

    [Test]
    public async Task ActionsColumn_ClickingMenuButton_ShouldNotTriggerDelete()
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

        // Act
        var cut = Render<MessagesTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        var menuButtons = cut.FindAll("fluent-menu-button");
        menuButtons.Should().NotBeEmpty("Actions column should have a menu button");

        // Click the menu button - should only open menu, not trigger delete
        menuButtons.First().Click();

        // Assert
        A.CallTo(() => _fakeMessageService.DeleteMessageAsync(A<string>._, A<long>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task ActionsColumn_MenuShouldContainDeleteAndArchiveItems()
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

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Delete", "Menu should contain Delete option");
        markup.Should().Contain("Archive", "Menu should contain Archive option");
    }

    [Test]
    public async Task ActionsColumn_WithMultipleMessages_EachRowShouldHaveMenuButton()
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

        // Assert - each message row should have its own menu button
        var menuButtons = cut.FindAll("fluent-menu-button");
        menuButtons.Count.Should().BeGreaterThanOrEqualTo(3, "Each message row should have a menu button");
    }
}
