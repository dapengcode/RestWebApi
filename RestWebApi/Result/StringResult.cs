using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;


namespace RestWebApi.Result
{
    public class StringResult:IApiResult
    {
        private StringResult()
        {
        }
        public StringResult(string str)
        {
            _str = str;
            _escape = true;
        }
        public override string ToString()
        {
            return _str ?? string.Empty;
        }
        protected string ContentType { get; set; }
        protected IWebApi Api { get; private set; }
        /// <summary>
        /// 初始化 StringResult 对象
        /// </summary>
        /// <param name="escape">当发现jsonp时以后需要转换为json格式(内部双引号/回车/特殊字符转义,前后加双引号)</param>
        /// <remarks></remarks>
        public StringResult(bool escape)
        {
            _escape = escape;
        }

        private readonly string _str;
        private readonly bool _escape;

        public HttpResponseMessage GetResponseMessage(IWebApi api)
        {
            Api = api;
            var content = GetHttpContent();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content,
            };
        }
        private HttpContent GetHttpContent()
        {
            var str = this.ToString();
            var jsonpName = Api.RequestValues.QueryString["callback"];
            if (jsonpName == null)
            {
                return new StringContent(str, Encoding.UTF8, ContentType ?? "text/plain");
            }
            else if (jsonpName.IndexOfAny(OutLimitChars) > -1)
            {
                return new StringContent("请求包含非法脚本", Encoding.UTF8, "text/plain");
            }
            if (_escape)
            {
                str = Newtonsoft.Json.JsonConvert.SerializeObject(this.ToString(), new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            }
            return new StringContent(string.Concat(jsonpName, "(", str, ")"), Encoding.UTF8, "application/javascript");
        }

        private static readonly char[] OutLimitChars = new[] { '<', '(', '\'', '"', ';', ',', '.', '/', '%' };
    }
}
