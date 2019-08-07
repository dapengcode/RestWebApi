using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.Filters
{
    public class IdempotentAttribute : ApiFilterAttribute
    {
        private bool _ignoreUser;
        private string[] _paramNames;
        private int _seconds;
        /// <summary>
        /// 幂等过滤器
        /// </summary>
        /// <param name="ignoreUser">是否忽略用户差异</param>
        /// <param name="seconds">缓存秒数</param>
        /// <param name="paramNames">缓存参数</param>
        public IdempotentAttribute(bool ignoreUser, int seconds, params string[] paramNames)
        {
            _ignoreUser = ignoreUser;
            _paramNames = paramNames == null || paramNames.Length == 0 ? null : paramNames;
            _seconds = seconds;
            if (_seconds < 0)
                _seconds = 10;
        }

        public IdempotentAttribute(bool ignoreUser, params string[] paramNames)
        : this(ignoreUser, 60, paramNames)
        {

        }

        public IdempotentAttribute(params string[] paramNames) : this(false, 60, paramNames)
        {

        }

        public IdempotentAttribute() : this(false, 60)
        {

        }

        public override object OnActionExecuting(IWebApi api)
        {
            if (api.RequestValues["nocache"] != null)
            {
                api.SetHeader("Cache-Control", "no-cache");
                return null;
            }
            var _key = GetIdempotentKey(api);
            var value = api.Cache.AddOrGetExisting(_key, "toomanyrequest", DateTimeOffset.Now.AddSeconds(2));
            var now = DateTime.UtcNow;
            if (value != null)
            {
                if (value.ToString().Equals("toomanyrequest"))
                {
                    api.SetHeader("Cache-Control","no-cache");
                    return new ApiException(ExceptionCode.TooManyRequests);
                }
                DateTime ifModifiedSince;
                DateTime.TryParse(api.RequestValues.Headers["If-Modified-Since"], out ifModifiedSince);
                var expire = ifModifiedSince.AddSeconds(_seconds);
                var sec = _seconds;
                if (expire > now)
                {
                    sec = (int) (expire - now).TotalSeconds;
                }
                api.SetHeader("Cache-Control","max-age="+sec);
                api.SetHeader("Expires","-1");
                api.SetHeader("ETag",_key);
                return value;
            }
            api.OnError += Api_OnError;
            api.SetHeader("Cache-Control","max-age="+_seconds);
            api.SetHeader("Expires",now.AddSeconds(_seconds).ToString("r"));
            api.SetHeader("ETag",_key);
            return null;

        }

        private void Api_OnError(object sender, EventArgs e)
        {
            var api = (IWebApi) sender;
            if (api == null) return;
            api.SetHeader("ETag",null);
            api.SetHeader("Expires",null);
            api.SetHeader("Cache-Control",null);
        }

        private string GetIdempotentKey(IWebApi api)
        {
            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                var str = string.Join("\n", GetValues(api));
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = md5Provider.ComputeHash(bytes);
                return new Guid(hash).ToString("n");
            }
        }

        /// <summary>
        /// 枚举所有需要参加MD5加密的值
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        private IEnumerable<string> GetValues(IWebApi api)
        {
            yield return api.Url.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path | UriComponents.Query,
                UriFormat.Unescaped);
            yield return api.Agent.ToString();
            if (_ignoreUser == false && api.OAuthUser != null && api.OAuthUser.AccessToken != Guid.Empty)
                yield return api.OAuthUser.AccessToken.ToString();
            if (_paramNames == null)
                yield return Convert.ToBase64String(api.RequestValues.FormBody);
            else
            {
                foreach (var paramName in _paramNames)
                {
                    yield return api.RequestValues[paramName];
                }
            }
        }

        public override bool Match(object obj)
        {
            return obj is IdempotentAttribute || obj is NoIdempotentAttribute;
        }

        public override string Description
        {
            get
            {
                var buffer = new StringBuilder(20);
                buffer.Append($"幂等缓存{_seconds}秒");
                buffer.Append(_ignoreUser ? ",关联用户" : ",全局");
                buffer.Append(_paramNames == null ? "" : ",幂等参数(" + string.Join(",", _paramNames) + ")");
                return buffer.ToString();
            }
        }
    }
}
