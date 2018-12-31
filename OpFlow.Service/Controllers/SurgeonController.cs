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
    public class SurgeonController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgeon>))]
        public async Task<HttpResponseMessage> GetSurgeons(int? specialtyId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var surgeons = await DataAccess.SqlHelper.GetSurgeons(specialtyId, user.ProviderID, user.LocationID);

            return surgeons == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, surgeons);
        }
    }
}