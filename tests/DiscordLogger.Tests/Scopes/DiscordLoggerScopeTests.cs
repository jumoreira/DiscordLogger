using DiscordLogger.Scopes;
using Xunit;

namespace DiscordLogger.Tests.Scopes;

public class DiscordLoggerScopeTests
{
    [Fact]
    public void Constructor_ShouldSetStateAndParent()
    {
        // Arrange
        var state = "test state";
        var parentState = "parent state";
        var parent = new DiscordLoggerScope(parentState, null);

        // Act
        var scope = new DiscordLoggerScope(state, parent);

        // Assert
        Assert.Equal(state, scope.State);
        Assert.Equal(parent, scope.Parent);
    }

    [Fact]
    public void GetScopes_WithSingleScope_ShouldReturnOneItem()
    {
        // Arrange
        var state = "test state";
        var scope = new DiscordLoggerScope(state, null);

        // Act
        var scopes = scope.GetScopes().ToList();

        // Assert
        Assert.Single(scopes);
        Assert.Equal(state, scopes[0]);
    }

    [Fact]
    public void GetScopes_WithNestedScopes_ShouldReturnInCorrectOrder()
    {
        // Arrange
        var state1 = "scope 1";
        var state2 = "scope 2";
        var state3 = "scope 3";

        var scope1 = new DiscordLoggerScope(state1, null);
        var scope2 = new DiscordLoggerScope(state2, scope1);
        var scope3 = new DiscordLoggerScope(state3, scope2);

        // Act
        var scopes = scope3.GetScopes().ToList();

        // Assert
        Assert.Equal(3, scopes.Count);
        Assert.Equal(state1, scopes[0]);
        Assert.Equal(state2, scopes[1]);
        Assert.Equal(state3, scopes[2]);
    }

    [Fact]
    public void GetScopes_WithNullState_ShouldSkipNull()
    {
        // Arrange
        var state1 = "scope 1";
        var scope1 = new DiscordLoggerScope(state1, null);
        var scope2 = new DiscordLoggerScope(null, scope1);
        var state3 = "scope 3";
        var scope3 = new DiscordLoggerScope(state3, scope2);

        // Act
        var scopes = scope3.GetScopes().ToList();

        // Assert
        Assert.Equal(2, scopes.Count);
        Assert.Equal(state1, scopes[0]);
        Assert.Equal(state3, scopes[1]);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var scope = new DiscordLoggerScope("test", null);

        // Act & Assert
        var exception = Record.Exception(() => scope.Dispose());
        Assert.Null(exception);
    }
}
