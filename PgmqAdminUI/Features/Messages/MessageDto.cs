namespace PgmqAdminUI.Features.Messages;

public sealed class MessageDto
{
    public required long MsgId { get; init; }
    public required string? Message { get; init; }
    public DateTimeOffset EnqueuedAt { get; init; }
    public DateTimeOffset? Vt { get; init; }
    public int ReadCount { get; init; }
}
