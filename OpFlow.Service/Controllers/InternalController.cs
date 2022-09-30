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
            var masterSpecialties = await sqlHelper.GetSpecialtyMaster();
            
            return Request.CreateResponse(HttpStatusCode.OK, new {
                Providers =  providers,
                MasterSpecialties = masterSpecialties
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetTraysPagedInternal")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PaginationController))]
        [Route("traysPagedInternal")]
        public async Task<HttpResponseMessage> GetTraysPagedInternal(string additionalData = null, string term = null, int page = 1)
        {
            var user = await CacheUtil.GetUserSecurity();
            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var instruments = await sqlHelper.GetTraysPaged(additionalData, term, page, null);

            return Request.CreateResponse(HttpStatusCode.OK, instruments);
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
        public async Task<HttpResponseMessage> GetVendors()
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var roles = await sqlHelper.GetVendors(user.LocationID);

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

        [SwaggerOperation("AddCommunicationComments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("communicationComments")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddCommunicationComments(int locationId, int historyId, [FromBody] TrayCommunicationHistoryUpdatePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sentDate = DateTime.Today;
            if (DateTime.TryParse(post.SentDate, out var tmpSentDate))
                sentDate = tmpSentDate;

            var communication = await sqlHelper.UpdateProposedTrayCommunicationHistory(historyId, sentDate, post.Comments,
                locationId);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("CommunicationScreen", Name = "GetCommunicationScreen")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCommunicationScreen(int? locationId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            if (locationId == null)
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Phases = new List<string>()
                });

            var phases = await sqlHelper.GetTrayProposalPhases(locationId.Value);
            var specialties = await sqlHelper.GetSpecialties(locationId.Value);
            var proposedTrays = await sqlHelper.GetProposedTrays(null, locationId.Value);
            var users = await sqlHelper.SearchUsers(null, null, null, locationId.Value);
            var communicationMethods = await sqlHelper.GetTrayCommunicationMethods(locationId.Value);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Phases = phases,
                Specialties = specialties,
                Proposals = proposedTrays,
                Users = users,
                CommunicationMethods = communicationMethods,
            });
        }

        [SwaggerOperation("GetCommunicationHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InternalTrayProposalHistory>))]
        [Route("communication")]
        [HttpPost]
        public async Task<HttpResponseMessage> GetCommunicationHistory([FromBody] InternalTrayCommunicationHistoryPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var communication = await sqlHelper.GetInternalProposedTrayCommunicationHistory(
                post.LocationID, post.PhaseID,
                post.TrayProposalIds, post.SpecialtyIds, post.UserIds);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Communication = communication
            });
        }

        [SwaggerOperation("InsertCommunicationHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InternalTrayProposalHistory>))]
        [Route("communicationHistory")]
        [HttpPost]
        public async Task<HttpResponseMessage> InsertCommunicationHistory([FromBody] InternalTrayCommunicationInsertPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var sentDate = DateTime.Today;
            if (DateTime.TryParse(post.SentDate, out var tmpSentDate))
                sentDate = tmpSentDate;

            var communication = await sqlHelper.InsertProposedTrayCommunicationHistory(
                post.Phase, post.Activity, post.Tray, sentDate, post.Audience, post.Method, post.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Communication = communication
            });
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("Specialty/{specialtyId}", Name = "UpdateSpecialtyMaster")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateSpecialtyMaster(int specialtyId, [FromBody] SpecialtyMasterPost request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var result = await sqlHelper.UpdateSpecialtyMaster(specialtyId, request.SpecialtyName);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("Specialty", Name = "CreateSpecialtyMaster")]
        [HttpPost]
        public async Task<HttpResponseMessage> CreateSpecialtyMaster([FromBody] SpecialtyMasterPost request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var specialtyId = await sqlHelper.InsertSpecialtyMaster(request.SpecialtyName);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("Specialty/{specialtyId}", Name = "DeleteSpecialtyMaster")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteSpecialtyMaster(int specialtyId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var result = await sqlHelper.DeleteSpecialtyMaster(specialtyId);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }
    }
}