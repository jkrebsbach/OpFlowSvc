using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http;
using OpFlow.Data;

namespace OpFlow.Service.Controllers
{
    public class OpFlowControllerBase : ApiController
    {
        private static readonly MemoryCache MemCache = MemoryCache.Default;


        internal static UserSecurity GetUserSecurity()
        {
            var username = HttpContext.Current.User.Identity.Name;

            if (MemCache.Contains(username))
                return MemCache[username] as UserSecurity;

            var secureUser = DataAccess.SqlHelper.GetSecureUser(username);

            MemCache.Add(username, secureUser, DateTimeOffset.UtcNow.AddHours(1));
            return secureUser;
        }
    }
}