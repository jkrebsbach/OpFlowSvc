using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OpFlow.Data;
using OpFlow.Service.App_Start;
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
            
            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var providers = await sqlHelper.GetOpFlowSetup();

            
            return Request.CreateResponse(HttpStatusCode.OK, providers);
        }

        // GET api/values/5
        [SwaggerOperation("PutUserLocation")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("userLocation")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutUserLocation(int providerId, int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var providers = await sqlHelper.GetOpFlowSetup();

            // Try to find the provider in the various databases
            var provider = providers.FirstOrDefault(p => p.ProviderID == providerId);
            if (provider != null)
            {
                // Remove user from other roles, get user into correct role
                var userAuthId = HttpContext.Current.User.Identity.GetUserId();
                var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                
                // Update user location as needed
                await sqlHelper.UpdateUserLocation(Guid.Parse(userAuthId), locationId);
            }

            // Dropped cache user object so system refreshes maped location
            CacheUtil.RefreshUserCache();
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // GET api/values/5
        [SwaggerOperation("PostLocation")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("location")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostLocation([FromBody] LocationPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            int? providerId = null;
            
            var sqlHelper = new SqlHelper();
            var providers = await sqlHelper.GetOpFlowSetup();

            // Try to find the provider in the various databases
            var provider = providers.FirstOrDefault(p => p.ProviderName.ToLower() == post.Provider.ToLower());
            if (provider != null)
            {
                providerId = provider.ProviderID;
            }
            
            var createHelper = new SqlHelper();

            var locationId = await createHelper.CreateLocation(providerId, post.Provider, post.Location);
            
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // GET api/values/5
        [SwaggerOperation("PutLocation")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("location/{locationId}")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutLocation(int locationId, [FromBody] OpFlowLocation post)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            
            var result = await sqlHelper.UpdateLocation(locationId, post.TrayHigh, post.TrayMed, post.TrayLow,
                post.SurgeonHigh, post.SurgeonMed, post.SurgeonLow, post.AuditHigh, post.AuditMed, post.AuditLow, post.DailyTarget);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="nameSearchText">Will filter based on first/last name string match</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<User>))]
        [Route("SearchUsers", Name = "SearchInternalUsers")]
        public async Task<HttpResponseMessage> GetUsers(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var users = await sqlHelper.SearchInternalUsers(nameSearchText, roleId, specialtyId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, users);
        }

        /// <summary>
        /// List roles
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Role>))]
        [Route("Roles", Name = "GetInternalRoles")]
        public async Task<HttpResponseMessage> GetRoles(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var roles = await sqlHelper.GetInternalRoles(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roles);
        }

        /// <summary>
        /// List vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Vendor>))]
        [Route("Vendors", Name = "GetVendors")]
        public async Task<HttpResponseMessage> GetVendors(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var roles = await sqlHelper.GetVendors(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roles);
        }

        /// <summary>
        /// Setup location with base data
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("InitializeLocation/{locationId}", Name = "InitializeLocation")]
        [HttpPost]
        public async Task<HttpResponseMessage> InitializeLocation(int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var roles = await sqlHelper.InitializeLocation(locationId);

            return Request.CreateResponse(HttpStatusCode.OK, roles);
        }
    }
}