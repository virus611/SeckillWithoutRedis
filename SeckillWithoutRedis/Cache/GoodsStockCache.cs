using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeckillWithoutRedis.Cache
{
    /// <summary>
    /// 商品库存缓存类,IOC单例
    /// </summary>
    public class GoodsStockCache
    {
        private readonly Dictionary<long, int> dict;
        private readonly object locker;

        public GoodsStockCache()
        {
            locker = new object();
            dict = new Dictionary<long, int>(10000);
        }

        #region 初始化商品

        /// <summary>
        /// 批量初始化商品
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public Task<int> AddGoods(List<GoodsModel> list)
        {
            if (list == null || list.Count == 0)
                return Task.FromResult((int)ErrType.ArgsIsNull);
            Monitor.Enter(locker);

            list.ForEach(item =>
            {
                dict[item.id] = item.stock;
            });

            Monitor.Exit(locker);
            return Task.FromResult((int)ErrType.Success);
        }

        /// <summary>
        /// 修改某商品库存
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="Append"></param>
        /// <returns></returns>
        public Task<int> AddGoods(GoodsModel obj, bool Append = false)
        {
            if (obj == null || obj.stock == 0)
                return Task.FromResult((int)ErrType.ArgsIsNull);
            Monitor.Enter(locker);
            if (Append)
            {
                //追加
                if (dict.ContainsKey(obj.id))
                {
                    int s = dict[obj.id] + obj.stock;
                    dict[obj.id] = s;
                }
                else
                {
                    dict[obj.id] = obj.stock;
                }
            }
            else
            {
                //直接配值
                dict[obj.id] = obj.stock;
            }
            Monitor.Exit(locker);
            return Task.FromResult((int)ErrType.Success);
        }
        #endregion


        #region 减少或者清0

        public Task<int> clear()
        {
            Monitor.Enter(locker);
            dict.Clear();
            Monitor.Exit(locker);
            return Task.FromResult((int)ErrType.Success);
        }

        /// <summary>
        /// 减少库存
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public Task<int> Reduce(long id, int num)
        {
            if (Monitor.TryEnter(locker, 2000))
            {
                ErrType flag = ErrType.Success;
                int stock;
                if (dict.TryGetValue(id, out stock))
                {
                    //有该商品
                    if (num > stock)
                    {
                        //超量了
                        flag = ErrType.NotEnough;
                    }
                    else
                    {
                        stock = stock - num;
                        dict[id] = stock;
                    }
                }
                else
                {
                    flag = ErrType.NotExist;
                }
                Monitor.Exit(locker);
                return Task.FromResult((int)flag);
            }
            else
            {
                return Task.FromResult((int)ErrType.TimeOut);
            }
        }

        /// <summary>
        /// 批量减少库存
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public Task<int> Reduce(List<GoodsModel> list)
        {
            if (list == null || list.Count == 0) return Task.FromResult((int)ErrType.ArgsIsNull);
            if (Monitor.TryEnter(locker, 2000))
            {
                ErrType flag = ErrType.Success;
                List<GoodsModel> editlist = new List<GoodsModel>();
                //检查数据合法性
                list.ForEach(item =>
                {
                    int Stock;
                    if (dict.TryGetValue(item.id, out Stock))
                    {
                        //有该商品
                        if (item.stock > Stock)
                        {
                            //超量了
                            flag = ErrType.NotEnough;
                        }
                        else
                        {
                            editlist.Add(new GoodsModel()
                            {
                                id = item.id,
                                stock = Stock - item.stock
                            });
                        }
                    }
                    else
                    {
                        flag = ErrType.NotExist;
                    }
                });
                //数据合法再修改
                if (flag == ErrType.Success)
                {
                    editlist.ForEach(item =>
                    {
                        dict[item.id] = item.stock;
                    });
                }
                Monitor.Exit(locker);
                return Task.FromResult((int)flag);
            }
            else
            {
                return Task.FromResult((int)ErrType.TimeOut);
            }
        }
        #endregion


        #region 信息输出（测试用） 
        public Task<string> getInfo()
        {
            StringBuilder sb = new StringBuilder(3000);
            Monitor.Enter(locker);

            foreach (KeyValuePair<long, int> item in dict)
            {
                if (item.Value >= 0)
                {
                    sb.Append($"{item.Key} ------  {item.Value}\r\n");
                }
            }

            Monitor.Exit(locker);
            return Task.FromResult(sb.ToString());
        }
        #endregion
    }




    public class GoodsModel
    {
        public long id { get; set; }
        public int stock { get; set; }
    }


    public enum ErrType
    {
        Success = 200,
        ArgsIsNull = 0,
        NotExist = 1,
        NotEnough = 2,
        TimeOut = 3,
    }
}
