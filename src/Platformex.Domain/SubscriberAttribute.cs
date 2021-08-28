using Orleans;

namespace Platformex.Domain
{
    public class SubscriberAttribute : ImplicitStreamSubscriptionAttribute
    {
        public SubscriberAttribute() : base("InitializeSubscriptions")
        {

        }
    }
}
