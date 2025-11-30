using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.DataClass;
using Strategy1.Executor.Core.DataClass.Rest;
using Strategy1.Executor.Core.Enum;
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
        public override List<ETH5M_Signal> GetOnlineSignals()
        {
            using AppDbContext db = new();
            List<ETH5M_Signal> res = [.. SignalRepo(db).Where(o => o.EndTime == null)];
            return res;
        }

        public override List<ETH5M_Signal> GenerateSignalsDuringSystemOff(int startIndex)
        {
            List<ETH5M_Signal> result = [];
            int size = Candles.Count;

            for (int i = startIndex; i < size; i++)
            {
                ETH5M_Indicator prev = Indicators[startIndex - 1];
                ETH5M_Indicator curr = Indicators[startIndex];
                var targetCandle = Candles[startIndex];

                ETH5M_Signal? signal = TryGenerateSignal(prev, curr, targetCandle);

                if (signal != null)
                {
                    TryCloseSignal(ref signal, i + 1, size);
                    result.Add(signal);
                }
            }
            return result;
        }

        public override void InitializeGeneratedSignals(List<ETH5M_Signal> generatedSignals)
        {
            if (generatedSignals.Count > 0)
            {
                using (AppDbContext db = new())
                {
                    SignalRepo(db).AddRange(generatedSignals);
                    db.SaveChanges();
                }

                foreach (var item in generatedSignals)
                {
                    if (item.EndTime == null)
                    {
                        OnlineSignals.Add(item);
                    }

                    if (EnterForMissedSignals && item.EnterPrice != null)
                    {
                        ProcessEnter((decimal)item.EnterPrice, item);
                    }
                }
            }
        }

        public override async Task FinalizeFinishedSignals(List<ETH5M_Signal> finishedSignals)
        {
            List<InmemoryOrder> adders = [];
            foreach (var signal in finishedSignals)
            {
                InmemoryOrder? target = OnlineOrders
                    .Where(a => a.SignalId == signal.Id)
                    .FirstOrDefault();

                if (target == null)
                {
                    Message.SendDebugMessage(
                        nameof(FinalizeFinishedSignals),
                        $"{signal.StartTime} 신호 미진입 종료"
                    );
                    continue;
                }

                if (target.Quantity > target.FulfilledQuantity)
                {
                    var cancelResult = await RestClientAdapter.CancelOrderAsync(
                        Symbol,
                        target.OrderId
                    );

                    if (cancelResult == null)
                    {
                        Message.SendDebugMessage(
                            nameof(FinalizeFinishedSignals),
                            $"신호 아이디 - {signal.Id}, OrderId - {target.OrderId} 취소 실패"
                        );
                    }

                    if (target.FulfilledQuantity == 0)
                    {
                        continue;
                    }
                }

                if (signal.ExitPrice == null)
                {
                    continue;
                }

                var response = await RestClientAdapter.PlaceOrderAsync(
                    Symbol,
                    OrderSide.Short,
                    (decimal)signal.ExitPrice!,
                    target.FulfilledQuantity
                );

                if (response.Data != null)
                {
                    OrderResult result = response.Data;

                    InmemoryOrder onlineOrder = new()
                    {
                        OrderId = result.OrderId,
                        CounterOrderId = target.OrderId,
                        TradeId = result.TradeId,
                        Quantity = result.Quantity,
                        Price = result.Price,
                        FulfilledQuantity = 0,
                        SignalId = signal.Id,
                        Signal = signal,
                        OrderStatus = OrderStatus.New,
                    };
                    adders.Add(onlineOrder);
                }
                else
                {
                    Message.SendDebugMessage(
                        nameof(FinalizeFinishedSignals),
                        $"OrderId: {target.OrderId} - 매수 청산 실패"
                    );
                }
            }

            foreach (var item in adders)
            {
                OnlineOrders.Add(item);
            }
        }

        public override List<ETH5M_Signal> TryTerminateResidualSignals()
        {
            using AppDbContext db = new();
            List<ETH5M_Signal> signals = [.. SignalRepo(db).Where(o => o.EndTime == null)];

            int size = Candles.Count;

            for (int i = 0; i < signals.Count; i++)
            {
                var signal = signals[i];
                int start = Candles.FindIndex(o => o.Id == signal.CandleId);
                if (start < 0)
                {
                    throw new BadDesignException(nameof(TryTerminateResidualSignals));
                }
                TryCloseSignal(ref signal, start + 1, size);
            }

            db.SaveChanges();

            return signals;
        }

        public override ETH5M_Signal? AddInmemorySignal()
        {
            ETH5M_Indicator prev = Indicators[^2];
            ETH5M_Indicator curr = Indicators[^1];
            var targetCandle = Candles[^1];

            return TryGenerateSignal(prev, curr, targetCandle);
        }

        private ETH5M_Signal? TryGenerateSignal(
            ETH5M_Indicator prev,
            ETH5M_Indicator curr,
            ETH5M_Candle targetCandle
        )
        {
            ETH5M_Signal? result = null;

            if (prev.EMA1 < prev.EMA2)
            {
                if (curr.EMA1 > curr.EMA2 && curr.EMA2 > curr.EMA3 && curr.EMA3 > curr.EMA4)
                {
                    var tp =
                        targetCandle.Close
                        + Coeff1 * targetCandle.Open
                        + Coeff2 * targetCandle.High
                        + Coeff3 * targetCandle.Low;

                    var lc = targetCandle.Close - Coeff4 * targetCandle.Low;

                    result = new()
                    {
                        StartTime = targetCandle.Time,
                        EndTime = null,
                        LosscutPrice = lc,
                        TakeProfitPrice = tp,
                        ExitPrice = null,
                        CandleId = targetCandle.Id,
                        SignalType = SignalType.Long,
                        EnterPrice = targetCandle.Close,
                        IsTriggered = false,
                    };
                }
            }

            return result;
        }

        private void TryCloseSignal(ref ETH5M_Signal signal, int startIndex, int endIndex)
        {
            decimal? exitPrice = null;

            int res = startIndex;
            decimal high = decimal.MinValue;
            decimal low = decimal.MaxValue;

            decimal takeProfitPrice = signal.TakeProfitPrice;
            decimal losscutPrice = signal.LosscutPrice;

            for (; res < endIndex; res++)
            {
                high = Candles[res].High > high ? Candles[res].High : high;
                low = Candles[res].Low < low ? Candles[res].Low : low;

                if (low < losscutPrice)
                {
                    exitPrice = losscutPrice;
                    break;
                }

                if (!signal.IsTriggered && high > takeProfitPrice)
                {
                    signal.IsTriggered = true;
                    continue;
                }

                if (signal.IsTriggered)
                {
                    if (Indicators[res].EMA1 < Indicators[res].EMA2)
                    {
                        exitPrice = Candles[res].Close;
                        break;
                    }
                }
            }

            if (exitPrice != null)
            {
                signal.EndTime = Candles[res].Time;
                signal.ExitPrice = Math.Round((decimal)exitPrice, 2);
                signal.ExpectedProfit = exitPrice - signal.EnterPrice;
            }
            else
            {
                signal.EndTime = null;
                signal.ExitPrice = null;
                signal.ExpectedProfit = null;
            }
        }
    }
}
