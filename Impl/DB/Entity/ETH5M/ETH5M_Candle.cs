using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Strategy1.Executor.Core.EntityClass;

namespace Strategy1.Executor.Impl.DB.Entity.ETH5M
{
    [Table(nameof(ETH5M_Candle))]
    public class ETH5M_Candle : BaseCandle
    {
        [JsonIgnore]
        public ETH5M_Indicator Indicator { get; set; }

        [JsonIgnore]
        public ETH5M_Signal? Signal { get; set; }
    }
}
