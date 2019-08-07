using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace RestWebApi.Parameter
{
    public class ParameterParser:AbstractBaseParser
    {
        private readonly IFormatterConverter _converter;
        private readonly IParameterParser[] _parsers;
        private readonly PropertyInfo[] _propertyInfos;
        private readonly int _propertyCount;
        public ParameterParser(string name, Type type) : base(name, type)
        {
            _converter=new ParameterConverter();
            _propertyInfos = type.GetProperties().Where(p => p.CanWrite).ToArray();
            _propertyCount = _propertyInfos.Length;
            if (_propertyCount == 0)
            {
                _propertyInfos = null;
                return;
            }
            _parsers=new IParameterParser[_propertyCount];
            for (var i = 0; i < _propertyCount; i++)
            {
                var p = _propertyInfos[i];
                _parsers[i] = ParameterParserFactory.Create(name, p);
            }
        }

        protected override Exception Try(UnvalidatedRequestValuesBase args, out object value)
        {
            if (_propertyCount == 0)
            {
                value = null;
                return new ApiException(ExceptionCode.ParameterMissing,FullName);
            }
            object entity = null;
            for (var i = 0; i < _propertyCount; i++)
            {
                var p = _propertyInfos[i];
                var parser = _parsers[i];
                if (parser == null)
                {
                    value = null;
                    return null;
                }
                object val;
                var error = parser.TryParse(args, out val);
                if (error != null)
                {
                    if (error.HResult == 3002)
                    {
                        continue;
                    }
                    value = null;
                    return error;
                }

                if (entity == null)
                {
                    entity = CreateNew();
                }
                p.SetValue(entity, val);
            }
            if (entity == null)
            {
                value = null;
                return new ApiException(ExceptionCode.ParameterMissing, FullName);
            }
            value = entity;
            return null;
        }

        protected override Exception Try(string arg, out object value)
        {
            try
            {
                value = _converter.Convert(arg, Type);
                return null;
            }
            catch
            {
                value = null;
                if (_propertyCount > 0 && arg.Length == 0)
                {
                    return new ApiException(ExceptionCode.ParameterMissing, FullName);
                }
                return new ApiException(ExceptionCode.ParameterFormatError, FullName, TypeName);
            }
        }
    }
}
