using Bunit;
using PgmqAdminUI.Components.UI;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
public class JsonViewerTests : FluentTestBase
{
    [Test]
    public async Task RendersJsonContent()
    {
        // Arrange
        const string jsonContent = "{\"test\": \"data\"}";

        // Act
        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().NotBeNull();
        });
    }

    [Test]
    public async Task ShowsTruncatedContent_WhenJsonLongerThanMaxLength()
    {
        // Arrange
        var longJson = new string('x', 200);

        // Act
        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, longJson)
            .Add(p => p.MaxLength, 50));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().Contain("...");
        });
    }

    [Test]
    public async Task ShowsExpandButton_WhenContentIsTruncated()
    {
        // Arrange
        var longJson = new string('x', 200);

        // Act
        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, longJson)
            .Add(p => p.MaxLength, 50));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var expandButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Expand"));
            expandButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task DoesNotShowExpandButton_WhenContentNotTruncated()
    {
        // Arrange
        const string shortJson = "{\"test\": \"data\"}";

        // Act
        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, shortJson)
            .Add(p => p.MaxLength, 100));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var expandButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Expand"));
            expandButton.Should().BeNull();
        });
    }

    [Test]
    public async Task ShowsPrettyPrintedJson_WhenExpanded()
    {
        // Arrange
        const string jsonContent = "{\"test\":\"data\",\"nested\":{\"value\":123}}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 10));

        // Act
        var expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));
        expandButton?.Click();

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var preElements = cut.FindAll("pre");
            preElements.Count.Should().BeGreaterThan(0);
        });
    }

    [Test]
    public async Task ShowsCollapseButton_WhenExpanded()
    {
        // Arrange
        const string jsonContent = "{\"test\":\"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 5));

        // Act
        var expandButton = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));
        expandButton?.Click();

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var collapseButton = cut.FindAll("fluent-button")
                .FirstOrDefault(b => b.TextContent.Contains("Collapse"));
            collapseButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task TogglesExpansion_WhenButtonClicked()
    {
        // Arrange
        const string jsonContent = "{\"test\":\"data\"}";

        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 5));

        // Initially collapsed - verify expand button exists
        await cut.WaitForAssertionAsync(() =>
        {
            var expandButton = cut.FindAll("fluent-button")
                .FirstOrDefault(b => b.TextContent.Contains("Expand"));
            expandButton.Should().NotBeNull();
        });

        // Act - Click to expand
        var expandBtn = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Expand"));
        expandBtn?.Click();

        // Now should show collapse button
        await cut.WaitForAssertionAsync(() =>
        {
            var collapseButton = cut.FindAll("fluent-button")
                .FirstOrDefault(b => b.TextContent.Contains("Collapse"));
            collapseButton.Should().NotBeNull();
        });

        // Act - Click to collapse
        var collapseBtn = cut.FindAll("fluent-button")
            .FirstOrDefault(b => b.TextContent.Contains("Collapse"));
        collapseBtn?.Click();

        // Should show expand button again
        await cut.WaitForAssertionAsync(() =>
        {
            var expandButton = cut.FindAll("fluent-button")
                .FirstOrDefault(b => b.TextContent.Contains("Expand"));
            expandButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task HandlesInvalidJson_Gracefully()
    {
        // Arrange
        const string invalidJson = "not valid json at all";

        // Act
        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, invalidJson));

        // Assert - Should still render without throwing
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().NotBeNull();
        });
    }

    [Test]
    public async Task UsesDefaultMaxLength_WhenNotSpecified()
    {
        // Arrange
        var jsonContent = new string('x', 150);

        // Act
        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent));

        // Assert - Default MaxLength is 100, so should be truncated
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().Contain("...");
        });
    }

    [Test]
    public async Task ShowsFullContent_WhenShorterThanMaxLength()
    {
        // Arrange
        const string jsonContent = "{\"short\": \"content\"}";

        // Act
        var cut = Render<JsonViewer>(parameters => parameters
            .Add(p => p.Content, jsonContent)
            .Add(p => p.MaxLength, 100));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().Contain(jsonContent);
        });
    }
}
