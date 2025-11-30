using Strategy1.Executor.Core.DataClass.Stream;
using Strategy1.Executor.Core.EntityClass;

namespace Strategy1.Executor.Core.Interface
{
    public interface IInitProcess // PreStream
    {
        public void PreStrategyInit();
        public void PreStrategyInitWrapper();
        public void InitWithoutAdditionalCandles();
        public void InitWithAdditionalCandles();
        public void PostStrategyInit();
        public void PostStrategyInitWrapper();
    }

    public interface IOnlineProcess // IntraStream
    {
        public void TryToMakeNewIndicator();
        public void TryToMakeNewIndicatorWrapper();
        public void ProcessWithSameCandle(KlineStreamRawData klines);
        public void ProcessWithDifferentCandle(KlineStreamRawData klines, BaseCandle prevCandle);
    }

    public interface IOrderProcess
    {
        public void ProcessEnter(decimal enterPrice, BaseSignal target);
        public void ProcessTakeProfit(decimal exitPrice, DateTime exitTime);
        public void ProcessLosscut(DateTime exitTime, BaseSignal target);
        public Action<OrderStreamRecv>? ProcessOnOrderUpdate();
    }

    public interface IStrategy : IInitProcess, IOnlineProcess, IOrderProcess
    {
        public void SetStreamCore(object core);
        public void SetRestClientAdapter(IRestClientAdapter adapter);
    }
}
