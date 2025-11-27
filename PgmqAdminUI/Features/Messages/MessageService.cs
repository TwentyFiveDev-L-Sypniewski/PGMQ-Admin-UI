using Npgmq;

namespace PgmqAdminUI.Features.Messages;

public partial class MessageService
{
    private readonly NpgmqClient _pgmq;
    private readonly ILogger<MessageService> _logger;

    public MessageService(string connectionString, ILogger<MessageService> logger)
    {
        _pgmq = new NpgmqClient(connectionString);
        _logger = logger;
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Message {MsgId} sent to {QueueName}")]
    partial void LogMessageSent(long msgId, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send message to {QueueName}")]
    partial void LogSendMessageFailed(Exception ex, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete message {MsgId} from {QueueName}")]
    partial void LogDeleteMessageFailed(Exception ex, long msgId, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to archive message {MsgId} from {QueueName}")]
    partial void LogArchiveMessageFailed(Exception ex, long msgId, string queueName);
}
