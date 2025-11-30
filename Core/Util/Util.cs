using CryptoExchange.Net.Objects;
using Strategy1.Executor.Core.Converter;

namespace Strategy1.Executor.Core.Util
{
    public static class Utils
    {
        public static DateTime GetDateTimeFromMilliSeconds(long milliseconds)
        {
            return DateTimeConverter.ConvertFromMilliseconds(milliseconds);
        }

        public static async Task HandleRequest<T>(
            Func<Task<WebCallResult<T>>> request,
            Action<T> outputData
        )
        {
            WebCallResult<T> result = await request();
            if (result.Success)
            {
                {
                    outputData(result.Data);
                }
            }
        }
    }
}
