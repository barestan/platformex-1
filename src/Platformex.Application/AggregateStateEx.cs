using System;
using System.Threading.Tasks;

namespace Platformex.Application
{
    public interface IModel
    {
        Guid Id { get; set; }
    }
    public abstract class AggregateStateEx<TIdentity, TAggregateState, TModel> :AggregateState<TIdentity, TAggregateState> 
        where TIdentity : Identity<TIdentity>
        where TModel : IModel
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly IDbProvider<TModel> Provider;
        protected TModel Model;

        public static TAggregateState FromModel(TModel model)
        {
            var item = (TAggregateState)Activator.CreateInstance(typeof(TAggregateState), new object[]{null});

            (item as AggregateStateEx<TIdentity, TAggregateState, TModel>)?.SetModel(model);
            return item;
        }
        private void SetModel(TModel model)
        {
            Model = model;
        }

        protected AggregateStateEx(IDbProvider<TModel> provider)
        {
            Provider = provider;
        }
        protected override async Task<bool> LoadStateInternal(TIdentity id)
        {
            bool isCreated;
            (Model,isCreated) = await Provider.LoadOrCreate(id.GetGuid());
            Model.Id = Model.Id == Guid.Empty ? id.GetGuid() : Model.Id;
            return isCreated;
        }

        public override Task BeginTransaction() => Provider.BeginTransaction();
        public override Task CommitTransaction() => Provider.CommitTransaction();
        public override async Task RollbackTransaction()
        {
            await Provider.RollbackTransaction();
            await LoadStateInternal(Identity);
        }

        protected override async Task AfterApply(IAggregateEvent<TIdentity> @event)
        {
            await Provider.SaveChangesAsync(Model.Id, Model);
        }

    }
}