using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using RestWebApi.Config;
using RestWebApi.Context;
using RestWebApi.Request;
using RestWebApi.Result;

namespace RestWebApi
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WebApi : WebApiBase, IHttpController
    {
        #region
        protected  HttpControllerContext ControllerContext { get; set; }
        protected internal HttpRequestMessage Request { get; private set; }
        protected internal IHttpRouteData RouteData { get; private set; }
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
        private const string OwinContext = "MS_OwinContext";
        private static readonly string HostName = Dns.GetHostName();
        private static readonly string HostAddresses = string.Join(",", Dns.GetHostAddresses(Dns.GetHostName()).Select(it => it.ToString()).Where(it => it.Contains(".")));
        private static readonly string[] ContentHeaders = { "Last-Modified", "Expires" };
        private static readonly char[] FileWarpper = { ' ', '\"' };
        #endregion
        async Task<HttpResponseMessage> IHttpController.ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            SynchronizationContext.SetSynchronizationContext(null);
            ControllerContext = controllerContext;
            using (var api = new ApiContext(this))
            {
                
                Trace.WriteLine(HostName, "HostName");
                Trace.WriteLine(HostAddresses, "HostAddresses");

                await InitRequestValues();

                var response = ProcessResult(await InvokeAction());

                ResponseEnd(response);
                response.Headers.Remove("Server");
                return response;
            }
        }
        
        private Stopwatch _stopwatch;
        protected double Timing => _stopwatch.Elapsed.TotalMilliseconds;
        private async Task InitRequestValues()
        {
            _stopwatch = Stopwatch.StartNew();
            Properties["Stopwatch"] = _stopwatch;
            RouteData = ControllerContext.RouteData;
            Request = ControllerContext.Request;
            Url = Request.RequestUri;
            ActionName = RouteData.Values["action"] as string;
            ActionVersion = RouteData.Values["action_version"] as string;
            RealHostAddress = HostAddress = GetClientIpAddress();

            IEnumerable<string> ips;
            if (Request.Headers.TryGetValues("X-Forwarded-For", out ips))
            {
                HostAddress = string.Join(",", ips);
            }
            UrlReferrer = Request.Headers.Referrer;
            UserAgent = Request.Headers.UserAgent.ToString();
            Method = Request.Method.Method.ToUpperInvariant();
            if (ActionName == "???")
            {
                if (Request.Method == HttpMethod.Post)
                {
                    ActionName = "Get";
                }
                else
                {
                    ActionName = Request.Method.Method;
                }
            }

            var contentType = Request.Content?.Headers?.ContentType?.MediaType ?? "";
            var isJson = contentType.Equals("application/json", StringComparison.OrdinalIgnoreCase);

            IRequestValues values;
            var ctx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            if (ctx != null)
            {
                values = new RequestValues(ctx.Request.Unvalidated);
                ctx.Request.InputStream.Position = 0;
                var bytes = ctx.Request.BinaryRead(ctx.Request.ContentLength);
                values.FormBody = bytes;
            }
            else
            {
                values = new RequestValues();
                if (isJson == false)
                {
                    await ReadBody(values);
                }
                ReadQueryStrings(values);
                ReadCookies(values);
                ReadHeaders(values);
                ReadOther(values);
            }

            if (isJson)
            {
                var charset = Request.Content?.Headers?.ContentType?.CharSet;
                var encoding = charset == null ? Encoding.UTF8 : Encoding.GetEncoding(charset);
                var json = encoding.GetString(values.FormBody);
                values.Form = new JsonFormBody(json);
            }

            values.ContentType = contentType;
            ReadRouteData(values);
            //values.ReadOnly();
            RequestValues = (RequestValues)values;
            WirteLog();
        }
        private void ReadRouteData(IRequestValues values)
        {
            var nv = new NameValueCollection();
            foreach (var item in ControllerContext.RouteData.Values)
            {
                nv.Add(item.Key, item.Value + "");
            }
            values.RouteDatas = nv;
        }

        private void ReadOther(IRequestValues values)
        {
            values.Url = Url;
            values.Path = Url.LocalPath;
            values.PathInfo = "";
            values.RawUrl = Url.PathAndQuery;
        }

        protected WebApi()
        {
        }

        private void ReadHeaders(IRequestValues values)
        {
            values.Headers = new RequestHeaders(Request.Headers);
        }

        private void ReadCookies(IRequestValues values)
        {
            var cookies = new HttpCookieCollection();
            foreach (var cookievalues in Request.Headers.GetCookies())
            {
                foreach (var cookie in cookievalues.Cookies)
                {
                    var c = new HttpCookie(cookie.Name, cookie.Value)
                    {
                        Domain = cookievalues.Domain,
                        HttpOnly = cookievalues.HttpOnly,
                        Path = cookievalues.Path,
                        Secure = cookievalues.Secure,
                    };
                    if (cookievalues.Expires.HasValue)
                    {
                        c.Expires = cookievalues.Expires.Value.DateTime;
                    }

                    cookies.Add(c);
                }
            }
            values.Cookies = cookies;
        }

        private void ReadQueryStrings(IRequestValues values)
        {
            values.QueryString = HttpUtility.ParseQueryString(Url.Query);
        }

        private async Task ReadBody(IRequestValues values)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var bodyparts = await Request.Content.ReadAsMultipartAsync();
                var form = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
                var files = new PostedFileCollection();
                foreach (var content in bodyparts.Contents)
                {
                    var headers = content.Headers;
                    var distion = headers.ContentDisposition;
                    if (headers.ContentType == null)
                    {
                        form.Add(distion.Name.Trim(FileWarpper), await content.ReadAsStringAsync());
                    }
                    else
                    {
                        files.Add(distion.Name.Trim(FileWarpper),
                            new PostedFile()
                            {
                                Name = distion.FileName.Trim(FileWarpper),
                                Type = headers.ContentType.MediaType.Trim(FileWarpper),
                                Length = (int)headers.ContentLength.GetValueOrDefault(-1),
                                Stream = await content.ReadAsStreamAsync()
                            });
                    }
                }
                values.Form = form;
                values.Files = files;
            }
            else
            {
                values.Form = await Request.Content.ReadAsFormDataAsync();
            }
            values.FormBody = await Request.Content.ReadAsByteArrayAsync();
        }

        private void WirteLog()
        {
            if (RealHostAddress != null) Trace.WriteLine(RealHostAddress, "RealIP");
            if (HostAddress != null) Trace.WriteLine(HostAddress, "*IP*");
            if (Url != null) Trace.WriteLine(Url.ToString(), "*Url*");
            Trace.WriteLine(RequestValues.ContentType, "ContentType");
            if (RequestValues.FormBody != null
                && RequestValues.FormBody.Length > 0
                && RequestValues.FormBody.Length < 4096)
            {
                Trace.WriteLine(RequestValues.Form.ToString(), "Form");
            }
            if (UrlReferrer != null) Trace.WriteLine(UrlReferrer.AbsoluteUri, "UrlReferrer");
            if (UserAgent != null) Trace.WriteLine(UserAgent, "UserAgent");
        }
        internal void ResponseEnd(HttpResponseMessage response)
        {
            //全局特殊参数
            if (RequestValues.QueryString["ServerInfo"] != null)
            {
                response.Headers.Add("K-Server-Name", HostName);
                response.Headers.Add("K-Server-IP", HostAddresses);
                response.Headers.Add("Access-Control-Expose-Headers", "K-Server-Name");
                response.Headers.Add("Access-Control-Expose-Headers", "K-Server-IP");
            }
            if (_stopwatch != null)
            {
                _stopwatch.Stop();
                if (RequestValues.QueryString["timing"] != null)
                {
                    response.Headers.Add("K-Timing", $"{Timing} ms");
                    response.Headers.Add("Access-Control-Expose-Headers", "K-Timing");
                }
                if (Timing > ApiConfig.WarningTime)
                {
                    Trace.TraceWarning("API用时过长");
                }
                Trace.WriteLine(Timing + " ms", "WebApi Timing");
            }

            //写入头文件
            foreach (var item in Properties.GetEnumerable("#Header-"))
            {
                var name = item.Key.Remove(0, 8);
                var value = item.Value;
                if (ContentHeaders.Contains(name))
                {
                    response.Content?.Headers?.TryAddWithoutValidation(name, value);
                }
                else
                {
                    response.Headers.TryAddWithoutValidation(name, value);
                }
            }

            //写入Cookie
            var cookeies = Properties.GetEnumerable<CookieHeaderValue>().Select(it => it.Value).ToArray();
            if (cookeies.Length > 0)
            {
                response.Headers.AddCookies(cookeies);
            }

            Trace.WriteLine("Request End", "WebApi Log");

        }
        private string GetUnsafeHeader(string name)
        {
            IEnumerable<string> values;
            return ControllerContext.Request.Headers.TryGetValues(name, out values) ? string.Join(",", values) : null;
        }
        private static string GetID(string value, string type)
        {
            return string.IsNullOrWhiteSpace(value) ? $"{type}-{HostName}-{Guid.NewGuid():n}" : value;
        }
        private HttpResponseMessage ProcessResult(object result)
        {
            if (result == null)
            {
                result = new JsonResult(null);
            }
            var respose = result as HttpResponseMessage;
            if (respose != null)
            {
                return respose;
            }
            if (result is IApiResult == false)
            {
                result = new JsonResult(result);
            }

            //Cache域 ETag
            SetETag(result);

            return ((IApiResult)result).GetResponseMessage(this);
        }
        private void SetETag(object result)
        {
            var etag = GetResponseHeader("ETag");
            if (etag != null)
            {
                var ser = result as ISerializableResult;
                if (ser != null)
                {
                    result = ser.ToApiResult();
                }

                var header = GetResponseHeader("Expires");
                if (header == null || header == "-1")
                {
                    return;
                }
                DateTimeOffset expires;
                DateTimeOffset.TryParse(header, out expires);
                //如果资源已经过期
                if (expires < DateTimeOffset.Now)
                {
                    Cache.Remove(etag);
                    return;
                }
                Cache.Set(etag, result, expires);
            }
        }
        private string GetResponseHeader(string name)
        {
            return Properties["#Header-" + name] as string;
        }
        public override void SetHeader(string name, string value)
        {
            if (value == null)
            {
                Properties.Remove("#Header-" + name);
            }
            else
            {
                Properties["#Header-" + name] = value;
            }
        }
        public override void SetCookie(HttpCookie cookie)
        {
            var ck = new CookieHeaderValue(cookie.Name, cookie.Value ?? "")
            {
                Domain = cookie.Domain,
                HttpOnly = cookie.HttpOnly,
                Path = cookie.Path,
                Secure = cookie.Secure && Url.Scheme == Uri.UriSchemeHttps,
            };
            if (cookie.Value == null)
            {
                ck.MaxAge = TimeSpan.Zero;
                ck.Expires = DateTimeOffset.MinValue;
                Properties["#Cookie-" + Properties.Count] = ck;
                return;
            }

            if (cookie.Expires > DateTime.Now)
            {
                ck.Expires = cookie.Expires;
            }
            Properties["#Cookie-" + cookie.Name] = ck;
        }
        private string GetClientIpAddress()
        {

            var ctx = Request.Properties[HttpContext] as HttpContextWrapper;
            if (ctx != null)
            {
                var ip = ctx.Request.ServerVariables["REMOTE_ADDR"];
                if (string.IsNullOrEmpty(ip))
                    ip = ctx.Request.UserHostAddress;
                return ip;
            }

            dynamic remoteEndpoint = Request.Properties[RemoteEndpointMessage];
            if (remoteEndpoint != null)
            {
                return remoteEndpoint.Address;
            }

            dynamic owinContext = Request.Properties[OwinContext];
            if (owinContext != null)
            {
                return owinContext.Request.RemoteIpAddress;
            }

            return null;
        }
    }
}
