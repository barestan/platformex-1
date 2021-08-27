using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Platformex.Domain
{
    public class EventMetadata : Metadata, IEventMetadata
    {

        public static IEventMetadata Empty { get; } = new EventMetadata();

        [JsonIgnore]
        public string CorrelationId
        {
            get => GetMetadataValue(MetadataKeys.CorrelationId);
            set => AddOrUpdateValue(MetadataKeys.CorrelationId, value);
        }

        [JsonIgnore]
        public IReadOnlyCollection<string> CorrelationIds
        {
            get => ContainsKey(MetadataKeys.CorrelationIds) ?
                GetMetadataValue(MetadataKeys.CorrelationIds)?.Split(',') :
                new List<string>().AsReadOnly();
            set => AddOrUpdateValue(MetadataKeys.CorrelationIds, value == null ? "" : string.Join(",", value));
        }


        [JsonIgnore]
        public string CausationId
        {
            get => GetMetadataValue(MetadataKeys.CausationId);
            set => AddOrUpdateValue(MetadataKeys.CausationId, value);
        }


        public EventMetadata()
        {
            // Empty
        }

        public EventMetadata(IDictionary<string, string> keyValuePairs)
            : base(keyValuePairs)
        {
        }

        public EventMetadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
            : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public EventMetadata(params KeyValuePair<string, string>[] keyValuePairs)
            : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs)
        {
        }

    }
}
