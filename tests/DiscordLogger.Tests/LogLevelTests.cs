namespace DiscordLogger.Tests;

public class LogLevelTests
{
    [Fact]
    public void LogLevel_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)LogLevel.Debug);
        Assert.Equal(1, (int)LogLevel.Information);
        Assert.Equal(2, (int)LogLevel.Warning);
        Assert.Equal(3, (int)LogLevel.Error);
        Assert.Equal(4, (int)LogLevel.Critical);
    }

    [Theory]
    [InlineData(LogLevel.Debug, "Debug")]
    [InlineData(LogLevel.Information, "Information")]
    [InlineData(LogLevel.Warning, "Warning")]
    [InlineData(LogLevel.Error, "Error")]
    [InlineData(LogLevel.Critical, "Critical")]
    public void LogLevel_ToString_ShouldReturnCorrectName(LogLevel level, string expectedName)
    {
        // Act
        var name = level.ToString();

        // Assert
        Assert.Equal(expectedName, name);
    }

    [Fact]
    public void LogLevel_Comparison_ShouldWorkCorrectly()
    {
        // Assert
        Assert.True(LogLevel.Debug < LogLevel.Information);
        Assert.True(LogLevel.Information < LogLevel.Warning);
        Assert.True(LogLevel.Warning < LogLevel.Error);
        Assert.True(LogLevel.Error < LogLevel.Critical);
    }
}
