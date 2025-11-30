using System.ComponentModel.DataAnnotations.Schema;
using Strategy1.Executor.Core.EntityClass;

namespace Strategy1.Executor.Impl.DB.Entity.ETH5M
{
    [Table(nameof(ETH5M_Indicator))]
    public class ETH5M_Indicator : BaseIndicator
    {
        public decimal? EMA1 { get; set; }
        public decimal? EMA2 { get; set; }
        public decimal? EMA3 { get; set; }
        public decimal? EMA4 { get; set; }
        public new ETH5M_Candle Candle { get; set; }
    }
}
