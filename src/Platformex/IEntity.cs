namespace Platformex
{
    public interface IEntity<out TIdentity> 
    {
        TIdentity Id { get; }
    }
}