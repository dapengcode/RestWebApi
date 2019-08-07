using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RestWebApi.Request
{
    public interface IRequestValues
    {
        /// <summary>
        /// 获取客户端提交的窗体变量集合
        /// </summary>
        NameValueCollection Form { get; set; }
        /// <summary>
        /// 获取客户端提交的 HTTP 查询字符串集合
        /// </summary>
        NameValueCollection QueryString { get; set; }
        /// <summary>
        /// 获取客户端发送的 HTTP 标头集合 
        /// </summary>
        NameValueCollection Headers { get; set; }
        /// <summary>
        /// 获取客户端发送的 cookies 集合
        /// </summary>
        HttpCookieCollection Cookies { get; set; }
        /// <summary>
        /// 获取客户端上载的文件集合
        /// </summary>
        HttpFileCollectionBase Files { get; set; }
        /// <summary>
        /// 获取遵守网站名称的所请求的 URL 部分
        /// </summary>
        string RawUrl { get; set; }
        /// <summary>
        /// 获取所请求资源的虚拟路径
        /// </summary>
        string Path { get; set; }
        /// <summary>
        /// 获取具有 URL 扩展名的资源的附加路径信息
        /// </summary>
        string PathInfo { get; set; }
        /// <summary>
        /// 获取 URL 数据
        /// </summary>
        Uri Url { get; set; }

        /// <summary>
        /// 获取客户端提交的窗体内容的字节
        /// </summary>
        byte[] FormBody { get; set; }
        /// <summary>
        /// 路由配置
        /// </summary>
        NameValueCollection RouteDatas { get; set; }
        /// <summary>
        /// 请求内容类型
        /// </summary>
        string ContentType { get; set; }
    }
}
