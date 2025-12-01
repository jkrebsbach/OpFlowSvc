using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProcedureController : OpFlowController
    {
        public ProcedureController(IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        public async Task<ActionResult> GetProcedures(int? specialtyId = null)
        {
            var user = await GetUserSecurity();
            

            var procedures = await _sqlHelper.GetProcedures(specialtyId, user.SelectedLocation);

            return procedures == null ?
                NotFound() :
                Ok(procedures);
        }

    }
}