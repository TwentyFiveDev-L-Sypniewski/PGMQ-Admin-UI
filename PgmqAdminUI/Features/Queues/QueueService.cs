using Npgmq;
using Npgsql;
using PgmqAdminUI.Features.Messages;

namespace PgmqAdminUI.Features.Queues;

public partial class QueueService
{
    private readonly NpgmqClient _pgmq;
    private readonly string _connectionString;
    private readonly ILogger<QueueService> _logger;

    public QueueService(string connectionString, ILogger<QueueService> logger)
    {
        _pgmq = new NpgmqClient(connectionString);
        _connectionString = connectionString;
        _logger = logger;
    }

    public virtual async Task<IEnumerable<QueueDto>> ListQueuesAsync(CancellationToken ct = default)
    {
        try
        {
            var queues = await _pgmq.ListQueuesAsync(ct);
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
            var messages = await _pgmq.ReadBatchAsync<string>(queueName, vt: 0, limit: pageSize, ct);

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

    public virtual async Task CreateQueueAsync(string queueName, CancellationToken ct = default)
    {
        try
        {
            await _pgmq.CreateQueueAsync(queueName, ct);
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
            await _pgmq.DropQueueAsync(queueName, ct);
            LogQueueDeleted(queueName);
            return true;
        }
        catch (Exception ex)
        {
            LogDeleteQueueFailed(ex, queueName);
            return false;
        }
    }

    public virtual async Task<QueueStatsDto?> GetQueueStatsAsync(string queueName, CancellationToken ct = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var command = new NpgsqlCommand("SELECT * FROM pgmq.metrics($1)", connection);
            command.Parameters.AddWithValue(queueName);

            await using var reader = await command.ExecuteReaderAsync(ct);

            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return new QueueStatsDto
                {
                    QueueName = reader.GetString(0),
                    QueueLength = reader.GetInt64(1),
                    NewestMsgAgeSec = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    OldestMsgAgeSec = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    TotalMessages = reader.GetInt64(4),
                    ScrapeTime = reader.GetFieldValue<DateTimeOffset>(5)
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            LogGetQueueStatsFailed(ex, queueName);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to list queues")]
    partial void LogListQueuesFailed(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get queue detail for {QueueName}")]
    partial void LogGetQueueDetailFailed(Exception ex, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Queue {QueueName} created")]
    partial void LogQueueCreated(string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create queue {QueueName}")]
    partial void LogCreateQueueFailed(Exception ex, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Queue {QueueName} deleted")]
    partial void LogQueueDeleted(string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete queue {QueueName}")]
    partial void LogDeleteQueueFailed(Exception ex, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get queue stats for {QueueName}")]
    partial void LogGetQueueStatsFailed(Exception ex, string queueName);
}
