using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

using PgmqAdminUI.Components.Pages;
using PgmqAdminUI.Features.Queues;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.Pages;

[Property("Category", "Component")]
[Obsolete("This test class has async rendering timing issues with Fluent UI components. Refactor to use bUnit's proper async waiting mechanisms instead of Task.Delay.")]
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
        using var _ = new AssertionScope();
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var title = cut.Find("h2");
        title.TextContent.Should().Contain("test-queue");
    }

    [Test]
    public async Task DisplaysThreeTabs()
    {
        using var _ = new AssertionScope();
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var tabs = cut.FindAll("fluent-tab");
        tabs.Count.Should().Be(3);
    }

    [Test]
    public async Task ShowsSendMessageButton()
    {
        using var _ = new AssertionScope();
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var sendButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Send Message"));

        sendButton.Should().NotBeNull();
    }

    [Test]
    public async Task ShowsBackToQueuesButton()
    {
        using var _ = new AssertionScope();
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var backButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Back to Queues"));

        backButton.Should().NotBeNull();
    }

    [Test]
    public async Task NavigatesBack_WhenBackButtonClicked()
    {
        using var _ = new AssertionScope();
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var backButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Back to Queues"));

        backButton?.Click();

        _fakeNavigationManager.Uri.Should().Contain("/queues");
    }

    private class FakeNavigationManager : NavigationManager
    {
        public FakeNavigationManager() => Initialize("https://localhost:5001/", "https://localhost:5001/");

        protected override void NavigateToCore(string uri, bool forceLoad) => Uri = ToAbsoluteUri(uri).ToString();
    }
}
