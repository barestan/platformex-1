using System;
using System.Collections.Generic;
using Platformex.Application;
using Siam.MemoContext;
using Siam.MemoContext.Domain;

namespace Siam.Application
{
    public class MemoModel : IModel 
    {
        public Guid Id { get; set; }
        public MemoDocument Document { get; set; }
        public MemoStatus Status { get; set; }
        public ICollection<MemoStatusHistory> History { get; } = new List<MemoStatusHistory>();
    }
    
    public class MemoState :  AggregateStateEx<MemoId, MemoState, MemoModel>, IMemoState,
    ICanApply<MemoUpdated, MemoId>,
    ICanApply<SigningStarted, MemoId>,
    ICanApply<MemoSigned, MemoId>,
    ICanApply<RejectionStarted, MemoId>,
    ICanApply<MemoRejected, MemoId>
    {
        public MemoState(IDbProvider<MemoModel> provider) : base(provider) { }
        public MemoDocument Document => Model.Document;
        public MemoStatus Status => Model.Status;
        public IEnumerable<MemoStatusHistory> History => Model.History;

        public void Apply(MemoUpdated @event) => Model.Document = @event.Document;

        public void Apply(SigningStarted @event)
        {
            Model.Status = MemoStatus.SigningStarted;
            Model.History.Add(new MemoStatusHistory(DateTime.Now, @event.UserId, Model.Status));
        }

        public void Apply(MemoSigned @event)
        {
            Model.Status = MemoStatus.Signed;
            Model.History.Add(new MemoStatusHistory(DateTime.Now, string.Empty, Model.Status));
        }

        public void Apply(RejectionStarted @event)
        {
            Model.Status = MemoStatus.RejectionStarted;
            Model.History.Add(new MemoStatusHistory(DateTime.Now, @event.UserId, Model.Status));
        }

        public void Apply(MemoRejected @event)
        {
            Model.Status = MemoStatus.Rejected;
            Model.History.Add(new MemoStatusHistory(DateTime.Now, string.Empty, Model.Status));
        }
    }
}