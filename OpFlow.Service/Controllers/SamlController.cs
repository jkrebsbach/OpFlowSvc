using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OpFlow.Service.App_Start;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    public class SamlController : ApiController
    {
        /// <summary>
        /// Endpoint to login users via SAML
        /// </summary>
        /// <returns></returns>
        [Route("api/Saml/Login", Name = "SamlLogin")]
        [HttpPost]
        public HttpResponseMessage SamlLogin()
        {
            var loginPath = SamlHelper.Login(HttpContext.Current.Request);

            // now redirect
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(loginPath);
            return response;
        }
        /// <summary>
        /// Endpoint to logout users via SAML
        /// </summary>
        /// <returns></returns>
        [Route("api/Saml/Logout", Name = "SamlLogout")]
        [HttpPost]
        public async Task<HttpResponse> SamlLogout()
        {
            SamlHelper.Logout(HttpContext.Current.Request, HttpContext.Current.Response);

            // find current user, sign them out
            try
            {
                var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                await userManager.UpdateSecurityStampAsync(HttpContext.Current.User.Identity.GetUserId());
            }
            catch (Exception ex)
            {
                throw;
            }

            return HttpContext.Current.Response;
        }
        /// <summary>
        /// Endpoint to excpose SAML artifacts
        /// </summary>
        /// <returns></returns>
        [Route("api/Saml/Artifacts", Name = "SamlArtifacts")]
        [HttpPost]
        public HttpResponse SamlArtifacts()
        {
            SamlHelper.ResolveArtifacts(HttpContext.Current.Request, HttpContext.Current.Response);

            return HttpContext.Current.Response;
        }
        /// <summary>
        /// Endpoint to login users via SAML
        /// </summary>
        /// <returns></returns>
        [Route("api/Saml/Attributes", Name = "SamlAttributes")]
        [HttpPost]
        public HttpResponseMessage SamlAttributes()
        {
            var user = CacheUtil.GetUserSecurity();
            var username = HttpContext.Current.User.Identity.Name;

            var result = DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, username, null);

            return result == null ? Request.CreateResponse(HttpStatusCode.NotFound, "User not found") : Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}