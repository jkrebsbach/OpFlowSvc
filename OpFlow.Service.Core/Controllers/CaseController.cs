using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CaseController : OpFlowController
    {
        public CaseController(IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // POST api/values
        public async Task<ActionResult> Post([FromBody]PatientCase newCase)
        {
            var user = await GetUserSecurity();

            var caseId = await _sqlHelper.CreateCase(newCase.PatientID, user.UserID, newCase.SpecialtyID,
                user.SelectedLocation, newCase.CaseNbr);

            return CreatedAtAction("Post", caseId);
        }
    }
}