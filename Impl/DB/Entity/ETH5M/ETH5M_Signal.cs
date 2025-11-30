using System.ComponentModel.DataAnnotations.Schema;
using Strategy1.Executor.Core.EntityClass;

namespace Strategy1.Executor.Impl.DB.Entity.ETH5M
{
    [Table(nameof(ETH5M_Signal))]
    public class ETH5M_Signal : BaseSignal
    {
        public bool IsTriggered { get; set; }
        public new ETH5M_Candle Candle { get; set; }
    }
}
