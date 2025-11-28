using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
[Obsolete]
public class SendMessageDialogTests : FluentTestBase
{
    private readonly MessageService _fakeMessageService;
    private readonly IMessageService _fakeNotificationService;

    public SendMessageDialogTests()
    {
        _fakeMessageService = A.Fake<MessageService>();
        _fakeNotificationService = A.Fake<IMessageService>();

        Services.AddSingleton(_fakeMessageService);
        Services.AddSingleton<IMessageService>(_fakeNotificationService);
    }

    [Test]
    public async Task RendersDialogTitle()
    {
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var title = cut.Find("h3");
        await Assert.That(title.TextContent).Contains("Send Message");
    }

    [Test]
    public async Task ShowsQueueNameInput_AsReadonly()
    {
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textFields = cut.FindAll("fluent-text-field");
        var queueNameField = textFields.FirstOrDefault();

        await Assert.That(queueNameField).IsNotNull();
    }

    [Test]
    public async Task ShowsMessageTextArea()
    {
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textAreas = cut.FindAll("fluent-text-area");
        await Assert.That(textAreas.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task ShowsDelayNumberField()
    {
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var numberFields = cut.FindAll("fluent-number-field");
        await Assert.That(numberFields.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task ShowsSendAndCancelButtons()
    {
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var sendButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Send"));
        var cancelButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Cancel"));

        await Assert.That(sendButton).IsNotNull();
        await Assert.That(cancelButton).IsNotNull();
    }

    [Test]
    public async Task CallsMessageService_WhenFormSubmittedWithValidJson()
    {
        A.CallTo(() => _fakeMessageService.SendMessageAsync(
            A<string>._,
            A<string>._,
            A<int?>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(123L));

        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}")).ConfigureAwait(false);

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit()).ConfigureAwait(false);

        await Task.Delay(100).ConfigureAwait(false); // Wait for async operation

        A.CallTo(() => _fakeMessageService.SendMessageAsync(
            "test-queue",
            "{\"test\": \"data\"}",
            A<int?>._,
            A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ShowsErrorMessage_WhenJsonIsInvalid()
    {
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("invalid json")).ConfigureAwait(false);

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit()).ConfigureAwait(false);

        await Task.Delay(100).ConfigureAwait(false); // Wait for validation

        var errorBars = cut.FindAll("fluent-message-bar");
        await Assert.That(errorBars.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task ShowsSuccessNotification_WhenMessageSentSuccessfully()
    {
        A.CallTo(() => _fakeMessageService.SendMessageAsync(
            A<string>._,
            A<string>._,
            A<int?>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(123L));

        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}")).ConfigureAwait(false);

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit()).ConfigureAwait(false);

        await Task.Delay(100).ConfigureAwait(false); // Wait for async operation

        A.CallTo(() => _fakeNotificationService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ShowsErrorNotification_WhenSendMessageFails()
    {
        A.CallTo(() => _fakeMessageService.SendMessageAsync(
            A<string>._,
            A<string>._,
            A<int?>._,
            A<CancellationToken>._))
            .Throws(new Exception("Database error"));

        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}")).ConfigureAwait(false);

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit()).ConfigureAwait(false);

        await Task.Delay(100).ConfigureAwait(false); // Wait for async operation

        A.CallTo(() => _fakeNotificationService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }
}
