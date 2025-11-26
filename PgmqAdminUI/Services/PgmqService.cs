using Npgmq;
using PgmqAdminUI.Models;

namespace PgmqAdminUI.Services;

public class PgmqService
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
            _logger.LogError(ex, "Failed to list queues");
            throw;
        }
    }

    public virtual async Task<QueueDetailDto> GetQueueDetailAsync(string queueName, int page, int pageSize, CancellationToken ct = default)
    {
        try
        {
            var messages = await _pgmq.ReadBatchAsync<string>(queueName, vt: 0, batchSize: pageSize, ct).ConfigureAwait(false);

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
            _logger.LogError(ex, "Failed to get queue detail for {QueueName}", queueName);
            throw;
        }
    }

    public virtual async Task<long> SendMessageAsync(string queueName, string jsonMessage, int? delaySeconds = null, CancellationToken ct = default)
    {
        try
        {
            var delay = delaySeconds ?? 0;
            var msgId = await _pgmq.SendAsync(queueName, jsonMessage, delay, ct).ConfigureAwait(false);
            _logger.LogInformation("Message {MsgId} sent to {QueueName}", msgId, queueName);
            return msgId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to {QueueName}", queueName);
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
            _logger.LogError(ex, "Failed to delete message {MsgId} from {QueueName}", msgId, queueName);
            return false;
        }
    }

    public virtual async Task<bool> ArchiveMessageAsync(string queueName, long msgId, CancellationToken ct = default)
    {
        try
        {
            var archived = await _pgmq.ArchiveAsync(queueName, [msgId], ct).ConfigureAwait(false);
            return archived.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive message {MsgId} from {QueueName}", msgId, queueName);
            return false;
        }
    }

    public virtual async Task CreateQueueAsync(string queueName, CancellationToken ct = default)
    {
        try
        {
            await _pgmq.CreateQueueAsync(queueName, ct).ConfigureAwait(false);
            _logger.LogInformation("Queue {QueueName} created", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create queue {QueueName}", queueName);
            throw;
        }
    }

    public virtual async Task<bool> DeleteQueueAsync(string queueName, CancellationToken ct = default)
    {
        try
        {
            await _pgmq.DropQueueAsync(queueName, ct).ConfigureAwait(false);
            _logger.LogInformation("Queue {QueueName} deleted", queueName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete queue {QueueName}", queueName);
            return false;
        }
    }
}
