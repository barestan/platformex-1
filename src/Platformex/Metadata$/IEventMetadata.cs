namespace Platformex
{
    public interface IEventMetadata : ICommonMetadata
    {
        IEventId EventId { get; }
        string EventName { get; }
        int EventVersion { get; }
        string CausationId { get; }

    }
    public interface IEventId : ISourceId
    {
    }
}