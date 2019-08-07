using System;
using System.Collections.Generic;
using System.Web;


namespace RestWebApi.Parameter
{
    public abstract class AbstractBaseParser : IParameterParser
    {
        private object _defaultValue;

        protected AbstractBaseParser(string name, Type type)
        {
            FullName = name;
            Name = name;
            var index = name.LastIndexOf('.') + 1;
            if (index > 0)
                Name = name.Substring(index, name.Length - index);
            Type = type;
            TypeName = type.FullName;
        }


        public string Name { get; }
        public string FullName { get; }
        public Type Type { get; }
        public string TypeName { get; }
        public bool HasDefaultValue { get; private set; }

        public object DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                HasDefaultValue = true;
                _defaultValue = value;
            }
        }

        public IList<Attribute> Attributes { get; set; }

        public Exception TryParse(UnvalidatedRequestValuesBase args, out object value)
        {
            Exception error;
            var str = args[FullName] ?? args[Name];
            if (str != null)
            {
                error = Try(str, out value);
                if (error != null)
                {
                    if (error.HResult != (int) ExceptionCode.ParameterMissing)
                        return error;
                }
                else if (value != null)
                {
                    TrimValue(ref value);
                    return null;
                }
                else
                {
                    return null;
                }
            }
            error = Try(args, out value);
            if (error == null)
            {
                TrimValue(ref value);
                return null;
            }
            if (error.HResult == (int) ExceptionCode.ParameterMissing)
            {
                if (!HasDefaultValue) return error;
                value = DefaultValue;
                TrimValue(ref value);
                return null;
            }
            return error;
        }

        protected object CreateNew()
        {
            return Activator.CreateInstance(Type);
        }

        protected abstract Exception Try(UnvalidatedRequestValuesBase args, out object value);
        protected abstract Exception Try(string arg, out object value);

        private static void TrimValue(ref object value)
        {
            var str = value as string;
            if (str != null)
                value = str.Trim();
        }
    }
}