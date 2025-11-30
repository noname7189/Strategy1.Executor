using System.Text;
using Microsoft.EntityFrameworkCore;
using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.DataClass;
using Strategy1.Executor.Core.DataClass.Rest;
using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.DB.Entity;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;
using Strategy1.Executor.Impl.Util;

namespace Strategy1.Executor.Impl.Strategies.Strategy1
{
    public partial class Strategy1
        : BaseStrategy<AppDbContext, ETH5M_Candle, ETH5M_Indicator, ETH5M_Signal>,
            IOrderRepository<AppDbContext, ETH5M_Order>
    {
        private void LosscutFullFulfillment(ETH5M_Signal target, InmemoryOrder targetOrder)
        {
            Task.Run(async () =>
            {
                OrderResult? result = (
                    await RestClientAdapter.PlaceOrderAsync(
                        Symbol,
                        OrderSide.Short,
                        target.LosscutPrice,
                        targetOrder.FulfilledQuantity
                    )
                ).Data;

                string res;
                if (result != null)
                {
                    InmemoryOrder order = new()
                    {
                        OrderId = result.OrderId,
                        CounterOrderId = targetOrder.OrderId,
                        TradeId = result.TradeId,
                        Quantity = result.Quantity,
                        Price = result.Price,
                        FulfilledQuantity = 0,
                        SignalId = target.Id,
                        Signal = target,
                        OrderStatus = OrderStatus.New,
                    };

                    OnlineOrders.Add(order);

                    res =
                        $"주문ID:{result.OrderId}, 신호ID: {targetOrder.SignalId}, 손절 예정가: {target.LosscutPrice}, 손절 수량: {targetOrder.FulfilledQuantity}";
                }
                else
                {
                    res =
                        $"신호ID:{target.Id}, 주문ID:{targetOrder.OrderId} - 손절 주문 중 result null 발생";
                }

                Log.InfoAndSendOrder(res);
            });
        }

        private void LosscutPartialFulfillment(ETH5M_Signal target, InmemoryOrder targetOrder)
        {
            Task.Run(async () =>
            {
                var cancelResult = await RestClientAdapter.CancelOrderAsync(
                    Symbol,
                    targetOrder.OrderId
                );
                if (cancelResult.Data == null)
                {
                    Message.SendDebugMessage(
                        nameof(LosscutPartialFulfillment),
                        $"신호 아이디 - {target.Id}, OrderId - {targetOrder.OrderId} 취소 실패"
                    );
                }

                OrderResult? result = (
                    await RestClientAdapter.PlaceOrderAsync(
                        Symbol,
                        OrderSide.Short,
                        target.LosscutPrice,
                        targetOrder.FulfilledQuantity
                    )
                ).Data;
                string res;
                if (result != null)
                {
                    InmemoryOrder order = new()
                    {
                        OrderId = result.OrderId,
                        CounterOrderId = targetOrder.OrderId,
                        TradeId = result.TradeId,
                        Quantity = result.Quantity,
                        Price = result.Price,
                        FulfilledQuantity = 0,
                        SignalId = target.Id,
                        Signal = target,
                        OrderStatus = OrderStatus.New,
                    };

                    OnlineOrders.Add(order);

                    res =
                        $"주문ID:{result.OrderId}, 신호ID: {targetOrder.SignalId}, 손절 예정가: {target.LosscutPrice}, 손절 수량: {targetOrder.FulfilledQuantity}";
                }
                else
                {
                    res =
                        $"신호ID:{target.Id}, 주문ID:{targetOrder.OrderId} - 손절 주문 중 result null 발생";
                }

                Log.InfoAndSendOrder(res);
            });
        }

        private void LosscutNoFulfillment(ETH5M_Signal target, InmemoryOrder targetOrder)
        {
            Task.Run(async () =>
            {
                var cancelResult = await RestClientAdapter.CancelOrderAsync(
                    Symbol,
                    targetOrder.OrderId
                );
                if (cancelResult == null)
                {
                    Message.SendDebugMessage(
                        nameof(LosscutPartialFulfillment),
                        $"신호 아이디 - {target.Id}, OrderId - {targetOrder.OrderId} 취소 실패"
                    );
                }
            });
        }

        private void SaveOneSignal(ETH5M_Signal signal)
        {
            Task.Run(() =>
            {
                using AppDbContext db = new();
                SignalRepo(db).Add(signal);
                db.SaveChanges();
            });
        }

        private static void UpdateEntity<TEntity>(TEntity target)
            where TEntity : class
        {
            Task.Run(() =>
            {
                using AppDbContext db = new();
                db.Entry(target).State = EntityState.Modified;
                db.SaveChanges();
            });
        }

        private void MakeTakeProfitOrder(Symbol symbol, decimal exitPrice, int signalId)
        {
            Task.Run(async () =>
            {
                InmemoryOrder? targetOrder = OnlineOrders.Find(a =>
                    a.SignalId == signalId && a.CounterOrderId == null
                );
                if (targetOrder != null)
                {
                    var response = await RestClientAdapter.PlaceOrderAsync(
                        symbol,
                        OrderSide.Short,
                        exitPrice,
                        targetOrder.FulfilledQuantity
                    );

                    if (response.Data != null)
                    {
                        var result = response.Data;
                        InmemoryOrder order = new()
                        {
                            OrderId = result.OrderId,
                            CounterOrderId = targetOrder.OrderId,
                            TradeId = result.TradeId,
                            Quantity = result.Quantity,
                            Price = result.Price,
                            FulfilledQuantity = 0,
                            SignalId = signalId,
                            Signal = targetOrder.Signal,
                            OrderStatus = OrderStatus.New,
                        };

                        OnlineOrders.Add(order);

                        string res =
                            $"주문ID:{result.OrderId}, 신호ID: {targetOrder.SignalId}, 익절 예정가: {exitPrice}, 익절 수량: {targetOrder.FulfilledQuantity}";

                        Message.SendOrderMessage(res);
                    }
                }
                else
                {
                    //Log.InfoAndSend($"{signalId} 신호는 존재하지 않는 신호");
                }
            });
        }
    }
}
