using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Strategy1.Executor.Core.EntityClass;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;

namespace Strategy1.Executor.Impl.Util
{
    internal class IndicatorUtil
    {
        public static int AddIndicators<C, I>(
            IIndicatorRepository<AppDbContext, I> indicRepo,
            List<C> candles,
            List<I> indicators
        )
            where C : BaseCandle
            where I : ETH5M_Indicator, new()
        {
            int size = candles.Count;
            int start = indicators.Count;

            List<I> adder = [];
            for (int i = start; i < size; i++)
            {
                // Independent Property
                I indic = new()
                {
                    Time = candles[i].Time,
                    CandleId = candles[i].Id,
                    EMA1 = GetEMA(5, i, candles, indicators[i - 1].EMA1!.Value),
                    EMA2 = GetEMA(20, i, candles, indicators[i - 1].EMA2!.Value),
                    EMA3 = GetEMA(60, i, candles, indicators[i - 1].EMA3!.Value),
                    EMA4 = GetEMA(120, i, candles, indicators[i - 1].EMA4!.Value),
                };

                indicators.Add(indic);
                adder.Add(indic);
            }

            using AppDbContext db = new();
            indicRepo.IndicatorRepo(db).AddRange(adder);
            db.SaveChanges();

            return start;
        }

        public static decimal GetEMA<C>(int period, int index, List<C> ohlcv, decimal prev)
            where C : BaseCandle
        {
            var k = 2 / (decimal)(period + 1);

            return k * ohlcv[index].Close + (1 - k) * prev;
        }
    }
}
