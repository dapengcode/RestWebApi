using System;
using System.Collections.Generic;
using System.Web;

namespace RestWebApi.Parameter
{
    public interface IParameterParser
    {
        string Name { get; }
        string FullName { get; }
        Type Type { get; }
        string TypeName { get; }
        
        bool HasDefaultValue { get; }
        object DefaultValue { get; set; }
        IList<Attribute> Attributes { get;set; }
        Exception TryParse(UnvalidatedRequestValuesBase args, out object value);
    }
}