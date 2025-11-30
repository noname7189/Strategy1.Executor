using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter.DependentConverter;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.DataClass.Rest
{
    public class PositionInfo
    {
        [JsonProperty("s"), JsonConverter(typeof(SymbolConverter))]
        public Symbol? Symbol { get; set; }

        [JsonProperty("pa")]
        public decimal Quantity { get; set; }

        [JsonProperty("ep")]
        public decimal EntryPrice { get; set; }

        [JsonProperty("cr")]
        public decimal RealizedPnl { get; set; }

        [JsonProperty("up")]
        public decimal UnrealizedPnl { get; set; }
    }
}
