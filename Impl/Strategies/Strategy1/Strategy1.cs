using Microsoft.EntityFrameworkCore;
using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;

namespace Strategy1.Executor.Impl.Strategies.Strategy1
{
    public partial class Strategy1
        : BaseStrategy<AppDbContext, ETH5M_Candle, ETH5M_Indicator, ETH5M_Signal>,
            IOrderRepository<AppDbContext, ETH5M_Order>
    {
        public override DbSet<ETH5M_Indicator> IndicatorRepo(AppDbContext db)
        {
            return db.ETH5M_Indicator;
        }

        public override DbSet<ETH5M_Signal> SignalRepo(AppDbContext db)
        {
            return db.ETH5M_Signal;
        }

        public DbSet<ETH5M_Order> OrderRepo(AppDbContext db)
        {
            return db.ETH5M_Order;
        }

        public override void PreStrategyInit()
        {
            throw new NotImplementedException();
        }
    }
}
