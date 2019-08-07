using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.Filters
{
    /// <summary>
    /// 无需登录
    /// </summary>
    public sealed class NoLoginAttribute:ApiFilterAttribute
    {
        public override bool Match(object obj)
        {
            return obj is NoLoginAttribute;
        }
    }
}
