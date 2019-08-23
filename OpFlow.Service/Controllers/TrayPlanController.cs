using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/trayplan")]
    public class TrayPlanController : ApiController
    {
        [SwaggerOperation("GetTrayPlan")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayPlan/{trayPlanId}")]
        public async Task<HttpResponseMessage> GetTrayPlan(int? trayPlanId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var reduction = await sqlHelper.GetTrayRationalizationReduction(user.ProviderID, user.LocationID);
            var usage = await sqlHelper.GetTrayRationalizationUsage(trayPlanId, user.ProviderID, user.LocationID);
            var trayPlans = await sqlHelper.GetTrayPlans(user.ProviderID, user.LocationID);
            var instruments = new List<TrayPlanInstrument>();

            if (trayPlanId.HasValue)
                instruments = await sqlHelper.GetTrayPlanInstruments(trayPlanId.Value, user.ProviderID, user.LocationID);

            var result = new
            {
                Reduction = reduction,
                Usage = usage,
                TrayPlans = trayPlans,
                Main = instruments.Where(i => i.ItemType == "M"),
                AddOn = instruments.Where(i => i.ItemType == "A"),
                Single = instruments.Where(i => i.ItemType == "S"),
                Peel = instruments.Where(i => i.ItemType == "P")
            };


            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("PostTrayPlan")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayPlan/{trayPlanId}")]
        public async Task<HttpResponseMessage> PostTrayPlan(int? trayPlanId, [FromBody] TrayPlanPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = await sqlHelper.UpdateTrayPlan(trayPlanId, post.PlanName, post.SpecialtyID, post.Instruments,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}