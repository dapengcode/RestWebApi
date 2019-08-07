using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;


namespace RestWebApi.Context
{
    public sealed class ApiContext:Container
    {
        public static ApiContext Current => (ApiContext) ApiContextManager.Container;
        private readonly WebApi _api;

        internal ApiContext(WebApi api)
        {
            _api = api;
            ApiContextManager.SetContainer(this);
        }

        public IWebApi WebApi => _api;

        
        /// <summary>
        /// 销毁上下文
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ApiContextManager.Dispose();
                Trace.Flush();
            }
        }
    }
}
