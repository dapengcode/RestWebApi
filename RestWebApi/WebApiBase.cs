using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using RestWebApi.Session;
using RestWebApi.Action;
using RestWebApi.Config;
using RestWebApi.Filters;
using RestWebApi.OAuth;
using RestWebApi.Request;
using RestWebApi.Result;
using RestWebApi.Sign;
using RestWebApi.Tools;

namespace RestWebApi
{
    public abstract class WebApiBase : IWebApi
    {
        /// <summary>
        /// 浏览器代理信息
        /// </summary>
        public string UserAgent { get; internal set; }
        /// <summary>
        /// 客户端请求信息
        /// </summary>
        public ApiAgent Agent { get; private set; }
        /// <summary>
        /// 是否浏览器发起的请求
        /// </summary>
        public bool IsBrowser => Agent == ApiAgent.Web
                                 || Agent == ApiAgent.Ajax
                                 || Agent == ApiAgent.Jsonp;

        /// <summary>
        /// Session信息
        /// </summary>
        public HttpSessionStateBase Session { get; private set; }
        /// <summary>
        /// 缓存信息
        /// </summary>
        public ObjectCache Cache { get; private set; }
        /// <summary>
        /// 属性集合
        /// </summary>
        public PropertyCollection Properties { get; private set; }
        /// <summary>
        /// 用户IP
        /// </summary>
        public string HostAddress { get; internal set; }
        /// <summary>
        /// 用户真实IP
        /// </summary>
        public string RealHostAddress { get; internal set; }
        /// <summary>
        /// 当前请求地址
        /// </summary>
        public Uri Url { get; internal set; }



        /// <summary>
        /// 上一个页面
        /// </summary>
        public Uri UrlReferrer { get; internal set; }
        /// <summary>
        /// 请求的方法名
        /// </summary>
        public string ActionName { get; internal set; }
        /// <summary>
        /// 接口版本
        /// </summary>
        public string ActionVersion { get; internal set; }
        /// <summary>
        /// Action封装
        /// </summary>
        public ApiAction ApiAction { get; private set; }
        /// <summary>
        /// 服务器内部异常
        /// </summary>
        public Exception Exception { get; private set; }
        /// <summary>
        /// 对请求值的封装
        /// </summary>
        public RequestValues RequestValues { get; internal set; }

        /// <summary>
        ///  客户端请求方式
        /// </summary>
        public string Method { get; internal set; }
        /// <summary>
        /// 设置头部信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public abstract void SetHeader(string name, string value);

        /// <summary>
        ///设置Cookie 
        /// </summary>
        /// <param name="cookie"></param>
        public abstract void SetCookie(HttpCookie cookie);

        public virtual ApiSessionState SessionState => ApiSessionState.Auto;

        #region 用户

        private OAuthUser _oauthUser;
        /// <summary>
        ///  当前登录用户
        /// </summary>
        public OAuthUser OAuthUser
        {
            get
            {
                if (_oauthUser != null) return _oauthUser;
                var userToken = OAuths.GetAccessToken(this);
                var sessionUser = OAuths.GetOAuthUserByAccessToken(this, userToken);
                if (sessionUser == null)
                {
                    OnInitUser();
                }
                else
                {
                    _oauthUser = sessionUser;
                }

                return _oauthUser;

            }
            set
            {
                _oauthUser = value;
                Session["LoginedUsers.AccessToken=" + _oauthUser.AccessToken] = _oauthUser;
            }
        }
        /// <summary>
        /// 是否已经登录
        /// </summary>
        /// <param name="accessToken">登录令牌</param>
        /// <returns></returns>
        protected bool IsLogined(Guid accessToken)
        {
            return Session.Keys.Cast<string>().Any(it => it == "LoginedUsers.AccessToken=" + accessToken);
        }

        public Guid AccessToken()
        {
            return OAuths.GetAccessToken(this);
        }

        #endregion

        #region 事件处理委托

        private EventHandler _onError;
        event EventHandler IWebApi.OnError
        {
            add
            {
                if (value == null) return;
                _onError -= value;
                _onError += value;
            }
            remove
            {
                if (value != null) _onError -= value;
            }
        }
        #endregion

        #region 构造函数

        protected WebApiBase()
        {
            Properties = new PropertyCollection();
            Session = CreateSession();
            Cache = CreateCache();
        }
        private static readonly ObjectCache DefaultCache = new MemoryCache("WeiApi$DefaultCache");
        /// <summary>
        /// 创建缓存
        /// </summary>
        /// <returns></returns>
        private static ObjectCache CreateCache()
        {
            if (ApiConfig.CacheType == null) return DefaultCache;
            return Activator.CreateInstance(ApiConfig.CacheType) as ObjectCache ?? DefaultCache;
        }
        /// <summary>
        /// 创建Session
        /// </summary>
        /// <returns></returns>
        private static HttpSessionStateBase CreateSession()
        {
            if (ApiConfig.SessionType == null) return new MemorySession();
            return Activator.CreateInstance(ApiConfig.SessionType) as HttpSessionStateBase ?? new MemorySession();
        }

        #endregion

        #region Api执行事件

        protected virtual Task<IApiResult> OnExecuting(string methodName)
        {
            return Task.FromResult(default(IApiResult));
        }

        protected virtual Task<IApiResult> OnExecuted(string methodName, object result)
        {
            return Task.FromResult(default(IApiResult));
        }

        protected virtual Task<IApiResult> OnExecuteError(string methodName, Exception ex)
        {
            return Task.FromResult(default(IApiResult));
        }

        #endregion

        #region Invoke

        protected internal async Task<object> InvokeAction()
        {
            SetHeader("Last-Modified", DateTime.Now.ToString("r"));
            //跨域
            if (UrlReferrer != null &&
                Uri.Compare(UrlReferrer, Url, UriComponents.SchemeAndServer, UriFormat.Unescaped,
                    StringComparison.OrdinalIgnoreCase) != 0
                && HeaderContains("X-CrossDomain", "true"))
            {
                SetHeader("Access-Control-Allow-Credentials", "true");
                SetHeader("Access-Control-Allow-Origin",
                    UrlReferrer.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped));
            }
            else
            {
                SetHeader("Access-Control-Allow-Origin", "*");
                SetHeader("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE,OPTIONS");
                SetHeader("Access-Control-Allow-Headers", "Accept, Origin, XRequestedWith, Content-Type, LastModified,Access-Token");
            }
            var etag = RequestValues.Headers["If-None-Match"];
            if (etag != null && Cache[etag] != null)
                return NotModifiedResult.Instance;
            Exception apiError;
            try
            {
                InitAgent();
                RewriteReferrer();
                return SaveSession(await ActionInvoker());
            }
            catch (Exception e)
            {
                apiError = e;
            }
            var task = OnExecuteError(ActionName, apiError);
            var r = task == null ? null : await task;
            return SaveSession(r ?? ErrorResult(apiError));

        }
        static readonly bool _NoSign = System.Configuration.ConfigurationManager.AppSettings["NoSign"] == "true";
        private async Task<object> ActionInvoker()
        {
            var action = ApiActionFactory.Get(this, ActionName, ActionVersion);
            if (action == null)
                return ErrorResult(new ApiException(ExceptionCode.ApiNotFound));
            if (action.Obsolete != null && action.Obsolete.IsError)
            {
                return ErrorResult(new ApiException(ExceptionCode.ApiObsolete, action.Obsolete.Message));
            }
            ActionName = action.Name;
            ApiAction = action;
            //验证签名
            if (action.HasFilter<SignAttribute>())
            {
                var signer = new Signer();
                if (_NoSign == false)
                {
                    var ex =  signer.CheckAsync(this); //验证签名返回错误消息
                    if (ex != null) //如果必须签名,则抛出异常
                    {
                        return ErrorResult(ex);
                    }
                }


            }


            //是否启动Session
            var sessionState = SessionState;
            if (sessionState == ApiSessionState.Auto)
                sessionState = IsBrowser ? ApiSessionState.Enable : ApiSessionState.Disable;
            switch (sessionState)
            {
                case ApiSessionState.Enable:
                    Properties["SessionState"] = 1;
                    var err = await OnInitSession();
                    if (err != null)
                        return ErrorResult(err);
                    break;
                case ApiSessionState.Disable:
                case ApiSessionState.Auto:
                default:
                    Properties["SessionState"] = 0;
                    Trace.WriteLine("本次请求禁用Session");
                    Session = new DictionarySession();
                    break;
            }
            //用户
            if (!action.HasFilter<NoLoginAttribute>())
            {
                //初始化用户 
                var task = OnInitUser();
                var ex = task.IsNull() ? null : await task;
                if (ex != null)
                    return ErrorResult(ex);
            }
            //解析Url参数
            Exception parseError;
            var args = action.GetArguments(RequestValues, out parseError);
            if (parseError != null)
                return ErrorResult(parseError);
            //验证参数

            //执行前
            {
                var task = OnExecuting(action.Name);
                var r = await task;
                if (r != null)
                {
                    Trace.WriteLine("Api.OnExecuting截断");
                    return ProcessResult(r);
                }
            }
            //前Filter
            {
                var r = await action.OnActionExecuting(this);
                if (r != null)
                {
                    Trace.WriteLine("Filter.OnExecuting截断");
                    return ProcessResult(r);
                }
            }
            //执行Action
            var result = await ApiAction.TryRunTask(action.Execute(this, args));
            {//后Filter
                var r = await action.OnActionExecuted(this, result);
                if (r != null)
                {
                    Trace.WriteLine("Filter.OnExecuted截断");
                    return ProcessResult(r);
                }
            }

            {//执行后
                var task = OnExecuted(action.Name, result);
                var r = await task;
                if (r == null) return ProcessResult(result);
                Trace.WriteLine("Api.OnExecuted截断");
                return ProcessResult(r);
            }


        }
        private object ProcessResult(object result)
        {
            var ex = result as Exception;
            if (ex != null)
            {
                return ErrorResult(ex);
            }
            return result;
        }
        private object SaveSession(object result)
        {
            if (string.IsNullOrEmpty(Session?.SessionID)) return result;
            Session["WebApiBase.Url.Host"] = Url.Host;
            if (Session.IsNewSession)
            {
                var cookie = new HttpCookie(Url.Port != 80 ? "sessionid" + Url.Port : "sessionid", Session.SessionID)
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = Url.Scheme == Uri.UriSchemeHttps
                };
                SetCookie(cookie);
                Trace.WriteLine("写入Session:" + Session.SessionID);
            }
            (Session as IObserver<string>)?.OnCompleted();
            return result;

        }

        private void RewriteReferrer()
        {
            if (!ApiConfig.IsDebug && UrlReferrer == null) return;
            var _ref = RequestValues.QueryString["_ref"];
            Uri rewriteUrl;
            if (_ref == null || !Uri.TryCreate(_ref, UriKind.RelativeOrAbsolute, out rewriteUrl)) return;
            if (rewriteUrl.IsAbsoluteUri)
            {
                var domain = rewriteUrl.Host;//对url进行验证
            }
            else
            {
                rewriteUrl = new Uri(UrlReferrer ?? Url, rewriteUrl);
            }
            UrlReferrer = rewriteUrl;
        }

        private void InitAgent()
        {
            var agt = RequestValues["Agent"];
            if (!string.IsNullOrEmpty(agt))
            {
                ApiAgent agent;
                int code;
                if (int.TryParse(agt, out code) &&
                    Enum.TryParse(agt, true, out agent))
                    Agent = agent;
                else
                {
                    Agent = ApiAgent.Wrong;
                }
            }
            else
            {
                Agent = ApiAgent.Web;
            }
        }
        private string _rawSessionId;
        private async Task<Exception> OnInitSession(bool reset = false)
        {
            var b = Session as IObserver<string>;
            if (b != null)
            {
                if (reset)
                {
                    Trace.WriteLine("重置Session");
                    b.OnNext(null);
                }
                else
                {
                    var sessionid = GetSessionId();
                    b.OnNext(sessionid);
                }
            }
            var host = Session["WebApiBase.Url.Host"] ?? null;
            if (reset == false && host != null && (string)host != Url.Host) //串Session
            {
                Trace.WriteLine("Session串站,准备重置");
                Trace.WriteLine(Session.SessionID, "OldSessionID");
                Trace.WriteLine(host, "SessionHost");
                Trace.WriteLine(Url.Host, "CurrentHost");
                await OnInitSession(true);
                return null;
            }
            Trace.WriteLine(Session.SessionID, "SessionID");
            _rawSessionId = Session.SessionID;
            return null;
        }
        protected virtual string GetSessionId()
        {
            string sessionid = null;
            var cookies = RequestValues.Cookies;
            var count = cookies.Count;
            for (var i = 0; i < count; i++)
            {
                var cookie = cookies[i];
                if (cookie != null && cookie.Name != SeesionFieldKey) continue;
                if (sessionid == null)
                {
                    if (cookie != null) sessionid = cookie.Value;
                }
                else
                {
                    SetCookie(new HttpCookie(SeesionFieldKey, null)
                    {
                        Domain = Url.Host,
                    });
                    Trace.WriteLine("值有多个,无法确认", "Cookie.SessionID");
                    return null;
                }
            }
            Trace.WriteLine(sessionid ?? "", "Cookie.SessionID");
            return sessionid;
        }
        /// <summary>
        /// User属性初始化时触发
        /// </summary>
        /// <returns></returns>
        public virtual Task<Exception> OnInitUser()
        {
            return EmptyTask<Exception>.Null;
        }

        /// <summary> 
        /// 获取当前会话的唯一标识符
        /// </summary>
        private string SeesionFieldKey
        {
            get
            {
                if (Url.Port != 80)
                {
                    return "sessionid" + Url.Port;
                }
                return "sessionid";
            }
        }
        private bool HeaderContains(string name, string value)
        {
            var values = RequestValues.Headers[name]?.Split(',');
            return values != null && values.Any(t => string.Equals(t.Trim(), value, StringComparison.OrdinalIgnoreCase));
        }

        private IApiResult ErrorResult(System.Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }
            IApiResult result = null;

            if (ex.HelpLink != null)
            {
                return new JsonResult(null) { ExceptionCode = (ExceptionCode)ex.HResult, Message = ex.Message };
            }
            else if(ex is TargetInvocationException)
            {
                return ErrorResult(ex.InnerException);
            }
            else
            {
                var e = ex as AggregateException;
                if (e?.InnerExceptions.Count == 1)
                {
                    return ErrorResult(e.InnerException);
                }
            }
            Exception = ex;

            var ent = _onError;
            if (ent != null)
            {
                try
                {
                    ent(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e, "OnError异常");
                }
            }


            Trace.WriteLine(ex, "WebApi Exception");
            if (ApiConfig.IsDebug)
            {
                result = new JsonResult(null)
                {
                    ExceptionCode = ExceptionCode.Debug,
                    Message = ex.Message + Environment.NewLine + ex.ToString(),
                };
            }
            else
            {
                result = new JsonResult(null)
                {
                    ExceptionCode = ExceptionCode.Unknown,
                    Message = ex.Message
                };
                SetHeader("X-LogID", (ex.Data["logid"] ?? "未知") + "");
            }
            return result;
        }

        #endregion
    }
}
