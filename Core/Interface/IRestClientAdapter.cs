using Strategy1.Executor.Core.DataClass.Rest;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.Interface
{
    public interface IRestClientAdapter
    {
        // StartUserStreamAsync
        Task<Response<ListenKeyData>> GetListenKey();

        // GetAccountInfoASync
        Task<Response<AccountInfo>> GetAccountInfoAsync();

        // GetKlinesAsync
        Task<Response<List<KlineData>>> GetKlinesAsync(
            Symbol symbol,
            Interval interval,
            int? limit
        );

        Task<Response<OrderResult>> PlaceOrderAsync(
            Symbol symbol,
            OrderSide position,
            decimal price,
            decimal quantity
        );

        Task<Response<OrderResult>> CancelOrderAsync(Symbol symbol, long orderId);
    }
}
