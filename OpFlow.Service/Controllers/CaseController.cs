using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class CaseController : ApiController
    {
        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<IHttpActionResult> Post([FromBody]PatientCase newCase)
        {
            DataAccess.SqlHelper.CreateCase(newCase);

            return Ok();
        }
    }
}