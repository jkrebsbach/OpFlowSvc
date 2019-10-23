using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/trayscheduling")]
    public class TraySchedulingController : ApiController
    {
        /// <summary>
        /// Return search result for receiving provider
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("Nightly", Name = "NightlyRefresh")]
        [HttpPost]
        public async Task<HttpResponseMessage> NightlyRefresh()
        {
            var authorized = AuthenticateUser();
            if (!authorized)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            await UnscheduledCases();

            return Request.CreateResponse(HttpStatusCode.OK, "DONE");
        }

        private async Task UnscheduledCases()
        {
            var tmpInt = 0;
        }

        private bool AuthenticateUser()
        {
            var authorization = HttpContext.Current.Request.Headers["Authorization"];

            if (authorization == null || !authorization.StartsWith("Basic ")) return false;

            try
            {
                var token = authorization.Substring("Basic ".Length).Trim();
                var result = Encoding.UTF8.GetString(Convert.FromBase64String(token));

                var pieces = result.Split(':');
                if (pieces[0] == "scheduler@opflow.com")
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}
