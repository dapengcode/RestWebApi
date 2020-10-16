using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RestWebApi.Filters;

namespace RestWebApi.Demo.Controllers
{
    public class HomeController : WebApi
    {
        
        public string UserName(string id)
        {
            return id;
        }
    }
}
