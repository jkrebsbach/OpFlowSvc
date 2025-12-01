using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/trayplan")]
    public class TrayPlanController : OpFlowController
    {
        public TrayPlanController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        [Route("trayPlan/{trayPlanId}")]
        public async Task<ActionResult> GetTrayPlan(int? trayPlanId)
        {
            var user = await GetUserSecurity();
            

            var trayPlans = await _sqlHelper.GetTrayPlans(user.SelectedLocation);

            if (trayPlanId == null)
            {
                var locationInstruments = await _sqlHelper.GetItems("instrument", null, true, user.SelectedLocation);
                var cptCodes = await _sqlHelper.GetCptCodes();

                return Ok(new
                {
                    SpecialtyID = (int?)null,
                    TrayPlans = trayPlans,
                    CptCodes = cptCodes,
                    Instruments = locationInstruments
                });
            }

            TrayPlanDetail trayPlan = null;
            var instruments = new List<TrayPlanInstrument>();
            var excessTrays = new List<TrayPlanExcessTray>();

            if (trayPlanId.HasValue)
            {
                trayPlan = await _sqlHelper.GetTrayPlanDetail(trayPlanId.Value, user.SelectedLocation);
                instruments = await _sqlHelper.GetTrayPlanInstruments(trayPlanId.Value, user.SelectedLocation);
                excessTrays = await _sqlHelper.GetTrayPlanExcessTrays(trayPlanId.Value, user.SelectedLocation);
            }

            var reduction = await _sqlHelper.GetTrayRationalizationReduction(user.SelectedLocation);
            
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
                Reduction = reduction,
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


            return Ok(result);
        }

        [Route("filter")]
        [HttpPost]
        public async Task<ActionResult> FilterTrayPlan([FromBody] TrayPlanFilterPost post)
        {
            var user = await GetUserSecurity();
            

            var usage = await _sqlHelper.GetTrayRationalizationUsage(post.TrayPlanID, post.SpecialtyID,
                post.CardCategoryID, post.ProcProfileID, post.CptID,
                post.TrayID, post.InstrumentCategoryID, post.InstrumentTypeID, post.InstrumentID, 
                user.SelectedLocation);

            return Ok(new
            {
                Usage = usage
            });
        }

        [Route("trayPlan/{trayPlanId}")]
        public async Task<ActionResult> PostTrayPlan(int? trayPlanId, [FromBody] TrayPlanPost post)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateTrayPlan(trayPlanId, post.PlanName, post.SpecialtyID, post.Instruments, post.ExcessTrays,
                user.SelectedLocation);

            if (!trayPlanId.HasValue) return Ok(result);

            foreach (var detail in post.Details)
            {
                await _sqlHelper.UpdateTrayPlanDetail(trayPlanId.Value, detail.Type, detail.Instruments, 
                    user.SelectedLocation);
            }

            return Ok(result);
        }
    }
}