
namespace Platformex.Web.Swagger
{
    public static class HttpRouting
    {
        public const string AggregateIdRouteParam = "aggregateId";
        public const string DomainRouteParam = "domainRouteParam";
        public const string AggregateRouteParam = "aggregateRouteParam";
        public const string ActionRouteParam = "actionRouteParam";
        public const string ActionFormat = "api/{DomainName}/{AggregateName}/{AggregateId}/{ActionName}";
        public const string SingletonActionFormat = "api/{DomainName}/{AggregateName}/{ActionName}";

        public class Params
        {
            public readonly string AggregateId = "{aggregateId}";

            public static Params ForController { get; } = new Params { DomainName = "{domainRouteParam}", AggregateName = "{aggregateRouteParam}", ActionName = "{actionRouteParam}" };

            public string DomainName { get; set; }

            public string AggregateName { get; set; }

            public string ActionName { get; set; }
        }
    }
}
