using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeckillWithoutRedis.Cache
{
    /// <summary>
    /// 订单队列 
    /// </summary>
    public class OrderQueue
    {
        private Queue<OrderData> queue = null;
        private object locker = new object();


        public OrderQueue()
        {
            queue = new Queue<OrderData>();
        }

        public int Count()
        {
            lock (locker)
            {
                return queue.Count;
            }
        }


        public void push(OrderData value)
        {

            lock (locker)
            {
                queue.Enqueue(value);
            }
        }


        public List<OrderData> pop(int count = 10)
        {
            if (count < 1) return null;
            lock (locker)
            {
                if (queue.Count == 0)
                {
                    return null;
                }
                else
                {
                    List<OrderData> list = new List<OrderData>();
                    for (int i = 0; i < count; i++)
                    {
                        if (queue.Count > 0)
                        {
                            OrderData req = queue.Dequeue();
                            if (req != null)
                            {
                                list.Add(req);
                            }
                        }
                    }
                    return list;
                }

            }
        }
    }
}
