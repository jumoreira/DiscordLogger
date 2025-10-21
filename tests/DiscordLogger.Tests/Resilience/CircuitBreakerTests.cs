using DiscordLogger.Resilience.CircuitBreaker;
using Xunit;
using CBResilience = DiscordLogger.Resilience.CircuitBreaker;

namespace DiscordLogger.Tests.Resilience;

public class CircuitBreakerTests
{
    [Fact]
    public async Task CircuitBreaker_StartsInClosedState()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 3,
            OpenTimeoutSeconds = 5
        };
        var circuitBreaker = new CBResilience.CircuitBreaker(options);

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, circuitBreaker.State);
    }

    [Fact]
    public async Task CircuitBreaker_OpensAfterThresholdFailures()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 3,
            OpenTimeoutSeconds = 5
        };
        var circuitBreaker = new CBResilience.CircuitBreaker(options);

        // Act - Simula 3 falhas
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await circuitBreaker.ExecuteAsync(() => throw new Exception("Test failure"));
            }
            catch
            {
                // Esperado
            }
        }

        // Assert
        Assert.Equal(CircuitBreakerState.Open, circuitBreaker.State);
    }

    [Fact]
    public async Task CircuitBreaker_RejectsRequestsWhenOpen()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            OpenTimeoutSeconds = 5
        };
        var circuitBreaker = new CBResilience.CircuitBreaker(options);

        // Act - Abre o circuito
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await circuitBreaker.ExecuteAsync(() => throw new Exception("Test failure"));
            }
            catch
            {
                // Esperado
            }
        }

        // Assert - Próxima requisição deve ser rejeitada
        await Assert.ThrowsAsync<CircuitBreakerOpenException>(async () =>
        {
            await circuitBreaker.ExecuteAsync(async () => await Task.CompletedTask);
        });
    }

    [Fact]
    public async Task CircuitBreaker_TransitionsToHalfOpenAfterTimeout()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            OpenTimeoutSeconds = 1 // 1 segundo para teste rápido
        };
        var circuitBreaker = new CBResilience.CircuitBreaker(options);

        // Act - Abre o circuito
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await circuitBreaker.ExecuteAsync(() => throw new Exception("Test failure"));
            }
            catch
            {
                // Esperado
            }
        }

        Assert.Equal(CircuitBreakerState.Open, circuitBreaker.State);

        // Aguarda timeout
        await Task.Delay(1100);

        // Tenta executar - deve transitar para HalfOpen
        var executed = false;
        try
        {
            await circuitBreaker.ExecuteAsync(async () =>
            {
                executed = true;
                await Task.CompletedTask;
            });
        }
        catch
        {
            // Pode falhar ou não
        }

        // Assert - Deve ter permitido execução (HalfOpen)
        Assert.True(executed);
    }

    [Fact]
    public async Task CircuitBreaker_ClosesAfterSuccessInHalfOpen()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            OpenTimeoutSeconds = 1,
            SuccessThreshold = 0.5 // 50% de sucesso
        };
        var circuitBreaker = new CBResilience.CircuitBreaker(options);

        // Act - Abre o circuito
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await circuitBreaker.ExecuteAsync(() => throw new Exception("Test failure"));
            }
            catch { }
        }

        // Aguarda timeout
        await Task.Delay(1100);

        // Executa com sucesso em HalfOpen
        for (int i = 0; i < 5; i++)
        {
            await circuitBreaker.ExecuteAsync(async () => await Task.CompletedTask);
        }

        // Assert - Deve ter fechado o circuito
        Assert.Equal(CircuitBreakerState.Closed, circuitBreaker.State);
    }

    [Fact]
    public async Task CircuitBreaker_ReopensOnFailureInHalfOpen()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            OpenTimeoutSeconds = 1
        };
        var circuitBreaker = new CBResilience.CircuitBreaker(options);

        // Act - Abre o circuito
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await circuitBreaker.ExecuteAsync(() => throw new Exception("Test failure"));
            }
            catch { }
        }

        // Aguarda timeout
        await Task.Delay(1100);

        // Falha em HalfOpen
        try
        {
            await circuitBreaker.ExecuteAsync(() => throw new Exception("Test failure in half-open"));
        }
        catch { }

        // Assert - Deve ter reaberto o circuito
        Assert.Equal(CircuitBreakerState.Open, circuitBreaker.State);
    }

    [Fact]
    public async Task CircuitBreaker_TracksStatistics()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 5
        };
        var circuitBreaker = new CBResilience.CircuitBreaker(options);

        // Act
        await circuitBreaker.ExecuteAsync(async () => await Task.CompletedTask);
        await circuitBreaker.ExecuteAsync(async () => await Task.CompletedTask);

        try
        {
            await circuitBreaker.ExecuteAsync(() => throw new Exception("Test failure"));
        }
        catch { }

        var stats = circuitBreaker.GetStatistics();

        // Assert
        Assert.Equal(2, stats.TotalSuccesses);
        Assert.Equal(1, stats.TotalFailures);
        Assert.True(stats.SuccessRate > 0.5);
    }
}
