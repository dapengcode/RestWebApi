using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace RestWebApi.Action
{
    public static class ApiActionFactory
    {
        private static readonly ConcurrentDictionary<MethodInfo, ApiAction> Actions = new ConcurrentDictionary<MethodInfo, ApiAction>();
        public static ApiAction Get(IWebApi api, string methodName, string version)
        {
            if(string.IsNullOrEmpty(methodName))
                throw new ArgumentException(nameof(methodName));
            var invokeMethodName = methodName;
            var apiver = 0;
            if (version == null)
            {
                var index = methodName.LastIndexOf('V');
                if (index > 0)
                {
                    version = methodName.Remove(0, index + 1);
                    if (int.TryParse(version, out apiver) && apiver >= 0)
                        methodName = methodName.Remove(index);
                }
            }else if (int.TryParse(version, out apiver) && apiver >= 0)
                invokeMethodName = methodName + "V" + version;
            else
            {
                return null;
            }
            try
            {
                var method = api.GetType().GetMethod(invokeMethodName,
                    BindingFlags.Public | BindingFlags.Instance
                    | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly |
                    BindingFlags.InvokeMethod);
                if (method == null || method.Attributes.HasFlag(MethodAttributes.Abstract)
                    || method.Attributes.HasFlag(MethodAttributes.SpecialName))
                {
                    return null;
                }
                ApiAction action;
                if (Actions.TryGetValue(method, out action) == false)
                {
                    action = new ApiAction(method)
                    {
                        Name = methodName,
                        ApiVersion = apiver
                    };
                    action.InitFilters();

                    Actions.TryAdd(method, action);
                }
                return action;
            }
            catch (AmbiguousMatchException e)
            {
                Trace.WriteLine(e.Message);
                return null;
            }
        }
        
    }
}
