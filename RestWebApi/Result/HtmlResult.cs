using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RestWebApi.Result
{
    /// <summary>
    /// 表示一段html类型的WebApi返回值
    /// </summary>
    public sealed class HtmlResult : JsonResult, ISerializableResult
    {
        private HtmlResult()
        {
            ExceptionCode = ExceptionCode.Html;
        }
        /// <summary>
        /// 构造一个html类型的WebApi返回值
        /// </summary>
        /// <param name="body">html中body中的内容,不包含body标签</param>
        public HtmlResult(string body)
        {
            ExceptionCode = ExceptionCode.Html;
            Message = string.Format(Html, body);
        }

        /// <summary>
        /// 构造一个html类型的WebApi返回值
        /// </summary>
        /// <param name="html">html中html</param>
        /// <param name="warp">warp是否需要用html,body标签包裹</param>
        public HtmlResult(string html, bool warp)
            : base(false)
        {
            ExceptionCode = ExceptionCode.Html;
            Message = warp ? string.Format(Html, html) : html;
        }

        private const string Html = @"
<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title></title>
</head>
<body>
    {0}
</body>
</html>";

        /// <summary> 
        /// 根据请求类型的不同返回不同的结果
        /// </summary>
        /// <remarks></remarks>
        public override string ToString()
        {
            if (Api.Agent == ApiAgent.Web)
            {
                ContentType = "text/html";
                return Message;
            }
            else
            {
                return base.ToString();
            }
        }

        IApiResult ISerializableResult.ToApiResult()
        {
            return this;
        }
    }
}
