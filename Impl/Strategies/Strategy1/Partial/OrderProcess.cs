using Microsoft.EntityFrameworkCore;
using Strategy1.Executor.Core.CoreClass;
using Strategy1.Executor.Core.DataClass;
using Strategy1.Executor.Core.DataClass.Rest;
using Strategy1.Executor.Core.DataClass.Stream;
using Strategy1.Executor.Core.EntityClass;
using Strategy1.Executor.Core.Enum;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Impl.DB;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;
using Strategy1.Executor.Impl.Util;

namespace Strategy1.Executor.Impl.Strategies.Strategy1
{
    public partial class Strategy1
        : BaseStrategy<AppDbContext, ETH5M_Candle, ETH5M_Indicator, ETH5M_Signal>,
            IOrderRepository<AppDbContext, ETH5M_Order>
    {
        public override void ProcessEnter(decimal enterPrice, BaseSignal target)
        {
            Console.WriteLine("ProcessEnter 진입");
            ETH5M_Signal signal = (ETH5M_Signal)target;
            decimal totalMargin = CurrentTradeCtx.CurrentMarginRate * CurrentTradeCtx.InitialMargin;
            decimal quantity = Math.Round(totalMargin / enterPrice, 3);
            Task.Run(async () =>
            {
                OrderResult? result = (
                    await RestClientAdapter.PlaceOrderAsync(
                        Symbol,
                        OrderSide.Long,
                        enterPrice,
                        quantity
                    )
                ).Data;

                if (result != null)
                {
                    InmemoryOrder onlineOrder = new()
                    {
                        OrderId = result.OrderId,
                        CounterOrderId = null,
                        TradeId = null,
                        Quantity = result.Quantity,
                        Price = result.Price,
                        FulfilledQuantity = result.FulfilledQuantity,
                        SignalId = signal.Id,
                        Signal = signal,
                        OrderStatus = OrderStatus.New,
                    };

                    OnlineOrders.Add(onlineOrder);
                    Log.InfoAndSendOrder(
                        $"주문ID:{result.OrderId}, 진입 예정가: {enterPrice}, 익절가: {target.TakeProfitPrice}, 손절가: {target.LosscutPrice}, 주문수량:{result.Quantity}"
                    );
                }
                else
                {
                    Message.SendDebugMessage(
                        nameof(ProcessEnter),
                        $"신호 아이디 - {signal.Id}, 매수 실패"
                    );
                }
            });
        }

        public override void ProcessLosscut(DateTime exitTime, BaseSignal signal)
        {
            ETH5M_Signal target = (ETH5M_Signal)signal;

            InmemoryOrder? targetOrder = OnlineOrders.Find(a =>
                a.SignalId == target.Id && a.CounterOrderId == null
            );

            if (targetOrder != null)
            {
                if (
                    targetOrder.FulfilledQuantity > 0
                    && targetOrder.FulfilledQuantity == targetOrder.Quantity
                )
                {
                    // 정상적인 상황
                    LosscutFullFulfillment(target, targetOrder);
                }
                else if (targetOrder.FulfilledQuantity > 0)
                {
                    // 주문 중 일정 수량만 이행됐을 때
                    LosscutPartialFulfillment(target, targetOrder);
                }
                else if (targetOrder.FulfilledQuantity == 0)
                {
                    // 주문이 하나도 이행되지 않았을 때
                    LosscutNoFulfillment(target, targetOrder);
                }
                else if (target.EnterPrice == null)
                {
                    throw new BadDesignException(nameof(ProcessLosscut));
                }
            }
            else
            {
                Log.InfoAndSend($"{signal.Id} 신호는 OnlineOrders 내에 존재하지 않는 신호");
                return;
            }

            FinalizeSignal(target, target.LosscutPrice, exitTime);
            UpdateEntity(target);
            Log.InfoAndSend(
                "손절 시간: {0}, 신호 ID: {1}, 탈출가: {2}, 진입가: {3}, 예상장부손익:{4}",
                exitTime,
                signal.Id,
                signal.LosscutPrice,
                Math.Round((decimal)signal.EnterPrice!, 2),
                Math.Round((decimal)signal.ExpectedProfit!, 2)
            );
        }

        public override void ProcessTakeProfit(decimal exitPrice, DateTime exitTime)
        {
            List<int> reducers = [];

            for (int i = 0; i < OnlineSignals.Count; i++)
            {
                if (OnlineSignals[i].IsTriggered)
                {
                    ETH5M_Signal signal = OnlineSignals[i];

                    ETH5M_Indicator curr = Indicators[^1];
                    if (curr.EMA1 < curr.EMA2)
                    {
                        exitPrice = Candles[^1].Close;
                        MakeTakeProfitOrder(Symbol, exitPrice, signal.Id);
                        FinalizeSignal(signal, exitPrice, exitTime);
                        UpdateEntity(signal);
                        Log.InfoAndSend(
                            "익절 시간: {0}, 신호 ID: {1}, 탈출가: {2}, 진입가: {3}, 예상장부손익:{4}",
                            exitTime,
                            signal.Id,
                            exitPrice,
                            Math.Round((decimal)signal.EnterPrice!, 2),
                            Math.Round((decimal)signal.ExpectedProfit!, 2)
                        );
                        reducers.Add(i);
                    }
                }
            }

            for (int i = reducers.Count - 1; i >= 0; i--)
            {
                OnlineSignals.RemoveAt(reducers[i]);
            }
        }

        public override Action<OrderStreamRecv>? ProcessOnOrderUpdate() =>
            (OrderStreamRecv recv) =>
            {
                OrderStreamData data = recv.Data;

                // catch : status belongs to other reason
                if (data.Status == null)
                {
                    return;
                }

                long orderId = data.OrderId;

                OrderStatus status = (OrderStatus)data.Status;

                InmemoryOrder? curr = OnlineOrders.Find(o => o.OrderId == orderId);

                for (int i = 0; i < 5; i++)
                {
                    if (curr == null)
                    {
                        Task.Delay(250).Wait();
                        curr = OnlineOrders.Find(o => o.OrderId == orderId);
                        continue;
                    }
                    else
                    {
                        curr.FulfilledQuantity = data.FulfilledQuantity;
                        curr.OrderStatus = status;
                        break;
                    }
                }

                if (curr == null)
                {
                    Message.SendOrderMessage($"orderID 매칭 에러");
                    return;
                }

                ETH5M_Order order = new()
                {
                    Finished = false,
                    OrderStatus = status,
                    OrderId = orderId,
                    CounterOrderId = curr.CounterOrderId,
                    Quantity = data.Quantity,
                    Price = data.Price,
                    CreatedAt = data.UpdateTime,
                    SignalId = curr.SignalId,

                    Fee = data.Fee,
                    RealizedProfit = data.RealizedProfit,
                    IsMaker = data.IsMaker,
                    FulfilledQuantity = data.FulfilledQuantity,
                    TradeId = data.TradeId,
                };

                CurrentTradeCtx.CurrentMargin += data.RealizedProfit - data.Fee;

                Task.Run(() =>
                {
                    using AppDbContext db = new();
                    OrderRepo(db).Add(order);
                    db.Entry(CurrentTradeCtx).State = EntityState.Modified;
                    db.SaveChanges();
                });

                if (curr.Quantity == curr.FulfilledQuantity)
                {
                    Message.SendOrderMessage(
                        $"신호ID: {curr.SignalId}, 주문타입: {(data.Side == OrderSide.Long ? "Long" : "Short")} 목표 물량: {curr.Quantity}, 성공 물량: {curr.FulfilledQuantity}"
                    );

                    if (curr.CounterOrderId != null)
                    {
                        InmemoryOrder? origin = OnlineOrders.Find(a =>
                            a.OrderId == curr.CounterOrderId
                        );
                        if (origin == null)
                        {
                            Message.SendOrderMessage(
                                $"origin, curr 불일치 발생, 신호 ID - {curr.SignalId}"
                            );
                            return;
                        }

                        string message;
                        if (origin.FulfilledQuantity == curr.Quantity)
                        {
                            message =
                                $"청산성공, 현재 마진={Math.Round(CurrentTradeCtx.CurrentMargin)}, 현재 초기 마진={Math.Round(CurrentTradeCtx.InitialMargin)}, 현재 Deposit={Math.Round(CurrentTradeCtx.CurrentDeposit)}";
                        }
                        else if (origin.FulfilledQuantity > curr.Quantity)
                        {
                            message =
                                $"청산 포지션 과소, 추가 청산 요구 물량 - {origin.FulfilledQuantity - curr.Quantity}";
                        }
                        else
                        {
                            message =
                                $"청산 포지션 과다, 추가 진입 요구 물량 - {curr.Quantity - origin.FulfilledQuantity}";
                        }

                        Message.SendOrderMessage(message);
                        OnlineOrders.Remove(origin);
                        OnlineOrders.Remove(curr);

                        Task.Run(() =>
                        {
                            using AppDbContext db = new();
                            List<ETH5M_Order> orderList =
                            [
                                .. OrderRepo(db).Where(a => a.SignalId == origin.SignalId),
                            ];
                            foreach (var order in orderList)
                            {
                                order.Finished = true;
                                db.Entry(order).State = EntityState.Modified;
                            }

                            db.SaveChanges();
                        });
                    }
                }
                else if (curr.OrderStatus == OrderStatus.Canceled)
                {
                    if (curr.CounterOrderId == null)
                    {
                        Message.SendOrderMessage(
                            $" 주문 취소 발생, 신호ID: {curr.Signal}, 주문타입: {(data.Side == OrderSide.Long ? "Long" : "Short")} 목표 물량: {curr.Quantity}, 성공 물량: {curr.FulfilledQuantity}"
                        );

                        if (curr.FulfilledQuantity == 0)
                        {
                            OnlineOrders.Remove(curr);
                            Task.Run(() =>
                            {
                                using AppDbContext db = new();
                                List<ETH5M_Order> orderList =
                                [
                                    .. OrderRepo(db).Where(a => a.OrderId == curr.OrderId),
                                ];
                                foreach (var order in orderList)
                                {
                                    order.Finished = true;
                                    db.Entry(order).State = EntityState.Modified;
                                }

                                db.SaveChanges();
                            });
                        }
                    }
                    else
                    {
                        Message.SendDebugMessage(
                            nameof(ProcessOnOrderUpdate),
                            $"Current Order가 Canceled인데 CounterOrderId가 존재하는 이상한 상황 - CounterId={curr.CounterOrderId}"
                        );
                    }
                }
            };
    }
}
