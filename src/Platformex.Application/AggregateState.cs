using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platformex.Domain;

namespace Platformex.Application
{
    public abstract class AggregateState<TIdentity, TAggregateState> :
        IAggregateState<TIdentity> where TIdentity : Identity<TIdentity>
    {
        protected abstract Task<bool> LoadStateInternal(TIdentity id);
        public abstract Task BeginTransaction();
        public abstract Task CommitTransaction();
        public abstract Task RollbackTransaction();

        private static readonly IReadOnlyDictionary<Type, Action<TAggregateState, IAggregateEvent>> ApplyMethods; 

        static AggregateState()
        {
            ApplyMethods = typeof(TAggregateState).GetAggregateEventApplyMethods<TIdentity, TAggregateState>();
        }
        public TIdentity Identity { get; protected set; }

        public Task<bool> LoadState(TIdentity id)
        {
            Identity = id;
            return LoadStateInternal(id);
        }

        public async Task Apply(IAggregateEvent<TIdentity> e)
        {
            await BeforeApply(e);
            
            var aggregateEventType = e.GetType();
            Action<TAggregateState, IAggregateEvent> applier;

            if (!ApplyMethods.TryGetValue(aggregateEventType, out applier))
            {
                throw new MissingMethodException($"missing HandleAsync({aggregateEventType.Name})");
            }

            applier((TAggregateState) (object) this, e);
            
            await AfterApply(e);
        }

        protected virtual Task BeforeApply(IAggregateEvent<TIdentity> id) => Task.CompletedTask;

        protected  virtual Task AfterApply(IAggregateEvent<TIdentity> id) => Task.CompletedTask;

    }
}