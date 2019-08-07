using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using RestWebApi.Session;
using RestWebApi.OAuth;
using RestWebApi.Action;
using RestWebApi.Request;

namespace RestWebApi
{
    /// <summary>
    ///     对RESTFul Api的行为进一步进行封装
    /// </summary>
    public interface IWebApi
    {
        /// <summary>
        ///     浏览器代理信息
        /// </summary>
        string UserAgent { get; }

        /// <summary>
        ///     客户端请求代理信息
        /// </summary>
        ApiAgent Agent { get; }

        bool IsBrowser { get; }

        /// <summary>
        ///     Session信息
        /// </summary>
        HttpSessionStateBase Session { get; }

        /// <summary>
        ///     Cache信息
        /// </summary>
        ObjectCache Cache { get; }

        /// <summary>
        ///     关联的属性集合
        /// </summary>
        PropertyCollection Properties { get; }

        /// <summary>
        /// 用户IP
        /// </summary>
        string HostAddress { get; }

        /// <summary>
        /// 用户真实IP
        /// </summary>
        string RealHostAddress { get; }

        /// <summary>
        /// 当前请求地址
        /// </summary>
        Uri Url { get; }
        /// <summary>
        /// 当前登录用户
        /// </summary>
        OAuthUser OAuthUser { get; set; }

        /// <summary>
        /// 上一个页面地址
        /// </summary>
        Uri UrlReferrer { get; }

        /// <summary>
        /// 当前请求的方法名
        /// </summary>
        string ActionName { get; }
        /// <summary>
        /// 接口版本
        /// </summary>
        string ActionVersion { get; }
        /// <summary>
        /// 
        /// </summary>
        ApiAction ApiAction { get; }
        /// <summary>
        /// 对请求值的封装
        /// </summary>
        RequestValues RequestValues { get; }
        /// <summary>
        ///  客户端请求方式
        /// </summary>
        string Method { get; }

        /// <summary>
        ///  当前请求的异常
        /// </summary>
        Exception Exception { get; }
        /// <summary>
        /// 设置请求响应头
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetHeader(string name, string value);
        /// <summary>
        /// 设置请求响应Cookie
        /// </summary>
        /// <param name="cookie"></param>
        void SetCookie(HttpCookie cookie);

        /// <summary>
        /// 服务器出现异常时触发
        /// </summary>
        event EventHandler OnError;

        /// <summary>
        /// 获取用户访问AccessToken
        /// </summary>
        Guid AccessToken();
        /// <summary>
        /// 用户初始化
        /// </summary>
        /// <returns></returns>
        Task<Exception> OnInitUser();
    }

    /// <summary>
    /// 客户端请求代理信息
    /// </summary>
    public enum ApiAgent
    {
        Web = 0,
        Ajax = 1,
        Jsonp = 2,
        Android = 3,
        Ios = 4,
        DotNet = 5,
        Java = 6,
        Python = 7,
        Php = 8,
        Go = 9,
        Ruby = 10,
        NodeJs = 11,
        Wrong = 1024
    }
}