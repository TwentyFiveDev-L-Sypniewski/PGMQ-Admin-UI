namespace PgmqAdminUI.Features.Queues;

public sealed class QueueStatsDto
{
    public required string QueueName { get; init; }
    public long QueueLength { get; init; }
    public int? NewestMsgAgeSec { get; init; }
    public int? OldestMsgAgeSec { get; init; }
    public long TotalMessages { get; init; }
    public DateTimeOffset ScrapeTime { get; init; }
}
