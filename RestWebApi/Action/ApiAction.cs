using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestWebApi.Config;
using RestWebApi.Filters;
using RestWebApi.Parameter;


namespace RestWebApi.Action
{
    public sealed class ApiAction
    {
        private readonly IList<ApiFilterAttribute> _filters=new List<ApiFilterAttribute>();
        public ObsoleteAttribute Obsolete { get; private set; }
        public bool Abandoned { get; private set; }
        public MethodInfo Method { get; private set; }
        public bool MustSign { get; private set; }
        public IList<ApiFilterAttribute> Filters => _filters;
        public string Name { get; internal set; }
        public int ApiVersion { get; internal set; }
        private readonly IParameterParser[] _parameterParsers;

        public IReadOnlyList<IParameterParser> ParameterParsers => new ReadOnlyCollection<IParameterParser>(_parameterParsers);
        private readonly int _parameterLength;
        public ApiAction(MethodInfo method)
        {
            if(method==null)
                throw new ArgumentNullException(nameof(method));
            if(!method.IsPublic)
                throw new ArgumentOutOfRangeException(nameof(method),"方法必须为public类型");
            if(method.IsGenericMethod)
                throw new ArgumentException($"{method.Name}不能为泛型方法",nameof(method));
            Method = method;
            Name = method.Name;
            Obsolete = method.GetCustomAttribute<ObsoleteAttribute>() ??
                       method.ReflectedType.GetCustomAttribute<ObsoleteAttribute>() ??
                       method.DeclaringType.GetCustomAttribute<ObsoleteAttribute>();
            var paras = method.GetParameters();
            var parasLength = paras.Length;
            var parsers=new IParameterParser[parasLength];
            for (var i = 0; i < parasLength; i++)
            {
                var p = paras[i];
                if(p.IsOut||p.ParameterType.IsByRef)
                    throw new ArgumentException("不支持out或ref参数");
                parsers[i] = ParameterParserFactory.Create(p);
                if (p.Attributes.HasFlag(ParameterAttributes.HasDefault)
                    || p.HasDefaultValue)
                {
                        parsers[i].DefaultValue = p.RawDefaultValue;
                }
            }
            _parameterParsers = parsers;

            _parameterLength = _parameterParsers.Length;


        }

        internal void InitFilters()
        {
            if (!typeof(IIgnoreGlobalApiFilter).IsAssignableFrom(Method.DeclaringType))
            {
                foreach (var filter in ApiConfig.GlobalFilters)
                {
                    var clone = (ApiFilterAttribute)filter.Clone();
                    clone.Init(this);
                    _filters.Add(clone);
                }
            }
            if (Method.ReflectedType == null) return;
            {
                var classFilter = Attribute.GetCustomAttributes(Method.ReflectedType, typeof(ApiFilterAttribute));
                var methodFilter = Attribute.GetCustomAttributes(Method, typeof(ApiFilterAttribute));
                var filters = classFilter.Concat(methodFilter);
                foreach (var filter in filters)
                {
                    var clone = (ApiFilterAttribute)(((ApiFilterAttribute)filter).Clone());
                    clone.Init(this);
                    _filters.Add(clone);
                }
            }
        }
        /// <summary>
        /// 添加过滤器
        /// </summary>
        /// <param name="filter"></param>
        private void AddFilter(ApiFilterAttribute filter)
        {
            if (filter.AllowMultiple == false)
            {
                for (var i = 0; i < _filters.Count; i++)
                {
                    if (!_filters[i].Match(filter)) continue;
                    _filters[i] = filter;
                    MustSign = HasFilter<SignAttribute>();
                    return;
                }
            }
            _filters.Add(filter);
            MustSign = HasFilter<SignAttribute>();
        }
        /// <summary>
        /// 判断是否包含指定类型的过滤器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasFilter<T>()
        {
            for (int i = 0, length = _filters.Count; i < length; i++)
            {
                if (_filters[i] is T)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 执行Action之前触发
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public async Task<object> OnActionExecuting(IWebApi api)
        {
            var length = _filters.Count;
            for (var i = 0; i < length; i++)
            {
                var f = _filters[i];
                var r = await TryRunTask(f.OnActionExecuting(api));
                if (r != null)
                    return r;
            }
            return null;
        }

        
        public object[] GetArguments(UnvalidatedRequestValuesBase values, out Exception error)
        {
            if (_parameterLength == 0)
            {
                error = null;
                return new object[0];
            }
            var args=new object[_parameterLength];
            for (var i = 0; i < _parameterLength; i++)
            {
                var p = _parameterParsers[i];
                object obj;
                error = p.TryParse(values, out obj);
                if (error != null)
                {
                    return null;
                }
                args[i] = obj;
            }
            error = null;
            return args;
        }

        /// <summary>
        /// 执行Action之后触发
        /// </summary>
        /// <param name="api"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task<object> OnActionExecuted(IWebApi api, object result)
        {
            var length = _filters.Count;
            for (var i = 0; i < length; i++)
            {
                var f = _filters[i];
                var r = await TryRunTask(f.OnActionExecuted(api,result));
                if (r != null)
                    return r;
            }
            return null;
        }

        public object Execute(IWebApi api, object[] args)
        {
            return Method.Invoke(api, args);
        }

        public static async Task<object> TryRunTask(object result)
        {
            while (true)
            {
                var task = result as Task;
                if (task == null) return result;
                await Task.WhenAny(task);
                var t = task.GetType();
                if (!t.IsGenericType) return null;
                result = t.GetProperty("Result").GetValue(task);
                if (result is Task)
                {
                    continue;
                }
                return result;
            }
        }
    }
}
