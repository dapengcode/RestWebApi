using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.Filters
{
    public sealed class SignAttribute:ApiFilterAttribute
    {
        public override bool Match(object obj)
        {
            return obj is SignAttribute || obj is NoSignAttribute;
        }

        public override string Description =>"必须签名";
    }
}
