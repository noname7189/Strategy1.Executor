using Newtonsoft.Json;

namespace Strategy1.Executor.Core.DataClass.Rest
{
    public class BalanceInfo
    {
        [JsonProperty("a")]
        public string Asset;

        [JsonProperty("wb")]
        public decimal WalletBalance;

        [JsonProperty("cw")]
        public decimal CrossWalletBalance;

        [JsonProperty("bc")]
        public decimal BalanceChange;
    }
}
