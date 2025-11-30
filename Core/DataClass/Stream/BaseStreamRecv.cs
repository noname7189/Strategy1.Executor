using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter;
using Strategy1.Executor.Core.Converter.DependentConverter;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.DataClass.Stream
{
    public class BaseStreamRecv
    {
        [JsonProperty("e"), JsonConverter(typeof(StreamEventTypeConverter))]
        public StreamEventType? Event;

        [JsonProperty("E"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime EventTime;
    }
}
