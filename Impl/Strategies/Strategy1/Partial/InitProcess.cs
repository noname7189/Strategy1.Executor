using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;
using Strategy1.Executor.Impl.Util;

namespace Strategy1.Executor.Impl.Strategies.Strategy1
{
    public partial class Strategy1
        : BaseStrategy<AppDbContext, ETH5M_Candle, ETH5M_Indicator, ETH5M_Signal>,
            IOrderRepository<AppDbContext, ETH5M_Order>
    {
        public override void InitWithoutAdditionalCandles()
        {
            List<ETH5M_Signal> finishedSignals = TryTerminateResidualSignals();

            OnlineOrders.AddRange(OrderUtil.GetInmemoryOrders(this));
            OnlineSignals.AddRange(GetOnlineSignals());

            FinalizeFinishedSignals(finishedSignals).Wait();
        }

        public override void InitWithAdditionalCandles()
        {
            int startIndex = IndicatorUtil.AddIndicators(this, Candles, Indicators);

            List<ETH5M_Signal> finishedSignals = TryTerminateResidualSignals();

            List<ETH5M_Signal> generatedSignals = GenerateSignalsDuringSystemOff(startIndex);

            OnlineOrders.AddRange(OrderUtil.GetInmemoryOrders(this));
            OnlineSignals.AddRange(GetOnlineSignals());

            FinalizeFinishedSignals(finishedSignals).Wait();
            InitializeGeneratedSignals(generatedSignals);
        }

        public override void PostStrategyInit() { }
    }
}
