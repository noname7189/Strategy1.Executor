using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter;
using Strategy1.Executor.Core.Converter.DependentConverter;
using Strategy1.Executor.Core.DataClass.Rest;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.DataClass.Stream
{
    public class AccountStreamRecv : BaseStreamRecv
    {
        [JsonProperty("T")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime TransactionTime;

        [JsonProperty("a")]
        public AccountStreamData Data;
    }

    public class AccountStreamData
    {
        [JsonProperty("m")]
        [JsonConverter(typeof(UpdateReasonConverter))]
        public UpdateReason? Reason;

        [JsonProperty("B")]
        public List<BalanceInfo> Balances;

        [JsonProperty("P")]
        public List<PositionInfo> Positions;
    }
}
