using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter;
using Strategy1.Executor.Core.Converter.DependentConverter;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.DataClass.Stream
{
    public class OrderStreamRecv : BaseStreamRecv
    {
        [JsonProperty("T"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime TransactionTime;

        [JsonProperty("o")]
        public OrderStreamData Data;
    }

    public class OrderStreamData
    {
        [JsonProperty("n")]
        public decimal Fee;

        [JsonProperty("rp")]
        public decimal RealizedProfit;

        [JsonProperty("p")]
        public decimal Price;

        [JsonProperty("ap")]
        public decimal AveragePrice;

        [JsonProperty("z")]
        public decimal FulfilledQuantity;

        [JsonProperty("l")]
        public decimal QuantityFilled;

        [JsonProperty("q")]
        public decimal Quantity;

        [JsonProperty("T"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime UpdateTime;

        [JsonProperty("t")]
        public long TradeId;

        [JsonProperty("i")]
        public long OrderId;

        [JsonProperty("m")]
        public bool IsMaker;

        [JsonProperty("N")]
        public string FeeAsset { get; set; } = string.Empty;

        [JsonProperty("s"), JsonConverter(typeof(SymbolConverter))]
        public Symbol? Symbol;

        [JsonProperty("S"), JsonConverter(typeof(OrderPositionConverter))]
        public OrderSide? Side;

        [JsonProperty("o"), JsonConverter(typeof(OrderTypeConverter))]
        public OrderType? Type;

        [JsonProperty("X"), JsonConverter(typeof(OrderStatusConverter))]
        public OrderStatus? Status;
    }
}
