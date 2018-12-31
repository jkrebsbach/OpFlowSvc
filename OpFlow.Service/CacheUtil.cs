using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using OpFlow.Data;

namespace OpFlow.Service
{
    public static class CacheUtil
    {
        private static readonly MemoryCache MemCache = MemoryCache.Default;


        public static async Task<UserSecurity> GetUserSecurity()
        {
            var userAuthId = HttpContext.Current.User.Identity.GetUserId();

            if (MemCache.Contains(userAuthId))
                return MemCache[userAuthId] as UserSecurity;

            var userAuthGuid = Guid.Parse(userAuthId);
            var secureUser = await DataAccess.SqlHelper.GetSecureUser(userAuthGuid, null);
            if (secureUser == null)
                throw new Exception("Unable to locate authenticated user");

            MemCache.Add(userAuthId, secureUser, DateTimeOffset.UtcNow.AddHours(1));
            return secureUser;
        }

        public static async Task<UserSecurity> GetUserByEmail(string userName = null)
        {
            if (userName == null)
                userName = HttpContext.Current.User.Identity.GetUserName();

            if (MemCache.Contains(userName))
                return MemCache[userName] as UserSecurity;

            var secureUser = await DataAccess.SqlHelper.GetSecureUser(null, null, userName);
            if (secureUser == null)
                throw new Exception("Unable to locate authenticated user");

            MemCache.Add(userName, secureUser, DateTimeOffset.UtcNow.AddHours(1));
            return secureUser;
        }
    }
}