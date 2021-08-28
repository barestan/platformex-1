using System;
using System.Collections.Generic;
using Platformex.Application;

namespace Platformex.Tests.TestHelpers
{
    public class ThingyState : AggregateStateEx<ThingyId, ThingyState, IThingyState>, IThingyState,
        ICanApply<ThingyDeletedEvent, ThingyId>, 
        ICanApply<ThingyDomainErrorAfterFirstEvent, ThingyId>,
        ICanApply<ThingyMessageAddedEvent, ThingyId>,
        ICanApply<ThingyMessageHistoryAddedEvent, ThingyId>,
        ICanApply<ThingyPingEvent, ThingyId>,
        ICanApply<ThingySagaCompleteRequestedEvent, ThingyId>,
        ICanApply<ThingySagaExceptionRequestedEvent, ThingyId>,
        ICanApply<ThingySagaStartRequestedEvent, ThingyId>
    {
        // IModel.Identity
        public Guid Id { get; set; }

        private readonly List<PingId> _pingsReceived = new List<PingId>();
        public IReadOnlyCollection<PingId> PingsReceived => _pingsReceived; 
        
        private readonly List<ThingyMessage> _messages = new List<ThingyMessage>();
        public IReadOnlyCollection<ThingyMessage> Messages => _messages;

        public bool IsDeleted { get; private set; }
        public bool DomainErrorAfterFirstReceived { get; private set; }


        public void Apply(ThingyDeletedEvent _) => IsDeleted = true;

        public void Apply(ThingyDomainErrorAfterFirstEvent _) => DomainErrorAfterFirstReceived = true;

        public void Apply(ThingyMessageAddedEvent e) => _messages.Add(e.ThingyMessage);

        public void Apply(ThingyMessageHistoryAddedEvent e) => _messages.AddRange(e.ThingyMessages);

        public void Apply(ThingyPingEvent e) => _pingsReceived.Add(e.PingId);

        public void Apply(ThingySagaCompleteRequestedEvent _) {}

        public void Apply(ThingySagaExceptionRequestedEvent _) {}

        public void Apply(ThingySagaStartRequestedEvent _) {}

        public ThingyState(IDbProvider<IThingyState> provider) : base(provider) {}
    }
}