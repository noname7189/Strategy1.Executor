using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Core.Util;

namespace Strategy1.Executor.Core.Converter.DependentConverter
{
    public class StreamEventTypeConverter : BaseConverter<StreamEventType>
    {
        private static List<KeyValuePair<StreamEventType, string>> Values =>
            [
                new KeyValuePair<StreamEventType, string>(
                    StreamEventType.ListenkeyExpired,
                    "listenKeyExpired"
                ),
                new KeyValuePair<StreamEventType, string>(
                    StreamEventType.AccountUpdate,
                    "ACCOUNT_UPDATE"
                ),
                new KeyValuePair<StreamEventType, string>(
                    StreamEventType.OrderUpdate,
                    "ORDER_TRADE_UPDATE"
                ),
            ];

        public override List<KeyValuePair<StreamEventType, string>> Mapping => Values;

        public static StreamEventType? GetKey(string? key)
        {
            return Values.SingleOrNull(v => v.Value == key)?.Key;
        }
    }
}
