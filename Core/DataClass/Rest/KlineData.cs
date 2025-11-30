using Newtonsoft.Json;
using Strategy1.Executor.Core.Converter;

namespace Strategy1.Executor.Core.DataClass.Rest
{
    [JsonConverter(typeof(KlineConverter))]
    public record KlineData
    {
        public required DateTime StartTime;
        public required decimal Open;
        public required decimal High;
        public required decimal Low;
        public required decimal Close;
        public required decimal Volume;
        public required int TradeCount;
    }
}
