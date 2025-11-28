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
        using var _ = new AssertionScope();
        const string jsonContent = "{\"test\": \"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent));

        cut.Markup.Should().NotBeNull();
    }

    [Test]
    public async Task ShowsTruncatedContent_WhenJsonLongerThanMaxLength()
    {
        using var _ = new AssertionScope();
        var longJson = new string('x', 200);

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, longJson)
            .Add(p => p.MaxLength, 50));

        cut.Markup.Should().Contain("...");
    }

    [Test]
    public async Task ShowsExpandButton_WhenContentIsTruncated()
    {
        using var _ = new AssertionScope();
        var longJson = new string('x', 200);

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, longJson)
            .Add(p => p.MaxLength, 50));

        var buttons = cut.FindAll("fluent-button");
        var expandButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Expand"));

        expandButton.Should().NotBeNull();
    }

    [Test]
    public async Task DoesNotShowExpandButton_WhenContentNotTruncated()
    {
        using var _ = new AssertionScope();
        const string shortJson = "{\"test\": \"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, shortJson)
            .Add(p => p.MaxLength, 100));

        var buttons = cut.FindAll("fluent-button");
        var expandButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Expand"));

        expandButton.Should().BeNull();
    }

    [Test]
    public async Task ShowsPrettyPrintedJson_WhenExpanded()
    {
        using var _ = new AssertionScope();
        const string jsonContent = "{\"test\":\"data\",\"nested\":{\"value\":123}}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 10));

        var expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));

        expandButton?.Click();

        await Task.Delay(50).ConfigureAwait(false); // Wait for state update

        var preElements = cut.FindAll("pre");
        preElements.Count.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task ShowsCollapseButton_WhenExpanded()
    {
        using var _ = new AssertionScope();
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

        collapseButton.Should().NotBeNull();
    }

    [Test]
    public async Task TogglesExpansion_WhenButtonClicked()
    {
        using var _ = new AssertionScope();
        const string jsonContent = "{\"test\":\"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 5));

        // Initially collapsed
        var expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));
        expandButton.Should().NotBeNull();

        // Click to expand
        expandButton?.Click();
        await Task.Delay(50).ConfigureAwait(false);

        // Now should show collapse button
        var collapseButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Collapse"));
        collapseButton.Should().NotBeNull();

        // Click to collapse
        collapseButton?.Click();
        await Task.Delay(50).ConfigureAwait(false);

        // Should show expand button again
        expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));
        expandButton.Should().NotBeNull();
    }

    [Test]
    public async Task HandlesInvalidJson_Gracefully()
    {
        using var _ = new AssertionScope();
        const string invalidJson = "not valid json at all";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, invalidJson));

        // Should still render without throwing
        cut.Markup.Should().NotBeNull();
    }

    [Test]
    public async Task UsesDefaultMaxLength_WhenNotSpecified()
    {
        using var _ = new AssertionScope();
        var jsonContent = new string('x', 150);

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent));

        // Default MaxLength is 100, so should be truncated
        cut.Markup.Should().Contain("...");
    }

    [Test]
    public async Task ShowsFullContent_WhenShorterThanMaxLength()
    {
        using var _ = new AssertionScope();
        const string jsonContent = "{\"short\": \"content\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 100));

        cut.Markup.Should().Contain(jsonContent);
    }
}
