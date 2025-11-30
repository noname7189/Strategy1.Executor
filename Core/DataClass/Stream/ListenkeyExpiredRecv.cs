using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter;
using Strategy1.Executor.Core.Converter.DependentConverter;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.DataClass.Stream
{
    public class ListenkeyExpiredRecv
    {
        [JsonProperty("stream")]
        public string ListenKey;

        [JsonProperty("data")]
        public ExpiredData Data;
    }

    public struct ExpiredData
    {
        [JsonProperty("e")]
        [JsonConverter(typeof(StreamEventTypeConverter))]
        public StreamEventType? EventType;

        [JsonProperty("E")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EventTime;

        [JsonProperty("listenkey")]
        public string ListenKey;
    }
}
