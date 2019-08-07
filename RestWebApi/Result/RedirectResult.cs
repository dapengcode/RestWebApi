using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;



namespace RestWebApi.Result
{
    /// <summary>
    /// 页面重定向类型的API返回值
    /// </summary>
    /// <remarks></remarks>
    public class RedirectResult : IApiResult
    {
        private RedirectResult()
        {

        }
        /// <summary> 
        /// 需要重定向的URL
        /// </summary>
        /// <remarks></remarks>
        public readonly string Url;
        /// <summary>
        /// 是否包含应用页
        /// </summary>
        public readonly bool IncludeRef;
        /// <summary> 
        /// 构造函数,初始化重定向返回值
        /// </summary>
        /// <param name="url">需要重定向的URL</param>
        /// <param name="includeRef">是否需要包含引用页参数</param>
        /// <remarks>周子鉴 2015.11.13</remarks>
        public RedirectResult(string url, bool includeRef)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");
            Url = url;
            IncludeRef = includeRef;
        }

        System.Net.Http.HttpResponseMessage IApiResult.GetResponseMessage(IWebApi api)
        {
            var url = Url;
            if (IncludeRef)
            {
                var arg = "_referrer=" + Uri.EscapeDataString(api.Url.ToString());
                var i = url.IndexOf('#');
                if (i >= 0)
                {
                    var anchor = url.Substring(i);
                    url = url.Remove(i);
                    url += (url.Contains("?") ? "&" : "?") + arg + anchor;
                }
                else
                {
                    url += (url.Contains("?") ? "&" : "?") + arg;
                }
            }

            

            if (api.Agent == ApiAgent.Web)
            {
                var messsage = new HttpResponseMessage(HttpStatusCode.Redirect);
                messsage.Headers.Add("Location", url);
                return messsage;
            }
            else
            {
                return ((IApiResult)new JsonResult(null)
                {
                    ExceptionCode = ExceptionCode.Redirect,
                    Message = url,
                }).GetResponseMessage(api);
            }
        }

    }
}
