using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data.Sales;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/sales")]
    public class SalesController : ApiController
    {
        [SwaggerOperation("ExportPDF")]
        [Route("pdf")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<KeyValuePair<string, string>>))]
        public async Task<HttpResponseMessage> ExportPDF([FromBody] SalesModel post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var dataSet = await sqlHelper.GetAnalyticsSalesToolSummary(post.SystemName, post.HospitalName, post.City,
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

            return ResponseHelper.PdfResponse(pdfBytes);
        }
    }
}