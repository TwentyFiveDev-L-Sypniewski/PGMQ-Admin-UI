using Npgmq;
using Npgsql;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Features.Messages;

public partial class MessageService
{
    private readonly NpgmqClient _pgmq;
    private readonly string _connectionString;
    private readonly ILogger<MessageService> _logger;

    public MessageService(string connectionString, ILogger<MessageService> logger)
    {
        _pgmq = new NpgmqClient(connectionString);
        _connectionString = connectionString;
        _logger = logger;
    }

    public virtual async Task<long> SendMessageAsync(string queueName, string jsonMessage, int? delaySeconds = null, CancellationToken ct = default)
    {
        try
        {
            var delay = delaySeconds ?? 0;
            var msgId = await _pgmq.SendAsync(queueName, jsonMessage, delay, ct);
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
            var deleted = await _pgmq.DeleteAsync(queueName, msgId, ct);
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
            var archived = await _pgmq.ArchiveAsync(queueName, msgId, ct);
            return archived;
        }
        catch (Exception ex)
        {
            LogArchiveMessageFailed(ex, msgId, queueName);
            return false;
        }
    }

    public virtual async Task<QueueDetailDto> GetArchivedMessagesAsync(
        string queueName,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            LogGettingArchivedMessages(queueName, page, pageSize);

            var offset = (page - 1) * pageSize;
            var sql = $"""
                SELECT msg_id, message, enqueued_at, vt, read_ct
                FROM pgmq.a_{queueName}
                ORDER BY enqueued_at DESC
                LIMIT {pageSize} OFFSET {offset}
                """;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            var messages = new List<MessageDto>();
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                messages.Add(new MessageDto
                {
                    MsgId = reader.GetInt64(0),
                    Message = reader.GetString(1),
                    EnqueuedAt = reader.GetFieldValue<DateTimeOffset>(2),
                    Vt = reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3),
                    ReadCount = reader.GetInt32(4)
                });
            }

            LogRetrievedArchivedMessages(queueName, messages.Count);

            return new QueueDetailDto
            {
                QueueName = queueName,
                Messages = messages,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = messages.Count
            };
        }
        catch (Exception ex)
        {
            LogErrorGettingArchivedMessages(queueName, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Message {MsgId} sent to {QueueName}")]
    partial void LogMessageSent(long msgId, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send message to {QueueName}")]
    partial void LogSendMessageFailed(Exception ex, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete message {MsgId} from {QueueName}")]
    partial void LogDeleteMessageFailed(Exception ex, long msgId, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to archive message {MsgId} from {QueueName}")]
    partial void LogArchiveMessageFailed(Exception ex, long msgId, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting archived messages for queue {QueueName}, page {Page}, page size {PageSize}")]
    partial void LogGettingArchivedMessages(string queueName, int page, int pageSize);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieved {Count} archived messages from queue {QueueName}")]
    partial void LogRetrievedArchivedMessages(string queueName, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error getting archived messages for queue {QueueName}")]
    partial void LogErrorGettingArchivedMessages(string queueName, Exception exception);
}
