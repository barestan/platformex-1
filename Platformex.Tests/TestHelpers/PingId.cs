using System;

namespace Platformex.Tests.TestHelpers
{
    public class PingId : SingleValueObject<string>
    {
        public static PingId New => new PingId(Guid.NewGuid().ToString());
        public static PingId With(string value) { return new PingId(value); }

        public PingId(string value) : base (value)
        {
        }
    }
}