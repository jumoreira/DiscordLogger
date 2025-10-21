using DiscordLogger.Filters;
using Microsoft.Extensions.Logging;
using Xunit;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DiscordLogger.Tests.Filters;

public class LogFilterTests
{
    [Fact]
    public void ShouldLog_WithNoFilters_ShouldReturnTrue()
    {
        // Arrange
        var options = new LogFilterOptions();
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(
            MsLogLevel.Information,
            new EventId(1, "Test"),
            "TestCategory",
            "Test message");

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("MyApp.Controllers", true)]
    [InlineData("MyApp.Services", true)]
    [InlineData("OtherApp.Controllers", false)]
    public void ShouldLog_WithIncludeCategories_ShouldFilterCorrectly(string category, bool expected)
    {
        // Arrange
        var options = new LogFilterOptions
        {
            IncludeCategories = new List<string> { "MyApp.*" }
        };
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(MsLogLevel.Information, new EventId(1), category, "Test");

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("MyApp.Controllers", false)]
    [InlineData("MyApp.Services", true)]
    [InlineData("OtherApp.Controllers", true)]
    public void ShouldLog_WithExcludeCategories_ShouldFilterCorrectly(string category, bool expected)
    {
        // Arrange
        var options = new LogFilterOptions
        {
            ExcludeCategories = new List<string> { "MyApp.Controllers" }
        };
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(MsLogLevel.Information, new EventId(1), category, "Test");

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(200, true)]
    [InlineData(300, false)]
    public void ShouldLog_WithIncludeEventIds_ShouldFilterCorrectly(int eventId, bool expected)
    {
        // Arrange
        var options = new LogFilterOptions
        {
            IncludeEventIds = new List<int> { 100, 200 }
        };
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(
            MsLogLevel.Information,
            new EventId(eventId),
            "TestCategory",
            "Test");

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100, false)]
    [InlineData(200, true)]
    public void ShouldLog_WithExcludeEventIds_ShouldFilterCorrectly(int eventId, bool expected)
    {
        // Arrange
        var options = new LogFilterOptions
        {
            ExcludeEventIds = new List<int> { 100 }
        };
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(
            MsLogLevel.Information,
            new EventId(eventId),
            "TestCategory",
            "Test");

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("User logged in successfully", false)]
    [InlineData("Error: Database connection failed", true)]
    [InlineData("Normal message", true)]
    public void ShouldLog_WithMessagePatternsBlacklist_ShouldFilterCorrectly(string message, bool expected)
    {
        // Arrange
        var options = new LogFilterOptions
        {
            MessagePatterns = new List<string> { "User logged in.*" },
            MessagePatternsAsWhitelist = false
        };
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(MsLogLevel.Information, new EventId(1), "TestCategory", message);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("User logged in successfully", true)]
    [InlineData("Error: Database connection failed", false)]
    [InlineData("Normal message", false)]
    public void ShouldLog_WithMessagePatternsWhitelist_ShouldFilterCorrectly(string message, bool expected)
    {
        // Arrange
        var options = new LogFilterOptions
        {
            MessagePatterns = new List<string> { "User logged in.*" },
            MessagePatternsAsWhitelist = true
        };
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(MsLogLevel.Information, new EventId(1), "TestCategory", message);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(MsLogLevel.Debug, false)]
    [InlineData(MsLogLevel.Information, true)]
    [InlineData(MsLogLevel.Warning, true)]
    [InlineData(MsLogLevel.Error, false)]
    public void ShouldLog_WithEnabledLevels_ShouldFilterCorrectly(MsLogLevel level, bool expected)
    {
        // Arrange
        var options = new LogFilterOptions
        {
            EnabledLevels = new Dictionary<MsLogLevel, bool>
            {
                { MsLogLevel.Information, true },
                { MsLogLevel.Warning, true },
                { MsLogLevel.Error, false }
            }
        };
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(level, new EventId(1), "TestCategory", "Test");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShouldLog_WithInvalidRegex_ShouldNotThrow()
    {
        // Arrange
        var options = new LogFilterOptions
        {
            MessagePatterns = new List<string> { "[invalid(regex" }
        };

        // Act & Assert
        var exception = Record.Exception(() => new LogFilter(options));
        Assert.Null(exception);
    }

    [Fact]
    public void ShouldLog_WithEventIdZero_ShouldAlwaysPass()
    {
        // Arrange
        var options = new LogFilterOptions
        {
            IncludeEventIds = new List<int> { 100 }
        };
        var filter = new LogFilter(options);

        // Act
        var result = filter.ShouldLog(
            MsLogLevel.Information,
            new EventId(0),
            "TestCategory",
            "Test");

        // Assert
        Assert.True(result);
    }
}
