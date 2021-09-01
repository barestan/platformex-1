namespace Platformex.Tests.TestHelpers
{
    public class Thingy : Entity<ThingyId>
    {
        public int PingsReceived { get; }
        public bool DomainErrorAfterFirstReceived { get; }

        public Thingy(
            ThingyId id,
            int pingsReceived,
            bool domainErrorAfterFirstReceived)
            : base(id)
        {
            PingsReceived = pingsReceived;
            DomainErrorAfterFirstReceived = domainErrorAfterFirstReceived;
        }
    }
}