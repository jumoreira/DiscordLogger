using DiscordLogger.Scopes;
using Xunit;

namespace DiscordLogger.Tests.Scopes;

public class DiscordScopeProviderTests
{
    [Fact]
    public void CurrentScope_InitiallyNull()
    {
        // Arrange
        using var provider = new DiscordScopeProvider();

        // Assert
        Assert.Null(provider.CurrentScope);
    }

    [Fact]
    public void Push_ShouldCreateNewScope()
    {
        // Arrange
        using var provider = new DiscordScopeProvider();
        var state = "test state";

        // Act
        using var scope = provider.Push(state);

        // Assert
        Assert.NotNull(provider.CurrentScope);
        Assert.Equal(state, provider.CurrentScope?.State);
    }

    [Fact]
    public void Push_MultipleTimes_ShouldCreateNestedScopes()
    {
        // Arrange
        using var provider = new DiscordScopeProvider();
        var state1 = "scope 1";
        var state2 = "scope 2";

        // Act
        using var scope1 = provider.Push(state1);
        using var scope2 = provider.Push(state2);

        // Assert
        Assert.NotNull(provider.CurrentScope);
        Assert.Equal(state2, provider.CurrentScope?.State);
        Assert.Equal(state1, provider.CurrentScope?.Parent?.State);
    }

    [Fact]
    public void Dispose_ShouldPopScope()
    {
        // Arrange
        using var provider = new DiscordScopeProvider();
        var state1 = "scope 1";
        var state2 = "scope 2";

        // Act
        using (var scope1 = provider.Push(state1))
        {
            using (var scope2 = provider.Push(state2))
            {
                Assert.Equal(state2, provider.CurrentScope?.State);
            }
            // Após dispose de scope2
            Assert.Equal(state1, provider.CurrentScope?.State);
        }
        // Após dispose de scope1
        Assert.Null(provider.CurrentScope);
    }

    [Fact]
    public void FormatScopes_WithNoScopes_ShouldReturnNull()
    {
        // Arrange
        using var provider = new DiscordScopeProvider();

        // Act
        var formatted = provider.FormatScopes();

        // Assert
        Assert.Null(formatted);
    }

    [Fact]
    public void FormatScopes_WithSingleScope_ShouldFormatCorrectly()
    {
        // Arrange
        using var provider = new DiscordScopeProvider();
        var state = "test scope";

        // Act
        using var scope = provider.Push(state);
        var formatted = provider.FormatScopes();

        // Assert
        Assert.NotNull(formatted);
        Assert.Contains("test scope", formatted);
    }

    [Fact]
    public void FormatScopes_WithNestedScopes_ShouldFormatHierarchy()
    {
        // Arrange
        using var provider = new DiscordScopeProvider();
        var state1 = "scope 1";
        var state2 = "scope 2";

        // Act
        using var scope1 = provider.Push(state1);
        using var scope2 = provider.Push(state2);
        var formatted = provider.FormatScopes();

        // Assert
        Assert.NotNull(formatted);
        Assert.Contains("scope 1", formatted);
        Assert.Contains("scope 2", formatted);
        Assert.Contains("=>", formatted);
    }

    [Fact]
    public void FormatScopes_WithKeyValuePairs_ShouldFormatCorrectly()
    {
        // Arrange
        using var provider = new DiscordScopeProvider();
        var kvps = new List<KeyValuePair<string, object>>
        {
            new("RequestId", "12345"),
            new("UserId", "user@example.com")
        };

        // Act
        using var scope = provider.Push(kvps);
        var formatted = provider.FormatScopes();

        // Assert
        Assert.NotNull(formatted);
        Assert.Contains("RequestId", formatted);
        Assert.Contains("12345", formatted);
        Assert.Contains("UserId", formatted);
        Assert.Contains("user@example.com", formatted);
    }

    [Fact]
    public void ProviderDispose_ShouldClearCurrentScope()
    {
        // Arrange
        var provider = new DiscordScopeProvider();
        using var scope = provider.Push("test");

        // Act
        provider.Dispose();

        // Assert
        Assert.Null(provider.CurrentScope);
    }
}
