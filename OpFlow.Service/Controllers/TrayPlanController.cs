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
        public async Task<HttpResponseMessage> GetTrayPlan(int? trayPlanId, int? specialtyId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayPlans = await sqlHelper.GetTrayPlans(user.SelectedLocation);

            if (trayPlanId == null && specialtyId == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    SpecialtyID = (int?)null,
                    TrayPlans = trayPlans
                });
            }

            TrayPlanDetail trayPlan = null;
            var instruments = new List<TrayPlanInstrument>();
            var excessTrays = new List<TrayPlanExcessTray>();

            if (trayPlanId.HasValue)
            {
                trayPlan = await sqlHelper.GetTrayPlanDetail(trayPlanId.Value, user.SelectedLocation);
                instruments = await sqlHelper.GetTrayPlanInstruments(trayPlanId.Value, user.SelectedLocation);
                excessTrays = await sqlHelper.GetTrayPlanExcessTrays(trayPlanId.Value, user.SelectedLocation);

                // if no specialty id, assign it to the tray plan specialty id
                specialtyId = (specialtyId ?? trayPlan.SpecialtyID);
            }

            var reduction = await sqlHelper.GetTrayRationalizationReduction(user.SelectedLocation);
            var usage = await sqlHelper.GetTrayRationalizationUsage(trayPlanId, specialtyId, 
               null, null, null, null, user.SelectedLocation);

            var targetTrayNames = instruments.Where(t => t.TargetTrayName != null).GroupBy(t => t.TargetTrayName).Select(t => t.Key).ToList();
            var targetTrays = instruments.OrderBy(t => string.IsNullOrEmpty(t.TargetTrayName)).ThenBy(t => t.TargetTrayName)
                .GroupBy(t => t.TargetTrayName).Select(targetTrayGroup => new TargetTraySummary()
                { TrayName = string.IsNullOrEmpty(targetTrayGroup.Key) ? "No target" : targetTrayGroup.Key,
                  Types = new List<TargetTrayCategory>() {new TargetTrayCategory()
                    { Type = "Main", Instruments = targetTrayGroup.Where(i => i.ItemType == "M").ToList()}, new TargetTrayCategory()
                    { Type = "Add On", Instruments = targetTrayGroup.Where(i => i.ItemType == "A").ToList()}, new TargetTrayCategory()
                    { Type = "Single", Instruments = targetTrayGroup.Where(i => i.ItemType == "S").ToList()}, new TargetTrayCategory()
                    { Type = "Peel", Instruments = targetTrayGroup.Where(i => i.ItemType == "P").ToList()}
                }}).ToList();

            var result = new
            {
                SpecialtyID = specialtyId,
                Reduction = reduction,
                Usage = usage,
                TrayPlans = trayPlans,
                TrayPlan = trayPlan,
                TargetTrayNames = targetTrayNames,
                TargetTrays = targetTrays,
                ExcessTrays = excessTrays,
                Main = instruments.Where(i => i.ItemType == "M"),
                AddOn = instruments.Where(i => i.ItemType == "A"),
                Single = instruments.Where(i => i.ItemType == "S"),
                Peel = instruments.Where(i => i.ItemType == "P")
            };


            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("FilterTrayPlan")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("filter")]
        [HttpPost]
        public async Task<HttpResponseMessage> FilterTrayPlan([FromBody] TrayPlanFilterPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var usage = await sqlHelper.GetTrayRationalizationUsage(post.TrayPlanID, post.SpecialtyID,
                post.InstrumentCategoryID, post.TrayID, post.InstrumentID, post.CardCategoryID,
                user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Usage = usage
            });
        }

        [SwaggerOperation("PostTrayPlan")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayPlan/{trayPlanId}")]
        public async Task<HttpResponseMessage> PostTrayPlan(int? trayPlanId, [FromBody] TrayPlanPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateTrayPlan(trayPlanId, post.PlanName, post.SpecialtyID, post.Instruments, post.ExcessTrays,
                user.SelectedLocation);

            if (!trayPlanId.HasValue) return Request.CreateResponse(HttpStatusCode.OK, result);

            foreach (var detail in post.Details)
            {
                await sqlHelper.UpdateTrayPlanDetail(trayPlanId.Value, detail.Type, detail.Instruments, 
                    user.SelectedLocation);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}