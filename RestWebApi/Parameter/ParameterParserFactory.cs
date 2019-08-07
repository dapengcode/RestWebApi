using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.Parameter
{
    public sealed class ParameterParserFactory
    {
        public static IParameterParser Create(ParameterInfo parameter)
        {
            if(parameter==null)
                throw new ArgumentNullException(nameof(parameter));
            return Create(parameter.Name, parameter.ParameterType, parameter.GetCustomAttributes());
        }

        public static IParameterParser Create(string prefix, PropertyInfo property)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentNullException(nameof(prefix));
            }
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            else if (property.GetIndexParameters().Length > 0)
            {
                return null;
            }
            if (prefix.Count(c => c == '.') > 10)
            {
                return null;
            }
            return Create(prefix + "." + property.Name, property.PropertyType, property.GetCustomAttributes());
        }

        private static IParameterParser Create(string name, Type type, IEnumerable<Attribute> attributes)
        {
            IParameterParser parser;
            if(typeof(DynamicObject).IsAssignableFrom(type))
                parser=new DynamicObjectParser(name,type);
            else if(type==typeof(object))
                parser=new DynamicObjectParser(name,type);
            else
            {
                parser = new ParameterParser(name, type);
            }
            parser.Attributes = attributes.ToList().AsReadOnly();
            return parser;
        }
    }
}
