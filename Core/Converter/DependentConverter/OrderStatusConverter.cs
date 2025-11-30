using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.Converter.DependentConverter
{
    public class OrderStatusConverter : BaseConverter<OrderStatus>
    {
        private static List<KeyValuePair<OrderStatus, string>> Values =>
            [
                new KeyValuePair<OrderStatus, string>(OrderStatus.New, "NEW"),
                new KeyValuePair<OrderStatus, string>(
                    OrderStatus.PartiallyFilled,
                    "PARTIALLY_FILLED"
                ),
                new KeyValuePair<OrderStatus, string>(OrderStatus.Filled, "FILLED"),
                new KeyValuePair<OrderStatus, string>(OrderStatus.Canceled, "CANCELED"),
            ];

        public override List<KeyValuePair<OrderStatus, string>> Mapping => Values;
    }
}
