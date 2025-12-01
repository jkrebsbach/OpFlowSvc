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
    public class BundleController : OpFlowController
    {
        public BundleController(IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        public async Task<ActionResult> GetBundles(int? specialtyId = null)
        {
            var user = await GetUserSecurity();
            

            var bundles = await _sqlHelper.GetBundles(specialtyId, user.SelectedLocation);

            return Ok(bundles);
        }

        // GET api/values/5
        [Route("api/bundle/procedures")]
        public async Task<ActionResult> GetBundleProcedures(int bundleId)
        {
            var user = await GetUserSecurity();

            var procedures = await _sqlHelper.GetBundleProcedures(bundleId, user.SelectedLocation);

            return Ok(procedures);
        }

        // POST api/values
        public async Task<ActionResult> Post([FromBody]BundlePost value)
        {
            var user = await GetUserSecurity();

            var result = await _sqlHelper.NewBundle(value.BundleDescription, value.SpecialtyID, value.Procedures, user.SelectedLocation);

            return Ok(result);
        }

        // PUT api/values/5
        public async Task<ActionResult> Put(int id, [FromBody]BundlePost value)
        {
            var user = await GetUserSecurity();

            var result = await _sqlHelper.UpdateBundle(id, value.BundleDescription, value.SpecialtyID, value.Procedures, user.SelectedLocation);

            return Ok();
        }

        // DELETE api/values/5
        public async Task<ActionResult> Delete(int id)
        {
            var user = await GetUserSecurity();

            var result = await _sqlHelper.DeleteBundle(id, user.ProviderID, user.LocationID);

            return Ok();
        }
    }
}