using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Strategy1.Executor.Core.Converter;
using Strategy1.Executor.Core.Converter.DependentConverter;
using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.DataClass.Rest;
using Strategy1.Executor.Core.DataClass.Stream;
using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Core.Util;
using WebSocketSharp;

namespace Strategy1.Executor.Core.Provider
{
    internal class BinanceProvider : BaseProvider
    {
        private const string AccountEndpoint = @"/fapi/v2/account";
        private const string KlinesEndpoint = @"/fapi/v1/klines";
        private const string ListenkeyEndpoint = @"/fapi/v1/listenKey";
        private const string OrderEndpoint = @"/fapi/v1/order";

        private const string StreamBase = @"wss://fstream.binance.com/ws";

        private static readonly HttpClient Client = new();
        private static readonly HMACSHA256 Encryptor = new();
        private static long _timeOffset = 0;

        private WebSocket UserDataSocket;
        private readonly List<WebSocket> KlineDataSocketList = [];

        internal override void InitContext()
        {
            Client.BaseAddress = new Uri("https://fapi.binance.com");
            Client.DefaultRequestHeaders.Add("X-MBX-APIKEY", PublicKey);
            Encryptor.Key = Encoding.UTF8.GetBytes(SecretKey);

            _timeOffset = GetOffset().Result;
        }

        internal override void CreateContext(ProviderType type) { }

        public override async Task<Response<AccountInfo>> GetAccountInfoAsync()
        {
            HttpResponseMessage response = await Client.GetAsync(
                GetEntireRouteForSigned(AccountEndpoint)
            );

            return await ResponseWrapper<AccountInfo>(response);
        }

        public override async Task<Response<List<KlineData>>> GetKlinesAsync(
            Symbol symbol,
            Interval interval,
            int? limit
        )
        {
            var parameters = new Dictionary<string, string>()
            {
                { "symbol", symbol.ToString() },
                {
                    "interval",
                    JsonConvert.SerializeObject(interval, new IntervalConverter()).Replace("\"", "")
                },
            };

            parameters.AddOptionalParameter("limit", limit?.ToString());

            string query = GetEntireQuery(parameters);
            string fullQuery = GetEntireRoute(KlinesEndpoint, query);

            HttpResponseMessage response = await Client.GetAsync(fullQuery);

            return await ResponseWrapper<List<KlineData>>(response);
        }

        internal static async Task<bool> KeepAlive()
        {
            HttpResponseMessage response = await Client.PutAsync(
                GetEntireRouteForSigned(ListenkeyEndpoint),
                null
            );

            JToken token = JToken.Parse(await response.Content.ReadAsStringAsync());

            var code = token["code"];

            // TODO : change code == null
            if (response.IsSuccessStatusCode == true && code == null)
            {
                return true;
            }

            return false;
        }

        public override async Task<Response<ListenKeyData>> GetListenKey()
        {
            HttpResponseMessage response = await Client.PostAsync(
                GetEntireRouteForSigned(ListenkeyEndpoint),
                null
            );

            return await ResponseWrapper<ListenKeyData>(response);
        }

        public override async Task<Response<OrderResult>> PlaceOrderAsync(
            Symbol symbol,
            OrderSide position,
            decimal price,
            decimal quantity
        )
        {
            var parameters = new Dictionary<string, string>()
            {
                { "symbol", symbol.ToString() },
                { "quantity", quantity.DecimalToString() },
                { "price", price.DecimalToString() },
            };

            parameters.AddOptionalParameter("side", OrderPositionConverter.GetValue(position));
            parameters.AddOptionalParameter("type", OrderTypeConverter.GetValue(OrderType.Limit));
            parameters.AddOptionalParameter(
                "timeInForce",
                GoodTillDateConverter.GetValue(GoodTillDate.GoodTillCanceled)
            );

            string query = GetEntireQuery(parameters);
            string finalQuery = GetEntireRouteForSigned(OrderEndpoint, query);

            HttpResponseMessage response = await Client.PostAsync(finalQuery, null);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(query);
                Console.WriteLine(finalQuery);
                Console.WriteLine(response);
                Console.WriteLine(response.Content.ToString());
            }
            return await ResponseWrapper<OrderResult>(response);
        }

        public override async Task<Response<OrderResult>> CancelOrderAsync(
            Symbol symbol,
            long orderId
        )
        {
            var parameters = new Dictionary<string, string>()
            {
                { "symbol", symbol.ToString() },
                { "orderId", orderId.ToString() },
            };

            string query = GetEntireQuery(parameters);

            HttpResponseMessage response = await Client.DeleteAsync(
                GetEntireRouteForSigned(OrderEndpoint, query)
            );
            return await ResponseWrapper<OrderResult>(response);
        }

        public override Task SubscribeToKlineUpdatesAsync(
            Symbol symbol,
            Interval interval,
            Action<KlineStreamRawData> onGetStreamData
        )
        {
            string intervalStr =
                IntervalConverter.GetValue(interval)
                ?? throw new Exception("KlineSocket interval exception");

            // All symbols for streams are lowercase
            string url = $"{StreamBase}/{symbol.ToString().ToLower()}@kline_{intervalStr}";

            WebSocket sock = new(url);

            sock.OnMessage += (sender, e) =>
            {
                var combinedToken = JToken.Parse(e.Data);

                var token = combinedToken["k"];

                //if (token == null) return;

                try
                {
                    KlineStreamRawData raw = new()
                    {
                        StartTime = (long)token["t"],
                        EndTime = (long)token["T"],
                        Open = (decimal)token["o"],
                        High = (decimal)token["h"],
                        Low = (decimal)token["l"],
                        Close = (decimal)token["c"],
                        Volume = (decimal)token["v"],
                        Final = (bool)token["x"],
                    };

                    onGetStreamData(raw);
                }
                catch { }
            };
            sock.SslConfiguration.EnabledSslProtocols = System
                .Security
                .Authentication
                .SslProtocols
                .Tls12;

            sock.Connect();

            bool ping = sock.Ping();
            bool isAlive = sock.IsAlive;
            bool isSecure = sock.IsSecure;

            if (ping && isAlive && isSecure)
            {
                KlineDataSocketList.Add(sock);
                return Task.CompletedTask;
            }
            throw new Exception("KlineSocket Finish Error");
        }

        public override Task SubscribeToUserDataUpdatesAsync()
        {
            UserDataSocket = new WebSocket($"{StreamBase}/{ListenKey}");

            if (OnListenKeyExpired != null)
                UserDataSocket.OnMessage += (sender, e) =>
                {
                    var combinedToken = JToken.Parse(e.Data);

                    if (combinedToken["data"] != null)
                    {
                        if (OnListenKeyExpired != null)
                        {
                            ExpiredData data = combinedToken["data"]!.ToObject<ExpiredData>();
                            BaseStreamRecv recv = new()
                            {
                                Event = data.EventType,
                                EventTime = data.EventTime,
                            };

                            OnListenKeyExpired.Invoke(recv);
                        }
                        return;
                    }

                    var evnt = combinedToken["e"]?.ToString();
                    if (evnt == null)
                        return;
                    StreamEventType? evntType = StreamEventTypeConverter.GetKey(evnt);

                    switch (evntType)
                    {
                        case StreamEventType.AccountUpdate:
                        {
                            AccountStreamRecv? recv = combinedToken.ToObject<AccountStreamRecv>();
                            if (recv != null)
                                OnAccountUpdate?.Invoke(recv);
                            return;
                        }
                        case StreamEventType.OrderUpdate:
                        {
                            OrderStreamRecv? recv = combinedToken.ToObject<OrderStreamRecv>();
                            if (recv != null)
                                OnOrderUpdate(recv);
                            return;
                        }
                        case null:
                            return;
                    }
                };

            UserDataSocket.SslConfiguration.EnabledSslProtocols = System
                .Security
                .Authentication
                .SslProtocols
                .Tls12;

            UserDataSocket.Connect();

            bool ping = UserDataSocket.Ping();
            bool isAlive = UserDataSocket.IsAlive;
            bool isSecure = UserDataSocket.IsSecure;

            Console.WriteLine($"{ping}, {isAlive}, {isSecure}");

            return ping && isAlive && isSecure
                ? Task.CompletedTask
                : throw new Exception("UserDataSocket Finish Error");
        }

        private static string GetEntireQuery(Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(a => $"{a.Key}={a.Value}"));
        }

        private static async Task<Response<T>> ResponseWrapper<T>(HttpResponseMessage response)
            where T : class
        {
            if (response.IsSuccessStatusCode)
            {
                string target = await response.Content.ReadAsStringAsync();
                return new()
                {
                    StatusCode = response.StatusCode,
                    Msg = null,
                    Data = JsonConvert.DeserializeObject<T>(target),
                };
            }
            else
            {
                return new()
                {
                    StatusCode = response.StatusCode,
                    Msg = JsonConvert
                        .DeserializeObject<ErrorWrapper>(
                            await response.Content.ReadAsStringAsync()
                        )!
                        .msg,
                    Data = null,
                };
            }
        }

        private static async Task<long> GetOffset()
        {
            long timeResult = long.Parse(
                (await (await Client.GetAsync("/fapi/v1/time")).Content.ReadAsStringAsync()).Split(
                    ":"
                )[1][0..^1]
            );

            long timestamp = (long)DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow)!;

            return timeResult - timestamp;
        }

        private static string GetEntireRoute(string endpoint, string? query = null)
        {
            if (query == null)
                return $"{endpoint}";
            else
                return $"{endpoint}?{query}";
        }

        private static string GetEntireRouteForSigned(string endpoint, string? query = null)
        {
            string body;
            string res;

            if (query == null || query == string.Empty)
            {
                body = $"timestamp={GetAdjustedCurrentTimestamp()}";
            }
            else
            {
                body = $"{query}&timestamp={GetAdjustedCurrentTimestamp()}";
            }

            res =
                $"{body}&signature={ByteToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(body)))}";

            return $"{endpoint}?{res}";
        }

        private static long GetAdjustedCurrentTimestamp()
        {
            return (long)DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow)! + _timeOffset;
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); /* hex format */
            }

            return sbinary;
        }
    }
}
