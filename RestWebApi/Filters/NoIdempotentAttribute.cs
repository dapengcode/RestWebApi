using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.Filters
{
    /// <summary>
    /// 无需幂等过滤器
    /// </summary>
    public class NoIdempotentAttribute:ApiFilterAttribute
    {
        public override object OnActionExecuting(IWebApi api)
        {
            api.SetHeader("Cache-Control","no-cache");
            return null;
        }

        public override bool Match(object obj)
        {
            return obj is NoIdempotentAttribute || obj is IdempotentAttribute;
        }
    }
}
