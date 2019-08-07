using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;


namespace RestWebApi.OAuth
{
    public class OAuthUser:IPrincipal,IIdentity
    {
        internal void Init(OAuthUser user)
        {
            if (user == null) return;
            Sid = user.Sid;
            Uid = user.Uid;
            AccessToken = user.AccessToken;
            NickName = user.NickName;
            Avatar = user.Avatar;
            Name = user.Name;
            LoginType = user.LoginType;
        }
        
        /// <summary>
        /// 用户Session标识
        /// </summary>
        public string Sid { get; set; }
        /// <summary>
        /// 所属店铺ID
        /// </summary>
        public int StoreId { get; set; }
        /// <summary>
        /// 用户标识
        /// </summary>
        public int Uid { get; set; }
        /// <summary>
        /// 访问令牌
        /// </summary>
        public Guid AccessToken { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }
        /// <summary> 当前用户的登录验证模式
        /// </summary>
        [ScriptIgnore]
        public virtual LoginType LoginType { get; set; }
        /// <summary>
        /// 是否授权成功
        /// </summary>
        [ScriptIgnore]
        public  bool IsAuthenticated => AccessToken != Guid.Empty&& LoginType > 0;
        
        /// <summary>
        /// 账号
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 认证方式
        /// </summary>
        public string AuthenticationType {
            get { return LoginType.ToString(); }
            set { LoginType = (LoginType) Enum.Parse(typeof(LoginType), value); }
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        IIdentity IPrincipal.Identity => this;
        /// <summary>
        /// 角色验证
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        bool IPrincipal.IsInRole(string role)
        {
            return true;
        }
    }
}
