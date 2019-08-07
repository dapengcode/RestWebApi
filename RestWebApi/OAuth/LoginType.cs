using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.OAuth
{
    /// <summary>
    /// 登录类型
    /// </summary>
    public enum LoginType
    {
        /// <summary>
        /// 未登录
        /// </summary>
        NoLogin = 0,
        /// <summary>
        /// 登录名密码
        /// </summary>
        Password = 1,
        /// <summary>
        /// 登录令牌登陆
        /// </summary>
        LoginToken = 2,
        /// <summary>
        /// 直接选择已登录的登录名
        /// </summary>
        Select = 3,
        /// <summary>
        /// 第三方登录名
        /// </summary>
        ThirdParty = 4,
        /// <summary>
        /// 使用临时密码登录
        /// </summary>
        TempPassword = 5,
    }
}
