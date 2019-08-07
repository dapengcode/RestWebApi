using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.Filters
{
    /// <summary>
    /// 必须登录
    /// </summary>
    public sealed class LoginAttribute:ApiFilterAttribute
    {
        public override object OnActionExecuting(IWebApi api)
        {
            if (api.ApiAction.HasFilter<NoLoginAttribute>()) return null;
            return api.OAuthUser==null || api.OAuthUser.IsAuthenticated == false ? new ApiException(ExceptionCode.NotLogin) : null;
        }

        public override bool Match(object obj)
        {
            return obj is LoginAttribute||obj is NoLoginAttribute;
        }

        public override string Description => "";
    }
}
