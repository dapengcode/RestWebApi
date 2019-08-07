using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RestWebApi.OAuth
{
    public static class OAuths
    {
        public static Guid GetAccessToken(IWebApi api)
        {
            if (api == null) throw new ArgumentNullException(nameof(api));
            
            var uk = api.RequestValues.Headers["Access-Token"];
            /*if (string.IsNullOrEmpty(uk))
                uk = api.RequestValues.Cookies["uk"]?.Value ?? "";
            if (string.IsNullOrEmpty(uk))
                uk = api.RequestValues.QueryString["uk"];
            if (string.IsNullOrEmpty(uk))
                uk = api.RequestValues.Form["uk"];*/

            var accessToken = Guid.Empty;
            Guid.TryParse(uk, out accessToken);
            return accessToken;
        }
        public static OAuthUser GetOAuthUserByAccessToken(IWebApi api,Guid accessToken)
        {
            if (accessToken == Guid.Empty)
            {
                return null;
            }
            var oauthUser = (OAuthUser)api.Session["LoginedUsers.AccessToken=" + accessToken];
            
            return oauthUser;

        }
    }
}
