using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using RestWebApi.Filters;

[assembly:System.Web.PreApplicationStartMethod(typeof(RestWebApi.Config.ApiConfig),"Init")]
namespace RestWebApi.Config
{
    public sealed class ApiConfig
    {
        private static  bool _isInit=false;
        public static readonly double WarningTime = 500;
        public static readonly bool IsDebug;
        public static readonly Type SessionType;
        public static readonly Type CacheType;
        internal static readonly List<ApiFilterAttribute> GlobalFilters=new List<ApiFilterAttribute>();
        static ApiConfig()
        {
            var appsettings = System.Configuration.ConfigurationManager.AppSettings;
            var debug = appsettings["DEBUG"];
            IsDebug = string.Equals(debug, "true", StringComparison.OrdinalIgnoreCase);

            double.TryParse(appsettings["WarningTime"], out WarningTime);
            if (WarningTime <= 0) WarningTime = 500;
            if (appsettings["SessionType"] != null)
                SessionType = Type.GetType(appsettings["SessionType"], false);
            if (appsettings["CacheType"] != null)
                CacheType = Type.GetType(appsettings["CacheType"], false);
        }

        public static void Init()
        {
            if (_isInit) return;
            _isInit = true;
            var routes = GlobalConfiguration.Configuration.Routes;

            routes.MapHttpRoute(
                    name: Guid.NewGuid().ToString(),
                    routeTemplate: "api/{controller}/{action}",
                    defaults: new { action = "???", }
            );

            routes.MapHttpRoute(
                    name: Guid.NewGuid().ToString(),
                    routeTemplate: "api/{controller}/v{action_version}/{action}",
                    defaults: new { action = "???" },
                    constraints: new { action_version = @"\d+" }
            );
            //AddFilter(new LoginAttribute());//默认必须登录
            AddFilter(new NoSignAttribute());//默认无需签名
            AddFilter(new IdempotentAttribute(false,1));//默认1秒幂等

        }

        public static void AddFilter(ApiFilterAttribute filter)
        {
            if (filter == null) return;
            if (!filter.AllowMultiple)
            {
                for (int i = 0; i < GlobalFilters.Count; i++)
                {
                    if (GlobalFilters[i].Math(filter))
                    {
                        GlobalFilters[i] = filter;
                        return;
                    }
                }
            }
            GlobalFilters.Add(filter);
        }
    }
}
