using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter;
using Strategy1.Executor.Core.Converter.DependentConverter;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.DataClass.Rest
{
    public class OrderResult
    {
        [JsonProperty("price")]
        public decimal Price;

        [JsonProperty("avgPrice")]
        public decimal AveragePrice;

        [JsonProperty("executedQty")]
        public decimal QuantityFilled;

        [JsonProperty("cumQty")]
        public decimal FulfilledQuantity;

        [JsonProperty("origQty")]
        public decimal Quantity;

        [JsonProperty("updateTime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime UpdateTime;

        [JsonProperty("time"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreateTime;

        [JsonProperty("orderId")]
        public long OrderId;

        [JsonProperty("tradeId")]
        public long? TradeId;

        [JsonProperty("symbol"), JsonConverter(typeof(SymbolConverter))]
        public Symbol? Symbol;

        [JsonProperty("side"), JsonConverter(typeof(OrderPositionConverter))]
        public OrderSide? Side;

        [JsonProperty("status"), JsonConverter(typeof(OrderStatusConverter))]
        public OrderStatus? Status;

        [JsonProperty("closePosition")]
        public bool Final;
    }
}
