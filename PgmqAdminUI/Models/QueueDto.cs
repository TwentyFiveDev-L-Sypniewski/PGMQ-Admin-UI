namespace PgmqAdminUI.Models;

public sealed class QueueDto
{
    public required string Name { get; init; }
    public long TotalMessages { get; init; }
    public long InFlightMessages { get; init; }
    public long ArchivedMessages { get; init; }
}
