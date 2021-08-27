using System;
using TechTalk.SpecFlow;

namespace Siam.Spec.Transformations
{
    [Binding]
    public class AggregatesTransformations
    {
        [StepArgumentTransformation]
        public DateTime ToAggregate(string name)
        {
            return DateTime.Now;
        }
    }
}