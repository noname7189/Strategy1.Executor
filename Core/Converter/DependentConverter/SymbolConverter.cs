using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.Converter.DependentConverter
{
    public class SymbolConverter : BaseConverter<Symbol>
    {
        private static readonly List<KeyValuePair<Symbol, string>> Values =
        [
            new KeyValuePair<Symbol, string>(Symbol.ETHUSDT, "ETHUSDT"),
            new KeyValuePair<Symbol, string>(Symbol.BTCUSDT, "BTCUSDT"),
        ];

        public override List<KeyValuePair<Symbol, string>> Mapping => Values;
    }
}
