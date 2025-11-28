using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
[Obsolete]
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
        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        var title = cut.Find("h3");
        await Assert.That(title.TextContent).Contains("Create New Queue");
    }

    [Test]
    public async Task ShowsQueueNameInput()
    {
        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        var textFields = cut.FindAll("fluent-text-field");
        await Assert.That(textFields.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task ShowsCreateButton()
    {
        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        var buttons = cut.FindAll("fluent-button");
        var createButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Create"));

        await Assert.That(createButton).IsNotNull();
    }

    [Test]
    public async Task ShowsCancelButton()
    {
        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        var buttons = cut.FindAll("fluent-button");
        var cancelButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Cancel"));

        await Assert.That(cancelButton).IsNotNull();
    }

    [Test]
    public async Task CallsQueueService_WhenFormSubmittedWithValidData()
    {
        A.CallTo(() => _fakeQueueService.CreateQueueAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var onQueueCreatedCalled = false;

        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnQueueCreated, () => { onQueueCreatedCalled = true; return Task.CompletedTask; }));

        var input = cut.Find("fluent-text-field");
        await cut.InvokeAsync(() => input.Change("test-queue")).ConfigureAwait(false);

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit()).ConfigureAwait(false);

        await Task.Delay(100).ConfigureAwait(false); // Wait for async operation

        A.CallTo(() => _fakeQueueService.CreateQueueAsync("test-queue", A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ShowsSuccessMessage_WhenQueueCreatedSuccessfully()
    {
        A.CallTo(() => _fakeQueueService.CreateQueueAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        var input = cut.Find("fluent-text-field");
        await cut.InvokeAsync(() => input.Change("test-queue")).ConfigureAwait(false);

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit()).ConfigureAwait(false);

        await Task.Delay(100).ConfigureAwait(false); // Wait for async operation

        A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }

    [Test]
    public async Task ShowsErrorMessage_WhenQueueCreationFails()
    {
        A.CallTo(() => _fakeQueueService.CreateQueueAsync(A<string>._, A<CancellationToken>._))
            .Throws(new Exception("Database error"));

        var cut = Render<CreateQueueDialog>(parameters => parameters
            .Add(p => p.IsOpen, true));

        var input = cut.Find("fluent-text-field");
        await cut.InvokeAsync(() => input.Change("test-queue")).ConfigureAwait(false);

        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit()).ConfigureAwait(false);

        await Task.Delay(100).ConfigureAwait(false); // Wait for async operation

        A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }
}
