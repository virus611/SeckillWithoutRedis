using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeckillWithoutRedis.Cache
{
    /// <summary>
    /// 订单消耗入库服务，生命周期同项目
    /// </summary>
    public class OrderService : BackgroundService
    { 
        private readonly OrderQueue queue; 
        public OrderService(OrderQueue orderQueue )
        {
            queue = orderQueue;  
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                OrderToDB(); 
                await Task.Delay(100); 
            } 
        }
         
        void OrderToDB()
        {
            List<OrderData> list = queue.pop(200);
            if (list != null && list.Count > 0)
            {
                //list写入数据库中
            }
        }
         
    }
}
