using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;


namespace RestWebApi.Result
{
    /// <summary> 
    /// 文件类型的API返回值
    /// </summary>
    public class FileResult : IApiResult
    {
        private FileResult()
        {

        }
        public readonly byte[] Data;
        public readonly string ContentType;
        public readonly string FileName;
        /// <summary> 
        /// 初始化文件类型的API返回值
        /// </summary>
        /// <param name="data">文件数据</param>
        /// <param name="contentType">文件类型,默认为 text/txt</param>
        /// <param name="fileName">文件名,默认为 随机名称</param>
        public FileResult(byte[] data, string contentType, string fileName)
        {
            Data = data;
            ContentType = contentType;
            FileName = fileName;
        }

        /// <summary> 
        /// 初始化文本文件类型的API返回值
        /// </summary>
        /// <param name="content">文件文本,默认编码为Encoding.Default(ASNI)</param>
        /// <param name="contentType">文件类型,默认为 text/txt</param>
        /// <param name="fileName">文件名,默认为 未命名文件</param>
        public FileResult(string content, string contentType, string fileName)
            : this(Encoding.Default.GetBytes(content ?? ""), contentType, fileName)
        {

        }

        System.Net.Http.HttpResponseMessage IApiResult.GetResponseMessage(IWebApi api)
        {
            switch (api.Agent)
            {
                case ApiAgent.Web:
                    var message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    message.Content = new ByteArrayContent(Data ?? new byte[0]);
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType ?? "application/octet-stream");
                    
                    if (api.UrlReferrer != null)
                    {
                        var name = FileName ?? Guid.NewGuid().ToString("n");
                        if (api.UserAgent.IndexOf("Safari", StringComparison.OrdinalIgnoreCase) > -1
                            && api.UserAgent.IndexOf("Edge", StringComparison.OrdinalIgnoreCase) < 0) //浏览器兼容
                        {
                            message.Content.Headers.Add("Content-Disposition", "attachment;filename=\"" + name + "\"");
                        }
                        else
                        {
                            message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = name,
                                FileNameStar = name,
                                Name = name,
                            };
                        }
                    }

                    return message;
                case ApiAgent.Android:
                case ApiAgent.Ios:
                case ApiAgent.DotNet:
                case ApiAgent.Ajax:
                case ApiAgent.Jsonp:
                    return ((IApiResult)new JsonResult(new
                    {
                        ContentType = ContentType ?? "text/txt",
                        FileName = FileName ?? Guid.NewGuid().ToString("n"),
                        Content = Convert.ToBase64String(Data ?? new byte[0]),
                    })
                    { ExceptionCode = ExceptionCode.File }).GetResponseMessage(api);
                default:
                    return ((IApiResult)new JsonResult(null)
                    {
                        ExceptionCode = ExceptionCode.NotImplemented,
                        Message = "不支持",
                    }).GetResponseMessage(api);
            }
        }
    }
}
