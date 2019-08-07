using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace RestWebApi.Result
{
    public interface IApiResult
    {
        HttpResponseMessage GetResponseMessage(IWebApi api);
    }
}
