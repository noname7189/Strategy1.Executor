using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.DataClass.Stream;
using Strategy1.Executor.Core.EntityClass;
using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Core.Util;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;
using Strategy1.Executor.Impl.Util;

namespace Strategy1.Executor.Impl.Strategies.Strategy1
{
    public partial class Strategy1
        : BaseStrategy<AppDbContext, ETH5M_Candle, ETH5M_Indicator, ETH5M_Signal>,
            IOrderRepository<AppDbContext, ETH5M_Order>
    {
        public override void TryToMakeNewIndicator()
        {
            _ = IndicatorUtil.AddIndicators(this, Candles, Indicators);
        }

        public override ETH5M_Signal? TryToMakeNewSignal()
        {
            ETH5M_Signal? signal = AddInmemorySignal();

            if (signal != null)
            {
                OnlineSignals.Add(signal);
                SaveOneSignal(signal);
            }

            return signal;
        }

        public override void ProcessWithDifferentCandle(
            KlineStreamRawData klines,
            BaseCandle prevCandle
        )
        {
            ProcessTakeProfit(prevCandle.Close, prevCandle.Time);

            _ = TryToMakeNewSignal();
        }

        public override void ProcessWithSameCandle(KlineStreamRawData klines)
        {
            int cnt = OnlineSignals.Count;
            if (cnt > 0)
            {
                for (int i = 0; i < cnt; i++)
                {
                    ETH5M_Signal signal = OnlineSignals[i];
                    if (klines.Low <= signal.LosscutPrice)
                    {
                        ProcessLosscut(Utils.GetDateTimeFromMilliSeconds(klines.EndTime), signal);
                        LosscutReducers.Add(i);
                    }

                    if (!signal.IsTriggered && klines.High >= signal.TakeProfitPrice)
                    {
                        signal.IsTriggered = true;
                    }
                }

                for (int i = LosscutReducers.Count - 1; i >= 0; i--)
                {
                    OnlineSignals.RemoveAt(LosscutReducers[i]);
                }
                LosscutReducers.Clear();
            }
        }
    }
}
