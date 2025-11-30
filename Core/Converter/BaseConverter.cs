using System.Diagnostics;
using Newtonsoft.Json;

namespace Strategy1.Executor.Core.Converter
{
    public abstract class BaseConverter<T> : JsonConverter
        where T : struct
    {
        public abstract List<KeyValuePair<T, string>> Mapping { get; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        private bool GetValue(string value, out T result)
        {
            var value2 = value;
            KeyValuePair<T, string> keyValuePair = Mapping.FirstOrDefault(kv =>
                kv.Value.Equals(value2, StringComparison.InvariantCulture)
            );
            if (keyValuePair.Equals(default(KeyValuePair<T, string>)))
            {
                keyValuePair = Mapping.FirstOrDefault(kv =>
                    kv.Value.Equals(value2, StringComparison.InvariantCultureIgnoreCase)
                );
            }

            if (!keyValuePair.Equals(default(KeyValuePair<T, string>)))
            {
                result = keyValuePair.Key;
                return true;
            }

            result = default;
            return false;
        }

        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
        )
        {
            if (reader.Value == null)
            {
                return null;
            }

            var value = reader.Value.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (!GetValue(value, out var result))
            {
                Trace.WriteLine(
                    string.Format(
                        "{0:yyyy/MM/dd HH:mm:ss:fff} | Warning | Cannot map enum value. EnumType: {1}, Value: {2}, Known values: {3}. If you think {4} should added please open an issue on the Github repo",
                        DateTime.Now,
                        typeof(T),
                        reader.Value,
                        string.Join(
                            ", ",
                            Mapping.Select<KeyValuePair<T, string>, string>(m => m.Value)
                        ),
                        reader.Value
                    )
                );
                return null;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
