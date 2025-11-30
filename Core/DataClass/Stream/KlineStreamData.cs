using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter;

namespace Strategy1.Executor.Core.DataClass.Stream
{
    public class KlineStreamData : KlineStreamRawData
    {
        [JsonProperty("t")]
        [JsonConverter(typeof(DateTimeConverter))]
        public new DateTime StartTime;

        [JsonProperty("T")]
        [JsonConverter(typeof(DateTimeConverter))]
        public new DateTime EndTime;
    }

    public class KlineStreamRawData
    {
        [JsonProperty("o")]
        public decimal Open;

        [JsonProperty("h")]
        public decimal High;

        [JsonProperty("l")]
        public decimal Low;

        [JsonProperty("c")]
        public decimal Close;

        [JsonProperty("v")]
        public decimal Volume;

        [JsonProperty("t")]
        public long StartTime;

        [JsonProperty("T")]
        public long EndTime;

        [JsonProperty("n")]
        public int TradeCount;

        [JsonProperty("x")]
        public bool Final;
    }
}
