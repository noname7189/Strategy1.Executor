using Microsoft.EntityFrameworkCore;
using Strategy1.Executor.Core.DataClass;
using Strategy1.Executor.Core.EntityClass;
using Strategy1.Executor.Core.Interface;
using Strategy1.Executor.Impl.DB;

namespace Strategy1.Executor.Impl.Util
{
    public class OrderUtil
    {
        public static List<InmemoryOrder> GetInmemoryOrders<O>(
            IOrderRepository<AppDbContext, O> repo
        )
            where O : BaseOrder
        {
            using AppDbContext db = new();
            List<O> orderList =
            [
                .. repo.OrderRepo(db).Where(a => a.Finished == false).Include(o => o.Signal),
            ];

            Dictionary<long, O> orderDic = [];
            foreach (var elem in orderList)
            {
                if (orderDic.TryGetValue(elem.OrderId, out var origin))
                {
                    if (elem.FulfilledQuantity > origin.FulfilledQuantity)
                    {
                        orderDic[elem.OrderId] = elem;
                    }
                }
                else
                {
                    orderDic.Add(elem.OrderId, elem);
                }
            }

            IEnumerable<O> distinctOrderList = orderDic.Select(o => o.Value);

            List<InmemoryOrder> res = [];
            foreach (var item in distinctOrderList)
            {
                InmemoryOrder elem = new()
                {
                    TradeId = item.TradeId,
                    OrderId = item.OrderId,
                    CounterOrderId = item.CounterOrderId,
                    SignalId = item.SignalId,
                    Signal = item.Signal,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    FulfilledQuantity = item.FulfilledQuantity,
                    OrderStatus = item.OrderStatus,
                };
                res.Add(elem);
            }

            return res;
        }
    }
}
