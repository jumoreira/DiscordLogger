using DiscordLogger.Batching;
using Xunit;

namespace DiscordLogger.Tests.Batching;

public class LogBatchProcessorTests
{
    [Fact]
    public async Task EnqueueMessage_ShouldAcceptMessage()
    {
        // Arrange
        var options = new BatchingOptions { MaxBatchSize = 5, FlushIntervalSeconds = 1 };
        var batches = new List<IList<QueuedLogMessage>>();
        using var processor = new LogBatchProcessor(options, batch =>
        {
            batches.Add(batch);
            return Task.CompletedTask;
        });

        var message = new QueuedLogMessage
        {
            Level = LogLevel.Information,
            Message = "Test message"
        };

        // Act
        var result = processor.EnqueueMessage(message);

        // Assert
        Assert.True(result);

        // Wait for processing
        await Task.Delay(1500);
        Assert.NotEmpty(batches);
    }

    [Fact]
    public async Task ProcessBatches_ShouldSendBatchWhenFull()
    {
        // Arrange
        var options = new BatchingOptions { MaxBatchSize = 3, FlushIntervalSeconds = 10 };
        var batches = new List<IList<QueuedLogMessage>>();
        using var processor = new LogBatchProcessor(options, batch =>
        {
            batches.Add(new List<QueuedLogMessage>(batch));
            return Task.CompletedTask;
        });

        // Act
        for (int i = 0; i < 3; i++)
        {
            processor.EnqueueMessage(new QueuedLogMessage
            {
                Level = LogLevel.Information,
                Message = $"Message {i}"
            });
        }

        // Wait for processing
        await Task.Delay(500);

        // Assert
        Assert.Single(batches);
        Assert.Equal(3, batches[0].Count);
    }

    [Fact]
    public async Task ProcessBatches_ShouldFlushOnTimeout()
    {
        // Arrange
        var options = new BatchingOptions { MaxBatchSize = 10, FlushIntervalSeconds = 1 };
        var batches = new List<IList<QueuedLogMessage>>();
        using var processor = new LogBatchProcessor(options, batch =>
        {
            batches.Add(new List<QueuedLogMessage>(batch));
            return Task.CompletedTask;
        });

        // Act
        processor.EnqueueMessage(new QueuedLogMessage
        {
            Level = LogLevel.Information,
            Message = "Test message"
        });

        // Wait for timeout flush
        await Task.Delay(1500);

        // Assert
        Assert.Single(batches);
        Assert.Single(batches[0]);
    }

    [Fact]
    public async Task ProcessBatches_ShouldHandleMultipleBatches()
    {
        // Arrange
        var options = new BatchingOptions { MaxBatchSize = 2, FlushIntervalSeconds = 10 };
        var batches = new List<IList<QueuedLogMessage>>();
        using var processor = new LogBatchProcessor(options, batch =>
        {
            batches.Add(new List<QueuedLogMessage>(batch));
            return Task.CompletedTask;
        });

        // Act
        for (int i = 0; i < 5; i++)
        {
            processor.EnqueueMessage(new QueuedLogMessage
            {
                Level = LogLevel.Information,
                Message = $"Message {i}"
            });
        }

        // Wait for processing
        await Task.Delay(1000);

        // Assert
        Assert.True(batches.Count >= 2);
        Assert.Equal(2, batches[0].Count);
        Assert.Equal(2, batches[1].Count);
    }

    [Fact]
    public async Task Dispose_ShouldFlushRemainingMessages()
    {
        // Arrange
        var options = new BatchingOptions { MaxBatchSize = 10, FlushIntervalSeconds = 100 };
        var batches = new List<IList<QueuedLogMessage>>();
        var processor = new LogBatchProcessor(options, batch =>
        {
            batches.Add(new List<QueuedLogMessage>(batch));
            return Task.CompletedTask;
        });

        // Act
        processor.EnqueueMessage(new QueuedLogMessage
        {
            Level = LogLevel.Information,
            Message = "Test message"
        });

        processor.Dispose();

        // Assert
        await Task.Delay(100);
        Assert.Single(batches);
        Assert.Single(batches[0]);
    }

    [Fact]
    public void EnqueueMessage_AfterDispose_ShouldReturnFalse()
    {
        // Arrange
        var options = new BatchingOptions();
        var processor = new LogBatchProcessor(options, _ => Task.CompletedTask);
        processor.Dispose();

        var message = new QueuedLogMessage
        {
            Level = LogLevel.Information,
            Message = "Test"
        };

        // Act
        var result = processor.EnqueueMessage(message);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ProcessBatches_WithError_ShouldContinueProcessing()
    {
        // Arrange
        var options = new BatchingOptions { MaxBatchSize = 2, FlushIntervalSeconds = 10 };
        var batches = new List<IList<QueuedLogMessage>>();
        var callCount = 0;

        using var processor = new LogBatchProcessor(options, batch =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new Exception("Test error");
            }
            batches.Add(new List<QueuedLogMessage>(batch));
            return Task.CompletedTask;
        });

        // Act
        for (int i = 0; i < 4; i++)
        {
            processor.EnqueueMessage(new QueuedLogMessage
            {
                Level = LogLevel.Information,
                Message = $"Message {i}"
            });
        }

        // Wait for processing
        await Task.Delay(1000);

        // Assert
        Assert.Equal(2, callCount);
        Assert.Single(batches); // Second batch should succeed
    }
}
