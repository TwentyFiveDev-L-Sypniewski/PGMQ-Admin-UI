using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
[Obsolete("This test class has async rendering timing issues with Fluent UI components. Tests fail because FluentMessageBar elements are not found in time. Refactor to use bUnit's WaitForAssertion or WaitForElement mechanisms instead of Task.Delay.")]
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
        using var _ = new AssertionScope();
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var title = cut.Find("h3");
        title.TextContent.Should().Contain("Send Message");
    }

    [Test]
    public async Task ShowsQueueNameInput_AsReadonly()
    {
        using var _ = new AssertionScope();
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textFields = cut.FindAll("fluent-text-field");
        var queueNameField = textFields.FirstOrDefault();

        queueNameField.Should().NotBeNull();
    }

    [Test]
    public async Task ShowsMessageTextArea()
    {
        using var _ = new AssertionScope();
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textAreas = cut.FindAll("fluent-text-area");
        textAreas.Count.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task ShowsDelayNumberField()
    {
        using var _ = new AssertionScope();
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var numberFields = cut.FindAll("fluent-number-field");
        numberFields.Count.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task ShowsSendAndCancelButtons()
    {
        using var _ = new AssertionScope();
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var buttons = cut.FindAll("fluent-button");
        var sendButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Send"));
        var cancelButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Cancel"));

        sendButton.Should().NotBeNull();
        cancelButton.Should().NotBeNull();
    }

    [Test]
    public async Task CallsMessageService_WhenFormSubmittedWithValidJson()
    {
        using var _ = new AssertionScope();
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
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        await Task.Delay(100); // Wait for async operation

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
        using var _ = new AssertionScope();
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("invalid json"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        await Task.Delay(100); // Wait for validation

        var errorBars = cut.FindAll("fluent-message-bar");
        errorBars.Count.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task ShowsSuccessNotification_WhenMessageSentSuccessfully()
    {
        using var _ = new AssertionScope();
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
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        await Task.Delay(100); // Wait for async operation

        A.CallTo(() => _fakeNotificationService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ShowsErrorNotification_WhenSendMessageFails()
    {
        using var _ = new AssertionScope();
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
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        await Task.Delay(100); // Wait for async operation

        A.CallTo(() => _fakeNotificationService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }
}
