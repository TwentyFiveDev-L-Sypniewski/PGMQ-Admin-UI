using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

using PgmqAdminUI.Components.Pages;
using PgmqAdminUI.Features.Queues;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.Pages;

[Property("Category", "Component")]
[Obsolete("This test class is obsolete and will be removed in a future version.")]
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
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var title = cut.Find("h2");
        await Assert.That(title.TextContent).Contains("test-queue");
    }

    [Test]
    public async Task DisplaysThreeTabs()
    {
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var tabs = cut.FindAll("fluent-tab");
        await Assert.That(tabs.Count).IsEqualTo(3);
    }

    [Test]
    public async Task ShowsSendMessageButton()
    {
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var sendButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Send Message"));

        await Assert.That(sendButton).IsNotNull();
    }

    [Test]
    public async Task ShowsBackToQueuesButton()
    {
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var backButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Back to Queues"));

        await Assert.That(backButton).IsNotNull();
    }

    [Test]
    public async Task NavigatesBack_WhenBackButtonClicked()
    {
        var cut = Render<QueueDetail>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var backButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Back to Queues"));

        backButton?.Click();

        await Assert.That(_fakeNavigationManager.Uri).Contains("/queues");
    }

    private class FakeNavigationManager : NavigationManager
    {
        public FakeNavigationManager() => Initialize("https://localhost:5001/", "https://localhost:5001/");

        protected override void NavigateToCore(string uri, bool forceLoad) => Uri = ToAbsoluteUri(uri).ToString();
    }
}
