using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.DataClass.Rest;
using Strategy1.Executor.Core.DataClass.Stream;
using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.StreamCore;
using Strategy1.Executor.Impl.Util;

StrategyManager manager = StrategyManager.Instance;

Action<AccountStreamRecv> onAccountUpdate = (data) =>
{
    if (data.Data.Reason == UpdateReason.FundingFee)
    {
        foreach (var item in data.Data.Balances)
        {
            if (item.Asset == "USDT")
            {
                Message.SendMessage($"{item.Asset} FundingFee: {item.BalanceChange}");
                using AppDbContext db = new();
                Strategy1.Executor.Impl.DB.Entity.TradeContext context = db
                    .TradeContext.Where(a => a.StrategyIdentifier == nameof(Strategy1))
                    .OrderByDescending(a => a.Id)
                    .Take(1)
                    .Single();
                context.CurrentMargin += item.BalanceChange;
                db.SaveChanges();
            }
        }
    }
};

Action<BaseStreamRecv> onListenKeyExpired = (data) =>
{
    Console.WriteLine($"{data.EventTime} - listenkey expired");
};

Action<AccountInfo> onGetAccountInfo = (data) =>
{
    Console.WriteLine($"MarBal: {data.TotalMarginBalance}, AvailBal: {data.AvailableBalance}");
};

IProvider provider = manager.AddProvider(
    new()
    {
        Exchange = Exchange.Binance,
        PublicKey = Constant.PublicKey,
        SecretKey = Constant.PrivateKey,
        OnGetAccountInfo = onGetAccountInfo,
        OnAccountUpdate = onAccountUpdate,
        OnListenKeyExpired = onListenKeyExpired,
    }
);

provider
    .AddStreamCore<ETH5M_StreamCore>()
    .AddStrategy<Strategy1.Executor.Impl.Strategies.Strategy1.Strategy1>();

manager.Run(keepRunning: true);
