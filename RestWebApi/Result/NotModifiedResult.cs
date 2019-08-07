using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace RestWebApi.Result
{
    public class NotModifiedResult:IApiResult
    {
        public static readonly NotModifiedResult Instance=new NotModifiedResult();

        
        private NotModifiedResult()
        {
        }

        public HttpResponseMessage GetResponseMessage(IWebApi api)
        {
            var result=new HttpResponseMessage(HttpStatusCode.NotModified);
            return result;
        }
    }
}
