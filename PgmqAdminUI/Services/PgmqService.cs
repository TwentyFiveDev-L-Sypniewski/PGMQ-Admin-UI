using Npgmq;
using PgmqAdminUI.Models;

namespace PgmqAdminUI.Services;

public partial class PgmqService
{
    private readonly NpgmqClient _pgmq;
    private readonly ILogger<PgmqService> _logger;

    public PgmqService(string connectionString, ILogger<PgmqService> logger)
    {
        _pgmq = new NpgmqClient(connectionString);
        _logger = logger;
    }

    public virtual async Task<IEnumerable<QueueDto>> ListQueuesAsync(CancellationToken ct = default)
    {
        try
        {
            var queues = await _pgmq.ListQueuesAsync(ct).ConfigureAwait(false);
            return queues.Select(q => new QueueDto
            {
                Name = q.QueueName,
                TotalMessages = 0,
                InFlightMessages = 0,
                ArchivedMessages = 0
            });
        }
        catch (Exception ex)
        {
            LogListQueuesFailed(ex);
            throw;
        }
    }

    public virtual async Task<QueueDetailDto> GetQueueDetailAsync(string queueName, int page, int pageSize, CancellationToken ct = default)
    {
        try
        {
            var messages = await _pgmq.ReadBatchAsync<string>(queueName, vt: 0, limit: pageSize, ct).ConfigureAwait(false);

            return new QueueDetailDto
            {
                QueueName = queueName,
                Messages = messages.Select(m => new MessageDto
                {
                    MsgId = m.MsgId,
                    Message = m.Message,
                    EnqueuedAt = m.EnqueuedAt,
                    Vt = m.Vt,
                    ReadCount = m.ReadCt
                }).ToList(),
                TotalCount = messages.Count,
                PageSize = pageSize,
                CurrentPage = page
            };
        }
        catch (Exception ex)
        {
            LogGetQueueDetailFailed(ex, queueName);
            throw;
        }
    }

    public virtual async Task<long> SendMessageAsync(string queueName, string jsonMessage, int? delaySeconds = null, CancellationToken ct = default)
    {
        try
        {
            var delay = delaySeconds ?? 0;
            var msgId = await _pgmq.SendAsync(queueName, jsonMessage, delay, ct).ConfigureAwait(false);
            LogMessageSent(msgId, queueName);
            return msgId;
        }
        catch (Exception ex)
        {
            LogSendMessageFailed(ex, queueName);
            throw;
        }
    }

    public virtual async Task<bool> DeleteMessageAsync(string queueName, long msgId, CancellationToken ct = default)
    {
        try
        {
            var deleted = await _pgmq.DeleteAsync(queueName, msgId, ct).ConfigureAwait(false);
            return deleted;
        }
        catch (Exception ex)
        {
            LogDeleteMessageFailed(ex, msgId, queueName);
            return false;
        }
    }

    public virtual async Task<bool> ArchiveMessageAsync(string queueName, long msgId, CancellationToken ct = default)
    {
        try
        {
            var archived = await _pgmq.ArchiveAsync(queueName, msgId, ct).ConfigureAwait(false);
            return archived;
        }
        catch (Exception ex)
        {
            LogArchiveMessageFailed(ex, msgId, queueName);
            return false;
        }
    }

    public virtual async Task CreateQueueAsync(string queueName, CancellationToken ct = default)
    {
        try
        {
            await _pgmq.CreateQueueAsync(queueName, ct).ConfigureAwait(false);
            LogQueueCreated(queueName);
        }
        catch (Exception ex)
        {
            LogCreateQueueFailed(ex, queueName);
            throw;
        }
    }

    public virtual async Task<bool> DeleteQueueAsync(string queueName, CancellationToken ct = default)
    {
        try
        {
            await _pgmq.DropQueueAsync(queueName, ct).ConfigureAwait(false);
            LogQueueDeleted(queueName);
            return true;
        }
        catch (Exception ex)
        {
            LogDeleteQueueFailed(ex, queueName);
            return false;
        }
    }

    // High-performance logging using source generation
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to list queues")]
    partial void LogListQueuesFailed(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get queue detail for {QueueName}")]
    partial void LogGetQueueDetailFailed(Exception ex, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Message {MsgId} sent to {QueueName}")]
    partial void LogMessageSent(long msgId, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send message to {QueueName}")]
    partial void LogSendMessageFailed(Exception ex, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete message {MsgId} from {QueueName}")]
    partial void LogDeleteMessageFailed(Exception ex, long msgId, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to archive message {MsgId} from {QueueName}")]
    partial void LogArchiveMessageFailed(Exception ex, long msgId, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Queue {QueueName} created")]
    partial void LogQueueCreated(string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create queue {QueueName}")]
    partial void LogCreateQueueFailed(Exception ex, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Queue {QueueName} deleted")]
    partial void LogQueueDeleted(string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete queue {QueueName}")]
    partial void LogDeleteQueueFailed(Exception ex, string queueName);
}
