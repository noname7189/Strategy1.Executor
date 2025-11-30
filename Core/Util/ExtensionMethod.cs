using System.Globalization;

namespace Strategy1.Executor.Core.Util
{
    public struct RequireClass<T>
        where T : class { }

    public struct RequireStruct<T>
        where T : struct { }

    internal static class ExtensionMethod
    {
        public static void AddOptionalParameter<T>(
            this Dictionary<string, T> parameters,
            string key,
            T? value
        )
        {
            if (value != null)
            {
                parameters.Add(key, value);
            }
        }

        public static string DecimalToString(this decimal dec)
        {
            var strdec = dec.ToString(CultureInfo.InvariantCulture);
            return strdec.Contains('.') ? strdec.TrimEnd('0').TrimEnd('.') : strdec;
        }

        public static TSource? SingleOrNull<TSource>(
            this IEnumerable<TSource> source,
            RequireClass<TSource> _ = default
        )
            where TSource : class
        {
            return Enumerable.SingleOrDefault(source.Select(x => (TSource?)x));
        }

        public static TSource? SingleOrNull<TSource>(
            this IEnumerable<TSource> source,
            RequireStruct<TSource> _ = default
        )
            where TSource : struct
        {
            return Enumerable.SingleOrDefault(source.Select(x => (TSource?)x));
        }

        public static TSource? SingleOrNull<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate,
            RequireClass<TSource> _ = default
        )
            where TSource : class
        {
            return Enumerable.SingleOrDefault(source.Where(predicate));
        }

        public static TSource? SingleOrNull<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate,
            RequireStruct<TSource> _ = default
        )
            where TSource : struct
        {
            return Enumerable.SingleOrDefault(source.Where(predicate));
        }
    }
}
