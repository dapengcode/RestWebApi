using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using RestWebApi.Action;


namespace RestWebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,Inherited = true,AllowMultiple = true)]
    public abstract class ApiFilterAttribute:DescriptionAttribute,ICloneable
    {
        public bool AllowMultiple { get; set; }

        public bool Math(object filter)
        {
            return filter != null && filter.GetType() == this.GetType();
        }

        protected ApiFilterAttribute()
        {
            
        }

        public virtual void Init(ApiAction action)
        {

        }
        /// <summary>
        /// 在调用操作方法之前发生
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public virtual object OnActionExecuting(IWebApi api)
        {
            return null;
        }
        /// <summary>
        /// 在调用操作方法之后发生
        /// </summary>
        /// <param name="api"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual object OnActionExecuted(IWebApi api, object result)
        {
            return null;
        }
        

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
