namespace Platformex
{
    public abstract class Query<TResult> : IQuery<TResult>
    {
        public ICommonMetadata Metadata { get; init; }

        protected Query()
        {
            Metadata = new CommandMetadata();
        }

        protected Query(ICommandMetadata metadata)
        {
            Metadata = metadata;
        }
    }
}