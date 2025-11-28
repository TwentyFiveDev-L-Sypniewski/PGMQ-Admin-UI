using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
public class CreateQueueDialogTests : FluentTestBase
{
    private readonly QueueService _fakeQueueService;
    private readonly IMessageService _fakeMessageService;

    public CreateQueueDialogTests()
    {
        _fakeQueueService = A.Fake<QueueService>();
        _fakeMessageService = A.Fake<IMessageService>();

        Services.AddSingleton(_fakeQueueService);
        Services.AddSingleton(_fakeMessageService);
    }

    [Test]
    public async Task RendersDialogTitle()
    {
        // Arrange & Act
        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var title = cut.Find("h3");
            title.TextContent.Should().Contain("Create New Queue");
        });
    }

    [Test]
    public async Task ShowsQueueNameInput()
    {
        // Arrange & Act
        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var textFields = cut.FindAll("fluent-text-field");
            textFields.Count.Should().BeGreaterThan(0);
        });
    }

    [Test]
    public async Task ShowsCreateButton()
    {
        // Arrange & Act
        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var createButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Create"));
            createButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task ShowsCancelButton()
    {
        // Arrange & Act
        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var cancelButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Cancel"));
            cancelButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task CallsQueueService_WhenFormSubmittedWithValidData()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.CreateQueueAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var onQueueCreatedCalled = false;

        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnQueueCreated, () => { onQueueCreatedCalled = true; return Task.CompletedTask; }));

        // Act
        var input = cut.Find("fluent-text-field");
        await cut.InvokeAsync(() => input.Change("test-queue"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeQueueService.CreateQueueAsync("test-queue", A<CancellationToken>._))
                .MustHaveHappened();
        });
    }

    [Test]
    public async Task ShowsSuccessMessage_WhenQueueCreatedSuccessfully()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.CreateQueueAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Act
        var input = cut.Find("fluent-text-field");
        await cut.InvokeAsync(() => input.Change("test-queue"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
                .MustHaveHappened();
        });
    }

    [Test]
    public async Task ShowsErrorMessage_WhenQueueCreationFails()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.CreateQueueAsync(A<string>._, A<CancellationToken>._))
            .Throws(new Exception("Database error"));

        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Act
        var input = cut.Find("fluent-text-field");
        await cut.InvokeAsync(() => input.Change("test-queue"));

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
                .MustHaveHappened();
        });
    }
}
