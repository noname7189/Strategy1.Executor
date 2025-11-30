using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Strategy1.Executor.Core.EntityClass;

namespace Strategy1.Executor.Impl.DB.Entity
{
    [Table(nameof(TradeContext))]
    public class TradeContext : BaseTradeContext
    {
        [MaxLength(30)]
        public string StrategyIdentifier { get; set; }
    }
}
