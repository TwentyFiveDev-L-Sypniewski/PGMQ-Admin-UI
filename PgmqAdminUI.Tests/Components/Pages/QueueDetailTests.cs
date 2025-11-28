using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

using PgmqAdminUI.Components.Pages;
using PgmqAdminUI.Features.Queues;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.Pages;

[Property("Category", "Component")]
public class QueueDetailTests : FluentTestBase
{
    private readonly FakeNavigationManager _fakeNavigationManager;
    private readonly QueueService _fakeQueueService;
    private readonly MessageService _fakeMessageService;
    private readonly IMessageService _fakeNotificationService;

    public QueueDetailTests()
    {
        _fakeNavigationManager = new FakeNavigationManager();
        _fakeQueueService = A.Fake<QueueService>();
        _fakeMessageService = A.Fake<MessageService>();
        _fakeNotificationService = A.Fake<IMessageService>();

        Services.AddSingleton<NavigationManager>(_fakeNavigationManager);
        Services.AddSingleton(_fakeQueueService);
        Services.AddSingleton(_fakeMessageService);
        Services.AddSingleton(_fakeNotificationService);
    }

    [Test]
    public async Task RendersQueueDetailPageTitle()
    {
        // Arrange & Act
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var title = cut.Find("h2");
            title.TextContent.Should().Contain("test-queue");
        });
    }

    [Test]
    public async Task DisplaysThreeTabs()
    {
        // Arrange & Act
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var tabs = cut.FindAll("fluent-tab");
            tabs.Count.Should().Be(3);
        });
    }

    [Test]
    public async Task ShowsSendMessageButton()
    {
        // Arrange & Act
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var sendButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Send Message"));
            sendButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task ShowsBackToQueuesButton()
    {
        // Arrange & Act
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var backButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Back to Queues"));
            backButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task NavigatesBack_WhenBackButtonClicked()
    {
        // Arrange
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Act - wait for button to appear and click it
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var backButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Back to Queues"));
            backButton.Should().NotBeNull();
            backButton?.Click();
        });

        // Assert
        _fakeNavigationManager.Uri.Should().Contain("/queues");
    }

    private class FakeNavigationManager : NavigationManager
    {
        public FakeNavigationManager() => Initialize("https://localhost:5001/", "https://localhost:5001/");

        protected override void NavigateToCore(string uri, bool forceLoad) => Uri = ToAbsoluteUri(uri).ToString();
    }
}
