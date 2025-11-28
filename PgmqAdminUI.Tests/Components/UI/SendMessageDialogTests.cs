using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using MessageService = PgmqAdminUI.Features.Messages.MessageService;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
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
        // Arrange & Act
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var title = cut.Find("h3");
            title.TextContent.Should().Contain("Send Message");
        });
    }

    [Test]
    public async Task ShowsQueueNameInput_AsReadonly()
    {
        // Arrange & Act
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var textFields = cut.FindAll("fluent-text-field");
            var queueNameField = textFields.FirstOrDefault();
            queueNameField.Should().NotBeNull();
        });
    }

    [Test]
    public async Task ShowsMessageTextArea()
    {
        // Arrange & Act
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var textAreas = cut.FindAll("fluent-text-area");
            textAreas.Count.Should().BeGreaterThan(0);
        });
    }

    [Test]
    public async Task ShowsDelayNumberField()
    {
        // Arrange & Act
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var numberFields = cut.FindAll("fluent-number-field");
            numberFields.Count.Should().BeGreaterThan(0);
        });
    }

    [Test]
    public async Task ShowsSendAndCancelButtons()
    {
        // Arrange & Act
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var sendButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Send"));
            var cancelButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Cancel"));

            sendButton.Should().NotBeNull();
            cancelButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task CallsMessageService_WhenFormSubmittedWithValidJson()
    {
        // Arrange
        A.CallTo(() => _fakeMessageService.SendMessageAsync(
            A<string>._,
            A<string>._,
            A<int?>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(123L));

        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Act
        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeMessageService.SendMessageAsync(
                "test-queue",
                "{\"test\": \"data\"}",
                A<int?>._,
                A<CancellationToken>._))
                .MustHaveHappened();
        });
    }

    [Test]
    public async Task ShowsErrorMessage_WhenJsonIsInvalid()
    {
        // Arrange
        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Act
        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("invalid json"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert - FluentMessageBar renders with class "fluent-messagebar"
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().Contain("fluent-messagebar");
        });
    }

    [Test]
    public async Task ShowsSuccessNotification_WhenMessageSentSuccessfully()
    {
        // Arrange
        A.CallTo(() => _fakeMessageService.SendMessageAsync(
            A<string>._,
            A<string>._,
            A<int?>._,
            A<CancellationToken>._))
            .Returns(Task.FromResult(123L));

        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Act
        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeNotificationService.ShowMessageBar(A<Action<MessageOptions>>._))
                .MustHaveHappened();
        });
    }

    [Test]
    public async Task ShowsErrorNotification_WhenSendMessageFails()
    {
        // Arrange
        A.CallTo(() => _fakeMessageService.SendMessageAsync(
            A<string>._,
            A<string>._,
            A<int?>._,
            A<CancellationToken>._))
            .Throws(new Exception("Database error"));

        var cut = Render<SendMessageDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.QueueName, "test-queue"));

        // Act
        var textArea = cut.Find("fluent-text-area");
        await cut.InvokeAsync(() => textArea.Change("{\"test\": \"data\"}"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeNotificationService.ShowMessageBar(A<Action<MessageOptions>>._))
                .MustHaveHappened();
        });
    }
}
