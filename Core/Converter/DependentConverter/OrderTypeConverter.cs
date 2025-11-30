using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Core.Util;

namespace Strategy1.Executor.Core.Converter.DependentConverter
{
    public class OrderTypeConverter : BaseConverter<OrderType>
    {
        private static readonly List<KeyValuePair<OrderType, string>> Values =
        [
            new KeyValuePair<OrderType, string>(OrderType.Limit, "LIMIT"),
            new KeyValuePair<OrderType, string>(OrderType.Market, "MARKET"),
            new KeyValuePair<OrderType, string>(OrderType.TakeProfit, "TAKE_PROFIT"),
            new KeyValuePair<OrderType, string>(OrderType.TakeProfitMarket, "TAKE_PROFIT_MARKET"),
            new KeyValuePair<OrderType, string>(OrderType.Stop, "STOP"),
            new KeyValuePair<OrderType, string>(OrderType.StopMarket, "STOP_MARKET"),
            new KeyValuePair<OrderType, string>(
                OrderType.TrailingStopMarket,
                "TRAILING_STOP_MARKET"
            ),
            new KeyValuePair<OrderType, string>(OrderType.Liquidation, "LIQUIDATION"),
        ];

        public override List<KeyValuePair<OrderType, string>> Mapping => Values;

        public static string? GetValue(OrderType value)
        {
            return Values.SingleOrNull(v => v.Key == value)?.Value;
        }
    }
}
