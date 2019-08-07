using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RestWebApi.Parameter
{
    public class ParameterConverter:FormatterConverter,IFormatterConverter
    {
        public static object Convert(object value, Type type, bool throwError)
        {
            if(type==null)
                throw new ArgumentNullException(nameof(type));
            if (type == typeof(string))
                return value.ToString();
            var str = value as string;
            if (str == null)
            {
                if (value == null)
                    return null;
                try
                {
                    return System.Convert.ChangeType(value, type);
                }
                catch
                {
                    if(throwError)
                    throw;
                    return null;
                }
            }
            if (type == typeof(Guid))
            {
                Guid g;
                if (Guid.TryParse(str, out g))
                    return g;
            }else if (type == typeof(Uri))
            {
                Uri u;
                if (Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out u))
                    return u;
            }
            else if (type==typeof(TimeSpan))
            {
                TimeSpan t;
                if (TimeSpan.TryParse(str, out t))
                    return t;
            }
            else if (type == typeof(Type))
            {
                return Type.GetType(str, false, true);
            }
            else if (type.IsPrimitive|| type == typeof(DateTime))
            {
                return System.Convert.ChangeType(value, type);
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject(value.ToString(), type, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
                }
                catch
                {
                    if (throwError)
                        throw;
                    return null;
                }
            }
            if(throwError)
                throw new InvalidCastException($"字符串:{str}转为类型:{type.FullName}失败");
            return null;
        }

        object IFormatterConverter.Convert(object value, Type type)
        {
            return Convert(value, type, true);
        }
    }
}
