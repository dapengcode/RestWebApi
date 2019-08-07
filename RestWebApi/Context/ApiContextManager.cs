using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestWebApi.Context
{
    internal static class ApiContextManager
    {
        private const string Key = "ApiContextManager_Key";
        public static TimeSpan SlidingExpiration=new TimeSpan(0,2,0);

        public static void SetContainer(IContainer container)
        {
            if(HasContainer)
                throw new ArgumentException("上下文中已经存在容器",nameof(container));
            var policy = new CacheItemPolicy()
            {
                SlidingExpiration = SlidingExpiration,
                RemovedCallback = CacheRemoved,
            };
            if (Debugger.IsAttached) //调试中过期时间变为1小时
            {
                policy.SlidingExpiration = new TimeSpan(1, 0, 0);
            }
            //尝试3次添加到容器缓存
            for (var i = 0; i < 3; i++)
            {
                var uid = Guid.NewGuid();
                if (MemoryCache.Default.AddOrGetExisting(Key + uid, container, policy) == null)
                {
                    //加入默认组件
                    var defcomp = new DefaultComponent(uid);
                    container.Add(defcomp);
                    //添加到上下文
                    CallContext.LogicalSetData(Key, uid);
                    return;
                }
                Thread.Sleep(20);
            }
            throw new NotImplementedException("容器添加到上下文失败");
        }
        /// <summary>
        /// 获取当前逻辑上下文中的容器
        /// </summary>
        public static IContainer Container
        {
            get
            {
                var data = CallContext.LogicalGetData(Key);
                if (data is Guid)
                {
                    return MemoryCache.Default.Get(Key + data) as IContainer;
                }
                return null;
            }
        }
        public static void Dispose()
        {
            var data = CallContext.LogicalGetData(Key);
            if (data == null)
            {
                return;
            }
            CallContext.FreeNamedDataSlot(Key);
            if (data is Guid)
            {
                MemoryCache.Default.Remove(Key + data);
                return;
            }
            using ((IDisposable)data) { };
        }
        public static IEnumerable<IComponent> GetComponents()
        {
            if (Container == null) yield break;
            foreach (var item in Container.Components)
            {
                yield return (IComponent)item;
            }
        }
        /// <summary>
        /// 缓存被移除时触发
        /// </summary>
        /// <param name="arguments"></param>
        private static void CacheRemoved(CacheEntryRemovedArguments arguments)
        {
            using (arguments.CacheItem.Value as IDisposable) { }
        }
        public static bool HasContainer => CallContext.LogicalGetData(Key) is Guid;
        internal class DefaultContainer : Container { }

        internal class DefaultComponent : Component
        {
            public DefaultComponent(Guid uid)
            {
                UID = uid;
            }
            public Guid UID { get; private set; }
            protected override void Dispose(bool disposing)
            {
                MemoryCache.Default.Remove(Key + UID);
                base.Dispose(disposing);
            }
        }
    }
    
}
