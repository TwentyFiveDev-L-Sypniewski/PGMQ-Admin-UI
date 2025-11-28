using Bunit;
using PgmqAdminUI.Components.UI;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
[Obsolete]
public class JsonViewerTests : FluentTestBase
{
    [Test]
    public async Task RendersJsonContent()
    {
        const string jsonContent = "{\"test\": \"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent));

        await Assert.That(cut.Markup).IsNotNull();
    }

    [Test]
    public async Task ShowsTruncatedContent_WhenJsonLongerThanMaxLength()
    {
        var longJson = new string('x', 200);

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, longJson)
            .Add(p => p.MaxLength, 50));

        await Assert.That(cut.Markup).Contains("...");
    }

    [Test]
    public async Task ShowsExpandButton_WhenContentIsTruncated()
    {
        var longJson = new string('x', 200);

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, longJson)
            .Add(p => p.MaxLength, 50));

        var buttons = cut.FindAll("fluent-button");
        var expandButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Expand"));

        await Assert.That(expandButton).IsNotNull();
    }

    [Test]
    public async Task DoesNotShowExpandButton_WhenContentNotTruncated()
    {
        const string shortJson = "{\"test\": \"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, shortJson)
            .Add(p => p.MaxLength, 100));

        var buttons = cut.FindAll("fluent-button");
        var expandButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Expand"));

        await Assert.That(expandButton).IsNull();
    }

    [Test]
    public async Task ShowsPrettyPrintedJson_WhenExpanded()
    {
        const string jsonContent = "{\"test\":\"data\",\"nested\":{\"value\":123}}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 10));

        var expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));

        expandButton?.Click();

        await Task.Delay(50).ConfigureAwait(false); // Wait for state update

        var preElements = cut.FindAll("pre");
        await Assert.That(preElements.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task ShowsCollapseButton_WhenExpanded()
    {
        const string jsonContent = "{\"test\":\"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 5));

        var expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));

        expandButton?.Click();

        await Task.Delay(50).ConfigureAwait(false); // Wait for state update

        var collapseButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Collapse"));

        await Assert.That(collapseButton).IsNotNull();
    }

    [Test]
    public async Task TogglesExpansion_WhenButtonClicked()
    {
        const string jsonContent = "{\"test\":\"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 5));

        // Initially collapsed
        var expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));
        await Assert.That(expandButton).IsNotNull();

        // Click to expand
        expandButton?.Click();
        await Task.Delay(50).ConfigureAwait(false);

        // Now should show collapse button
        var collapseButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Collapse"));
        await Assert.That(collapseButton).IsNotNull();

        // Click to collapse
        collapseButton?.Click();
        await Task.Delay(50).ConfigureAwait(false);

        // Should show expand button again
        expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));
        await Assert.That(expandButton).IsNotNull();
    }

    [Test]
    public async Task HandlesInvalidJson_Gracefully()
    {
        const string invalidJson = "not valid json at all";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, invalidJson));

        // Should still render without throwing
        await Assert.That(cut.Markup).IsNotNull();
    }

    [Test]
    public async Task UsesDefaultMaxLength_WhenNotSpecified()
    {
        var jsonContent = new string('x', 150);

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent));

        // Default MaxLength is 100, so should be truncated
        await Assert.That(cut.Markup).Contains("...");
    }

    [Test]
    public async Task ShowsFullContent_WhenShorterThanMaxLength()
    {
        const string jsonContent = "{\"short\": \"content\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 100));

        await Assert.That(cut.Markup).Contains(jsonContent);
    }
}
