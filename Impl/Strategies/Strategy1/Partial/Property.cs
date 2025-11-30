using Microsoft.Extensions.Logging;
using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.DB.Entity;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;

namespace Strategy1.Executor.Impl.Strategies.Strategy1
{
    public partial class Strategy1
        : BaseStrategy<AppDbContext, ETH5M_Candle, ETH5M_Indicator, ETH5M_Signal>,
            IOrderRepository<AppDbContext, ETH5M_Order>
    {
        // Public Property
        public readonly bool EnterForMissedSignals = false;
        private readonly List<int> LosscutReducers = [];
        public TradeContext CurrentTradeCtx { get; set; }

        public decimal Coeff1 { get; set; }
        public decimal Coeff2 { get; set; }
        public decimal Coeff3 { get; set; }
        public decimal Coeff4 { get; set; }

        protected readonly ILogger Log = LoggerFactory
            .Create(builder =>
            {
                builder.AddConsole();
                builder.AddFile();
            })
            .CreateLogger("[Strategy1]");
    }
}
