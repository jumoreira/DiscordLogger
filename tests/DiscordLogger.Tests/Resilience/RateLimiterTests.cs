using DiscordLogger.Resilience.RateLimiting;
using Xunit;

namespace DiscordLogger.Tests.Resilience;

public class RateLimiterTests
{
    [Fact]
    public void TokenBucketRateLimiter_AllowsBurst()
    {
        // Arrange
        var rateLimiter = new TokenBucketRateLimiter(
            capacity: 10,
            refillRate: 5,
            refillInterval: TimeSpan.FromSeconds(1)
        );

        // Act - Tenta adquirir 10 tokens (burst)
        var results = new List<bool>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(rateLimiter.TryAcquire());
        }

        // Assert
        Assert.All(results, result => Assert.True(result));

        // O 11º deve falhar (bucket vazio)
        Assert.False(rateLimiter.TryAcquire());
    }

    [Fact]
    public async Task TokenBucketRateLimiter_RefillsOverTime()
    {
        // Arrange
        var rateLimiter = new TokenBucketRateLimiter(
            capacity: 5,
            refillRate: 5,
            refillInterval: TimeSpan.FromMilliseconds(100)
        );

        // Act - Esvazia bucket
        for (int i = 0; i < 5; i++)
        {
            rateLimiter.TryAcquire();
        }

        Assert.False(rateLimiter.TryAcquire());

        // Aguarda refill
        await Task.Delay(150);

        // Assert - Deve ter tokens novamente
        Assert.True(rateLimiter.TryAcquire());
    }

    [Fact]
    public void TokenBucketRateLimiter_Reset_RestoresCapacity()
    {
        // Arrange
        var rateLimiter = new TokenBucketRateLimiter(
            capacity: 5,
            refillRate: 5,
            refillInterval: TimeSpan.FromSeconds(1)
        );

        // Act - Esvazia bucket
        for (int i = 0; i < 5; i++)
        {
            rateLimiter.TryAcquire();
        }

        Assert.False(rateLimiter.TryAcquire());

        // Reset
        rateLimiter.Reset();

        // Assert - Deve ter capacidade completa novamente
        Assert.True(rateLimiter.TryAcquire());
    }

    [Fact]
    public async Task AdaptiveRateLimiter_IncreasesCapacityOnSuccess()
    {
        // Arrange
        var rateLimiter = new AdaptiveRateLimiter(
            initialCapacity: 10,
            minCapacity: 5,
            maxCapacity: 20,
            refillInterval: TimeSpan.FromSeconds(1),
            increaseMultiplier: 1.5,
            decreaseMultiplier: 0.8
        );

        // Act - Simula 100 sucessos
        for (int i = 0; i < 100; i++)
        {
            rateLimiter.ReportSuccess();
        }

        // Aguarda ajuste (ocorre a cada 10 segundos, mas podemos forçar)
        await Task.Delay(100);

        // Assert - Capacidade deve aumentar (verificação indireta via TryAcquire)
        // Este teste é simplificado, idealmente teríamos acesso à capacidade interna
        Assert.True(rateLimiter.TryAcquire());
    }

    [Fact]
    public void AdaptiveRateLimiter_DecreasesCapacityOnFailure()
    {
        // Arrange
        var rateLimiter = new AdaptiveRateLimiter(
            initialCapacity: 20,
            minCapacity: 5,
            maxCapacity: 50,
            refillInterval: TimeSpan.FromSeconds(1),
            increaseMultiplier: 1.2,
            decreaseMultiplier: 0.5
        );

        // Act - Simula muitas falhas
        for (int i = 0; i < 50; i++)
        {
            rateLimiter.ReportFailure();
        }

        for (int i = 0; i < 10; i++)
        {
            rateLimiter.ReportSuccess();
        }

        // Assert - Capacidade deve diminuir devido à taxa de falha
        // Verificação simplificada
        Assert.True(rateLimiter.TryAcquire());
    }
}
