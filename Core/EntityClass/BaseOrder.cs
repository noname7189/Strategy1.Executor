using System.ComponentModel.DataAnnotations;
using Strategy1.Executor.Core.DataClass;

namespace Strategy1.Executor.Core.EntityClass
{
    public class BaseOrder : InmemoryOrder
    {
        [Key]
        public int Id { get; set; }
        public decimal Fee { get; set; }
        public decimal RealizedProfit { get; set; }
        public bool Finished { get; set; }
        public bool IsMaker { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
