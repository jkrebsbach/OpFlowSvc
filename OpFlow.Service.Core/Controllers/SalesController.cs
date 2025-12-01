using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Data.Sales;
using OpFlow.Service.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/sales")]
    public class SalesController : OpFlowController
    {
        public SalesController(IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        [Route("pdf")]
        [HttpPut]
        public async Task<ActionResult> ExportPDF([FromBody] SalesModel post)
        {
            var user = await GetUserSecurity();
            

            var dataSet = await _sqlHelper.GetAnalyticsSalesToolSummary(post.SystemName, post.HospitalName, post.City,
                post.State,
                post.ContactName, post.Salesperson, post.CaseCount, post.SpdLaborRate, post.ContractDuration,
                post.AnnualMaintenance, post.Depreciation, post.TrayCount, post.InstrumentAvg,
                post.TrayYear, post.VendorYear, post.CardYear, post.ImproveYear);

            var datasets = new Dictionary<string, DataTable>
            {
                ["SalesReport"] = dataSet.Tables[0],
                ["Subscription"] = dataSet.Tables[1],
                ["ValueSavings"] = dataSet.Tables[2]
            };

            var pdfBytes = ReportHelper.GetReport("SalesReport", "PDF", datasets);

            return File(pdfBytes, "application/octet-stream");
        }
    }
}