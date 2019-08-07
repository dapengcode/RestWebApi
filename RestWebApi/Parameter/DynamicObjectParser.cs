using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace RestWebApi.Parameter
{
    public class DynamicObjectParser:AbstractBaseParser
    {
        public DynamicObjectParser(string name, Type type) : base(name, type)
        {
        }

        protected override Exception Try(UnvalidatedRequestValuesBase args, out object value)
        {
            dynamic obj = (DynamicObject) CreateNew();
            try
            {
                var names = obj.GetDynamicMemberNames();
                foreach (var name in names)
                {
                    var v = args[name] ?? args[FullName + "." + name];
                    if (v != null)
                    {
                        obj[name] = v;
                    }
                }
                value = obj["result"];
                if(value==null)
                    return new ApiException(ExceptionCode.ParameterMissing,FullName);
                return value as Exception;
            }
            catch (Exception ex)
            {
                value = null;
                return ex;
            }
        }

        protected override Exception Try(string arg, out object value)
        {
            dynamic obj = (DynamicObject)CreateNew();
            try
            {
                obj["this"] = arg;
                value = obj["result"];
                if (value == null)
                {
                    return new ApiException(ExceptionCode.ParameterMissing, FullName);
                }
                return value as Exception;
            }
            catch (Exception ex)
            {
                value = null;
                return ex;
            }
        }
    }
}
