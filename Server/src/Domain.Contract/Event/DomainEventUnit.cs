namespace Domain.Event;

[Alias("domain_event")]
[GenerateSerializer(GenerateFieldIds = GenerateFieldIds.PublicProperties)]
public sealed class DomainEventUnit
{
    public Guid EventId { get; set; }
    public required long GrainId { get; set; }
    public required int ETag { get; init; }
    public required DomainEventBase Value { get; init; }
    public required DateTime Timestamp { get; init; }
    public string Type => Value.GetType().Name;
}

public interface IDomainEventHandler<TDomainEvent> : Mediator.INotificationHandler<TDomainEvent>
    where TDomainEvent : DomainEventBase;
