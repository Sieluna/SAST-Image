namespace Domain.Event;

public sealed class Checkpoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime LastProcessedTimestamp { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
