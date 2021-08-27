namespace Platformex
{
    public interface ICommand
    {
        ICommandMetadata Metadata { get; init; }
    }

    public interface ICommand<out T> : ICommand where T : Identity<T>
    {
        T Id { get; }
    }

    public abstract record Command : ICommand
    {
        public ICommandMetadata Metadata { get; init; }

        protected Command()
        {
            Metadata = new CommandMetadata(SourceId.New);
        }

        protected Command(ICommandMetadata metadata)
        {
            Metadata = metadata;
        }

    }
    
    
    
}