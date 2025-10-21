using DiscordLogger.Resilience.Backoff;
using Xunit;

namespace DiscordLogger.Tests.Resilience;

public class BackoffStrategyTests
{
    [Fact]
    public void ExponentialBackoff_IncreasesExponentially()
    {
        // Arrange
        var strategy = new ExponentialBackoffStrategy(
            initialDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromMinutes(5),
            multiplier: 2.0,
            useJitter: false
        );

        // Act
        var delay1 = strategy.GetDelay(1);
        var delay2 = strategy.GetDelay(2);
        var delay3 = strategy.GetDelay(3);

        // Assert
        Assert.Equal(1, delay1.TotalSeconds, 1); // ~1s
        Assert.Equal(2, delay2.TotalSeconds, 1); // ~2s
        Assert.Equal(4, delay3.TotalSeconds, 1); // ~4s
    }

    [Fact]
    public void ExponentialBackoff_RespectsMaxDelay()
    {
        // Arrange
        var strategy = new ExponentialBackoffStrategy(
            initialDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromSeconds(10),
            multiplier: 2.0,
            useJitter: false
        );

        // Act
        var delay = strategy.GetDelay(10); // 1 * 2^9 = 512s, mas max é 10s

        // Assert
        Assert.True(delay.TotalSeconds <= 10);
    }

    [Fact]
    public void ExponentialBackoff_WithJitter_VariesDelay()
    {
        // Arrange
        var strategy = new ExponentialBackoffStrategy(
            initialDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromMinutes(5),
            multiplier: 2.0,
            useJitter: true
        );

        // Act - Coleta múltiplos delays para a mesma tentativa
        var delays = new List<TimeSpan>();
        for (int i = 0; i < 10; i++)
        {
            delays.Add(strategy.GetDelay(3));
        }

        // Assert - Deve haver variação (jitter)
        var distinct = delays.Distinct().Count();
        Assert.True(distinct > 1, "Jitter deveria produzir delays variados");
    }

    [Fact]
    public void LinearBackoff_IncreasesLinearly()
    {
        // Arrange
        var strategy = new LinearBackoffStrategy(
            increment: TimeSpan.FromSeconds(2),
            maxDelay: TimeSpan.FromMinutes(5),
            useJitter: false
        );

        // Act
        var delay1 = strategy.GetDelay(1);
        var delay2 = strategy.GetDelay(2);
        var delay3 = strategy.GetDelay(3);

        // Assert
        Assert.Equal(2, delay1.TotalSeconds, 1); // ~2s
        Assert.Equal(4, delay2.TotalSeconds, 1); // ~4s
        Assert.Equal(6, delay3.TotalSeconds, 1); // ~6s
    }

    [Fact]
    public void LinearBackoff_RespectsMaxDelay()
    {
        // Arrange
        var strategy = new LinearBackoffStrategy(
            increment: TimeSpan.FromSeconds(5),
            maxDelay: TimeSpan.FromSeconds(20),
            useJitter: false
        );

        // Act
        var delay = strategy.GetDelay(10); // 5 * 10 = 50s, mas max é 20s

        // Assert
        Assert.True(delay.TotalSeconds <= 20);
    }

    [Fact]
    public void FibonacciBackoff_FollowsFibonacciSequence()
    {
        // Arrange
        var strategy = new FibonacciBackoffStrategy(
            baseDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromMinutes(5),
            useJitter: false
        );

        // Act
        var delay1 = strategy.GetDelay(1); // Fib(1) = 1
        var delay2 = strategy.GetDelay(2); // Fib(2) = 1
        var delay3 = strategy.GetDelay(3); // Fib(3) = 2
        var delay4 = strategy.GetDelay(4); // Fib(4) = 3
        var delay5 = strategy.GetDelay(5); // Fib(5) = 5

        // Assert (aproximado devido a jitter desabilitado)
        Assert.Equal(1, delay1.TotalSeconds, 1);
        Assert.Equal(1, delay2.TotalSeconds, 1);
        Assert.Equal(2, delay3.TotalSeconds, 1);
        Assert.Equal(3, delay4.TotalSeconds, 1);
        Assert.Equal(5, delay5.TotalSeconds, 1);
    }

    [Fact]
    public void FibonacciBackoff_RespectsMaxDelay()
    {
        // Arrange
        var strategy = new FibonacciBackoffStrategy(
            baseDelay: TimeSpan.FromSeconds(1),
            maxDelay: TimeSpan.FromSeconds(10),
            useJitter: false
        );

        // Act
        var delay = strategy.GetDelay(15); // Fib(15) = 610, mas max é 10s

        // Assert
        Assert.True(delay.TotalSeconds <= 10);
    }

    [Fact]
    public void AllStrategies_HandleZeroOrNegativeAttempt()
    {
        // Arrange
        var exponential = new ExponentialBackoffStrategy();
        var linear = new LinearBackoffStrategy();
        var fibonacci = new FibonacciBackoffStrategy();

        // Act & Assert - Não deve lançar exceção
        Assert.True(exponential.GetDelay(0) >= TimeSpan.Zero);
        Assert.True(linear.GetDelay(0) >= TimeSpan.Zero);
        Assert.True(fibonacci.GetDelay(0) >= TimeSpan.Zero);
        
        Assert.True(exponential.GetDelay(-1) >= TimeSpan.Zero);
        Assert.True(linear.GetDelay(-1) >= TimeSpan.Zero);
        Assert.True(fibonacci.GetDelay(-1) >= TimeSpan.Zero);
    }
}
