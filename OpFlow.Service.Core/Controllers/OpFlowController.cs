using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using System.Data;

namespace OpFlow.Service.Core.Controllers
{
    public class OpFlowController : ControllerBase
    {
        private HttpContext _httpContext;
        private readonly MemoryCache _memoryCache;
        private SqlHelper _sqlHelper;

        public OpFlowController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, SqlHelper sqlHelper)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _memoryCache = (MemoryCache)memoryCache;
            _sqlHelper = sqlHelper;
        }

        public async Task<UserSecurity> GetUserSecurity(string userAuthId = null)
        {
            if (userAuthId == null)
                userAuthId = _httpContext.User.Identity.Name;

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

        public async Task<Guid> GetUserAuthID()
        {
            var userAuthId = _httpContext.User.Identity.Name;

            var userAuthGuid = Guid.Parse(userAuthId);

            return userAuthGuid;
        }

        public void RefreshUserCache()
        {
            var userAuthId = _httpContext.User.Identity.Name;

            _memoryCache.Remove(userAuthId);
        }

        public ActionResult CsvResponse(DataTable sourceTable)
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
        public ActionResult CsvResponse(string csvData)
        {
            var extractBytes = System.Text.Encoding.Unicode.GetBytes(csvData);
            return File(extractBytes, "application/octet-steam");
        }
    }
}
