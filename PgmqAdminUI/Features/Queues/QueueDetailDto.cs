using PgmqAdminUI.Features.Messages;

namespace PgmqAdminUI.Features.Queues;

public sealed class QueueDetailDto
{
    public required string QueueName { get; init; }
    public IReadOnlyList<MessageDto> Messages { get; init; } = [];
    public long TotalCount { get; init; }
    public int PageSize { get; init; }
    public int CurrentPage { get; init; }
}
