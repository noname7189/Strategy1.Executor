using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Strategy1.Executor.Impl.Util
{
    public static class Message
    {
        public static readonly string DebugToken = "Sample1";
        public static readonly string OrderToken = "Sample2";

        public static readonly string ChatId = "Sameple3";
        public static readonly string Token = "Sample4";

        public static DateTime GetDateTimeFromMilliSeconds(long milliseconds)
        {
            return Core.Util.Utils.GetDateTimeFromMilliSeconds(milliseconds);
        }

        public static void SendDebugMessage(string currentStage, string message)
        {
            Task.Run(() =>
            {
                HttpClient client = new()
                {
                    BaseAddress = new Uri($"https://api.telegram.org/bot{DebugToken}/sendMessage"),
                };

                string data = JsonSerializer.Serialize(
                    new { chat_id = ChatId, text = $"[{currentStage}] {message}" }
                );
                using StringContent jsonContent = new(data, Encoding.UTF8, "application/json");

                client.PostAsync("", jsonContent).Wait();
            });
        }

        public static void SendMessage(string message)
        {
            Task.Run(() =>
            {
                HttpClient client = new()
                {
                    BaseAddress = new Uri($"https://api.telegram.org/bot{Token}/sendMessage"),
                };

                string data = JsonSerializer.Serialize(new { chat_id = ChatId, text = message });
                using StringContent jsonContent = new(data, Encoding.UTF8, "application/json");

                client.PostAsync("", jsonContent).Wait();
            });
        }

        public static void SendOrderMessage(string message)
        {
            Task.Run(() =>
            {
                HttpClient client = new()
                {
                    BaseAddress = new Uri($"https://api.telegram.org/bot{OrderToken}/sendMessage"),
                };

                string data = JsonSerializer.Serialize(new { chat_id = ChatId, text = message });
                using StringContent jsonContent = new(data, Encoding.UTF8, "application/json");

                client.PostAsync("", jsonContent).Wait();
            });
        }

        public static void Info(this ILogger log, string? str, params object?[] args)
        {
            Task.Run(() => log.LogInformation(str, args));
        }

        public static void InfoAndSend(this ILogger log, string? str, params object?[] args)
        {
            log.Info(str, args);
            SendMessage(string.Format(str, args));
        }

        public static void InfoAndSendOrder(this ILogger log, string? str, params object?[] args)
        {
            log.Info(str, args);
            SendOrderMessage(string.Format(str, args));
        }
    }
}
