using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using System.Data;
using System.IdentityModel.Claims;

namespace OpFlow.Service.Controllers
{
    public class OpFlowController : ControllerBase
    {
        protected HttpContext _httpContext;
        protected IMemoryCache _memoryCache;
        protected SqlHelper _sqlHelper;

        public OpFlowController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, SqlHelper sqlHelper)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _memoryCache = memoryCache;
            _sqlHelper = sqlHelper;
        }

        internal async Task<UserSecurity> GetUserSecurity(string userAuthId = null)
        {
            if (userAuthId == null)
                userAuthId = _httpContext.User.FindFirst("uid")?.Value;

            _memoryCache.TryGetValue<UserSecurity>(userAuthId, out var userSecurity);

            if (userSecurity != null)
                return userSecurity;

            var userAuthGuid = Guid.Parse(userAuthId);

            var secureUser = await _sqlHelper.GetSecureUser(userAuthGuid, null);
            if (secureUser == null)
                throw new Exception("Unable to locate authenticated user");

            _memoryCache.Set(userAuthId, secureUser, DateTimeOffset.UtcNow.AddHours(1));
            return secureUser;
        }

        internal async Task<Guid> GetUserAuthID()
        {
            var userAuthId = _httpContext.User.FindFirst("uid")?.Value;

            var userAuthGuid = Guid.Parse(userAuthId);

            return userAuthGuid;
        }

        internal void RefreshUserCache()
        {
            var userAuthId = _httpContext.User.FindFirst("uid")?.Value;

            _memoryCache.Remove(userAuthId);
        }

        internal ActionResult CsvResponse(DataTable sourceTable)
        {
            var extract = string.Empty;
            var strDelim = string.Empty;
            for (var index = 0; index < sourceTable.Columns.Count; index++)
            {
                extract += $"{strDelim}{sourceTable.Columns[index].ColumnName}";
                strDelim = ",";
            }

            foreach (DataRow dataRow in sourceTable.Rows)
            {
                extract += "\r\n";
                strDelim = string.Empty;

                for (var index = 0; index < sourceTable.Columns.Count; index++)
                {
                    var cellData = dataRow[index].ToString().Trim().Replace("\"", "\"\"");
                    extract += $"{strDelim}\"{cellData}\"";
                    strDelim = ",";
                }
            }

            return CsvResponse(extract);
        }
        internal ActionResult CsvResponse(string csvData)
        {
            var extractBytes = System.Text.Encoding.Unicode.GetBytes(csvData);
            return File(extractBytes, "application/octet-steam");
        }
    }
}
