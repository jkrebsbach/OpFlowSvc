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
        private string[] _roles = new[] { "CommonConnection", "RexConnection" };

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

            var result = new List<OpFlowProvider>();
            foreach (var role in _roles)
            {
                var sqlHelper = new SqlHelper(role);
                var providers = await sqlHelper.GetOpFlowSetup();

                providers.ForEach(p => p.RoleName = role);
                result.AddRange(providers);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutLocation")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("location")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutLocation(int providerId, int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            foreach (var role in _roles)
            {
                var sqlHelper = new SqlHelper(role);
                var providers = await sqlHelper.GetOpFlowSetup();

                // Try to find the provider in the various databases
                var provider = providers.FirstOrDefault(p => p.ProviderID == providerId);
                if (provider != null)
                {
                    // Remove user from other roles, get user into correct role
                    var userAuthId = HttpContext.Current.User.Identity.GetUserId();
                    var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    var roles = await userManager.GetRolesAsync(userAuthId);

                    bool inNewRole = false;
                    foreach (var currRole in roles)
                    {
                        if (currRole == role)
                        {
                            inNewRole = true;
                        }
                        else
                        {
                            await userManager.RemoveFromRoleAsync(userAuthId, currRole);
                        }
                    }

                    if (!inNewRole)
                        await userManager.AddToRoleAsync(userAuthId, role);

                    // Update user location as needed
                    await sqlHelper.UpdateUserLocation(Guid.Parse(userAuthId), locationId);
                    break;
                }
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
            var newLocationRole = "CommonConnection";
            foreach (var role in _roles)
            {
                var sqlHelper = new SqlHelper(role);
                var providers = await sqlHelper.GetOpFlowSetup();

                // Try to find the provider in the various databases
                var provider = providers.FirstOrDefault(p => p.ProviderName.ToLower() == post.Provider.ToLower());
                if (provider != null)
                {
                    newLocationRole = role;
                    providerId = provider.ProviderID;
                    break;
                }
            }

            var createHelper = new SqlHelper(newLocationRole);

            var locationId = await createHelper.CreateLocation(providerId, post.Provider, post.Location);
            
            return Request.CreateResponse(HttpStatusCode.OK);
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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var roles = await sqlHelper.GetVendors(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roles);
        }
    }
}