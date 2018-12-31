using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public async Task<HttpResponseMessage> Post([FromBody]PatientCase newCase)
        {
            var user = await CacheUtil.GetUserSecurity();

            var caseId = await DataAccess.SqlHelper.CreateCase(newCase.PatientID, user.UserID, newCase.SpecialtyID,
                user.ProviderID, user.LocationID, newCase.CaseNbr);

            return Request.CreateResponse(HttpStatusCode.Created, caseId);
        }
    }
}