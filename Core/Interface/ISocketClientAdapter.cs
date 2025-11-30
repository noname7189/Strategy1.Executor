using Strategy1.Executor.Core.DataClass.Stream;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.Interface
{
    internal interface ISocketClientAdapter
    {
        Task SubscribeToUserDataUpdatesAsync();
        Task SubscribeToKlineUpdatesAsync(
            Symbol symbol,
            Interval interval,
            Action<KlineStreamRawData> onGetStreamData
        );
    }
}
