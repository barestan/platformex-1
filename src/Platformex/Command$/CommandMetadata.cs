using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Platformex
{
    public class CommandMetadata : Metadata, ICommandMetadata
    {
        public CommandMetadata()
        {
        }
        public CommandMetadata(ISourceId sourceId)
        {
            SourceId = sourceId ?? Platformex.SourceId.New;
        }

        public CommandMetadata(IDictionary<string, string> keyValuePairs)
            : base(keyValuePairs)
        {
        }

        public CommandMetadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
            : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public CommandMetadata(params KeyValuePair<string, string>[] keyValuePairs)
            : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs)
        {
        }


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
    }
}