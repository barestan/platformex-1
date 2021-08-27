using System;
using Platformex.Domain;

namespace Platformex.Tests.TestHelpers
{
    public class ThingyMessage : Entity<ThingyMessageId>
    {
        public ThingyMessage(
            ThingyMessageId id,
            string message)
            : base(id)
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

            Message = message;
        }

        public string Message { get; }
    }
}