﻿
namespace Platformex
{
    public class EventId : Identity<EventId>, IEventId
    {
        public EventId(string value)
            : base(value)
        {
        }
    }
}
