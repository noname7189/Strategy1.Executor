using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter;

namespace Strategy1.Executor.Core.DataClass.Rest
{
    public record AccountInfo
    {
        public decimal TotalInitialMargin;
        public decimal TotalMarginBalance;
        public decimal TotalUnrealizedProfit;
        public decimal TotalWalletBalance;
        public decimal TotalCrossWalletBalance;
        public decimal AvailableBalance;

        [JsonProperty("updateTime"), JsonConverter(typeof(DateTimeConverter))]
        public DateTime? UpdateTime;
    }
}
