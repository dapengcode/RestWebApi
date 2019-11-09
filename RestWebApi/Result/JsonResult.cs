using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace RestWebApi.Result
{
    public class JsonResult : StringResult, ISerializableResult
    {
        internal JsonResult()
            : base(false)
        {
            ServerTime = DateTime.Now;
        }
        /// <summary> 初始化 JsonResult 对象
        /// </summary>
        /// <param name="data">用于API返回的Json.Data中的数据</param>
        /// <remarks></remarks>
        public JsonResult(object data)
            : base(false)
        {
            Data = data;
            ExceptionCode = ExceptionCode.None;
            Message = "";
            ServerTime = DateTime.Now;
        }
        /// <summary> 
        /// 获取或设置 异常代码, 默认正常为0
        /// </summary>
        /// <remarks></remarks>
        [ScriptIgnore]
        public ExceptionCode ExceptionCode { get; set; }

        public int Code { get { return (int)ExceptionCode; } }
        /// <summary>
        /// 获取或设置 错误消息, 默认没有错误为String.Empty
        /// </summary>
        /// <remarks></remarks>
        public string Message { get; set; }
        /// <summary> 
        /// 获取或设置 用于序列化为json字符串的API返回对象
        /// </summary>
        /// <remarks></remarks>
        public object Data { get; set; }
        /// <summary>
        /// 获取或设置 当前服务器时间
        /// </summary>
        /// <remarks></remarks>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// 获取当前对象的Json字符串表现形式
        /// </summary>
        /// <remarks></remarks>
        public override string ToString()
        {
            if (ExceptionCode < 0)
            {
                Trace.WriteLine(Message, "*业务异常*");
            }
            else if (ExceptionCode > 0)
            {
                if ((int)ExceptionCode > 10 && ExceptionCode != ExceptionCode.Redirect)
                {
                    Trace.WriteLine(Message, "系统异常");
                }
                if (ExceptionCode != ExceptionCode.Html)
                {
                    //防止xss攻击, 返回值编码
                    Message = Message?.Replace("<", "&lt;")?.Replace(">", "&gt;");
                }
            }
            if (ExceptionCode == ExceptionCode.Debug && Api.Agent == ApiAgent.Web)
            {
                if (Message != null) return Message;
            }
            if (Message == null)
            {
                Message = "";
            }
            ContentType = "application/json";
            try
            {
                var json = JsonConvert.SerializeObject(this, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
                Trace.TraceInformation(json);
                return json;
            }
            catch (Exception e)
            {
                Trace.TraceError($"json序列化发生错误:{e.Message}");
                return string.Empty;
            }
            
            
            
        }

        IApiResult ISerializableResult.ToApiResult()
        {
            if (Message == null)
            {
                Message = "";
            }
            var json = JsonConvert.SerializeObject(this, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });

            return new JsonSerializableResult() { Json = json };
        }

        private class JsonSerializableResult : StringResult
        {
            public JsonSerializableResult()
                : base(false)
            {
            }
            public string Json { private get; set; }

            public override string ToString()
            {
                if (Json == null)
                {
                    return "";
                }
                //var index = Json.LastIndexOf("\"ServerTime\":\"", StringComparison.OrdinalIgnoreCase);
                //string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ContentType = "application/json";
                return Json;
            }
        }

       
    }
}
