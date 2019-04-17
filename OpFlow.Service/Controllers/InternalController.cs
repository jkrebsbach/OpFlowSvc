using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/internal")]
    public class InternalController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("GetSetup")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<OpFlowProvider>))]
        [Route("setup")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetSetup()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = await sqlHelper.GetOpFlowSetup();
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutLocation")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("location")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutLocation(int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var result = await sqlHelper.UpdateUserLocation(user.UserID, locationId);

            CacheUtil.RefreshUserCache();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}