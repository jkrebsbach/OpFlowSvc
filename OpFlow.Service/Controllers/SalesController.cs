using System;
using System.Collections.Generic;
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

            byte[] pdfBytes = new byte[0];

            return ResponseHelper.PdfResponse(pdfBytes);
        }
    }
}