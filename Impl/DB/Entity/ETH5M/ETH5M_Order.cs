using System.ComponentModel.DataAnnotations.Schema;
using Strategy1.Executor.Core.EntityClass;

namespace Strategy1.Executor.Impl.DB.Entity.ETH5M
{
    [Table(nameof(ETH5M_Order))]
    public class ETH5M_Order : BaseOrder
    {
        public new ETH5M_Signal Signal { get; set; }
    }
}
