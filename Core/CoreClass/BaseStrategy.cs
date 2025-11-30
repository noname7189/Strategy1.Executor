using Microsoft.EntityFrameworkCore;
using Strategy1.Executor.Core.DataClass;
using Strategy1.Executor.Core.DataClass.Stream;
using Strategy1.Executor.Core.EntityClass;
using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Core.Interface;

namespace Strategy1.Executor.Core.CoreClass
{
    public abstract class BaseStrategy<X, C, I, S>
        : IStrategy,
            IIndicatorRepository<X, I>,
            ISignalRepository<X, S>,
            ISignalGenerator<S>
        where X : DbContext, new()
        where C : BaseCandle, new()
        where I : BaseIndicator, new()
        where S : BaseSignal, new()
    {
        protected List<C> Candles { get; set; }
        protected readonly List<I> Indicators = [];
        public readonly List<S> OnlineSignals = [];
        public readonly List<InmemoryOrder> OnlineOrders = [];
        protected Symbol Symbol { get; set; }

        public BaseStreamCore<X, C> StreamCore { get; private set; }
        public IRestClientAdapter RestClientAdapter { get; private set; }

        public void SetStreamCore(object core)
        {
            StreamCore =
                core as BaseStreamCore<X, C> ?? throw new ArgumentNullException(nameof(core));
        }

        public void SetRestClientAdapter(IRestClientAdapter adapter)
        {
            RestClientAdapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public void PreStrategyInitWrapper()
        {
            Candles = StreamCore.Candles;
            Symbol = StreamCore.Symbol;
            PreStrategyInit();
        }

        public void PostStrategyInitWrapper()
        {
            Indicators.RemoveRange(0, Indicators.Count - StrategyManager.Instance.BaseCandleCount);
            PostStrategyInit();
        }

        public void TryToMakeNewIndicatorWrapper()
        {
            TryToMakeNewIndicator();
            Indicators.RemoveAt(0);
        }

        public abstract DbSet<I> IndicatorRepo(X db);
        public abstract DbSet<S> SignalRepo(X db);
        public abstract void InitWithAdditionalCandles();
        public abstract void InitWithoutAdditionalCandles();
        public abstract void PreStrategyInit();
        public abstract void PostStrategyInit();
        public abstract void ProcessEnter(decimal enterPrice, BaseSignal target);
        public abstract void ProcessLosscut(DateTime exitTime, BaseSignal target);
        public abstract Action<OrderStreamRecv>? ProcessOnOrderUpdate();
        public abstract void ProcessTakeProfit(decimal exitPrice, DateTime exitTime);
        public abstract void ProcessWithDifferentCandle(
            KlineStreamRawData klines,
            BaseCandle prevCandle
        );
        public abstract void ProcessWithSameCandle(KlineStreamRawData klines);
        public abstract void TryToMakeNewIndicator();
        public abstract S? TryToMakeNewSignal();

        protected void FinalizeSignal(S signal, decimal exitPrice, DateTime exitTime)
        {
            signal.EndTime = exitTime;
            signal.ExitPrice = exitPrice;
            signal.ExpectedProfit = exitPrice - signal.EnterPrice;
        }

        public abstract List<S> GetOnlineSignals();
        public abstract List<S> GenerateSignalsDuringSystemOff(int startIndex);
        public abstract Task FinalizeFinishedSignals(List<S> finishedSignals);
        public abstract List<S> TryTerminateResidualSignals();
        public abstract S? AddInmemorySignal();
        public abstract void InitializeGeneratedSignals(List<S> generatedSignals);
    }
}
