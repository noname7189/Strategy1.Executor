using Microsoft.EntityFrameworkCore;
using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;
using Strategy1.Executor.Impl.Util;

namespace Strategy1.Executor.Impl.StreamCore
{
    public class ETH5M_StreamCore : BaseStreamCore<AppDbContext, ETH5M_Candle>
    {
        public override Symbol Symbol => Symbol.ETHUSDT;
        public override Interval Interval => Interval.FiveMinutes;

        public override DbSet<ETH5M_Candle> CandleRepo(AppDbContext db)
        {
            return db.ETH5M_Candle;
        }

        public override void PreStreamInit()
        {
            using AppDbContext db = new();
            List<ETH5M_Candle> candles =
            [
                .. db
                    .ETH5M_Candle.OrderByDescending(a => a.Id)
                    .Take(Constant.BaseCandleCount)
                    .Include(candle => candle.Indicator)
                    .AsNoTracking()
                    .Reverse(),
            ];

            for (var i = 1; i < candles.Count; i++)
            {
                if (candles[i - 1].Time.AddMinutes(5) != candles[i].Time)
                {
                    throw new Exception(
                        $"{GetType()} - candle list has blank at {candles[i - 1].Time}!"
                    );
                }
            }
            Candles.AddRange(candles);
        }
    }
}
