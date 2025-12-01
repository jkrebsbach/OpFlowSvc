using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Service.DataAccess;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/internal")]
    public class InternalController : OpFlowController
    {
        public InternalController(IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        [Route("setup")]
        [HttpGet]
        public async Task<ActionResult> GetSetup()
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            
            var providers = await _sqlHelper.GetOpFlowSetup();
            var masterSpecialties = await _sqlHelper.GetSpecialtyMaster();
            
            return Ok(new {
                Providers =  providers,
                MasterSpecialties = masterSpecialties
            });
        }

        // GET api/values/5
        [Route("traysPagedInternal")]
        public async Task<ActionResult> GetTraysPagedInternal(string additionalData = null, string term = null, int page = 1)
        {
            var user = await GetUserSecurity();
            if (user.RoleType != "Internal")
                return NotFound();

            

            var instruments = await _sqlHelper.GetTraysPaged(additionalData, term, page, null);

            return Ok(instruments);
        }

        // GET api/values/5
        [Route("userLocation")]
        [HttpPut]
        public async Task<ActionResult> PutUserLocation(int providerId, int locationId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            
            var providers = await _sqlHelper.GetOpFlowSetup();

            // Try to find the provider in the various databases
            var provider = providers.FirstOrDefault(p => p.ProviderID == providerId);
            if (provider != null)
            {
                // Remove user from other roles, get user into correct role
                var userAuthId = _httpContext.User.FindFirst("uid")?.Value;

                // Update user location as needed
                await _sqlHelper.UpdateUserLocation(Guid.Parse(userAuthId), locationId);
            }

            // Dropped cache user object so system refreshes maped location
            RefreshUserCache();
            return Ok();
        }

        // GET api/values/5
        [Route("location")]
        [HttpPost]
        public async Task<ActionResult> PostLocation([FromBody] LocationPost post)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            int? providerId = null;
            
            
            var providers = await _sqlHelper.GetOpFlowSetup();

            // Try to find the provider in the various databases
            var provider = providers.FirstOrDefault(p => p.ProviderName.ToLower() == post.Provider.ToLower());
            if (provider != null)
            {
                providerId = provider.ProviderID;
            }
            
            var locationId = await _sqlHelper.CreateLocation(providerId, post.Provider, post.Location);

            return Ok();
        }

        // GET api/values/5
        [Route("location/{locationId}")]
        [HttpPut]
        public async Task<ActionResult> PutLocation(int locationId, [FromBody] OpFlowLocation post)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            
            
            var result = await _sqlHelper.UpdateLocation(locationId, post.TrayHigh, post.TrayMed, post.TrayLow,
                post.SurgeonHigh, post.SurgeonMed, post.SurgeonLow, post.AuditHigh, post.AuditMed, post.AuditLow, post.DailyTarget);

            return Ok(result);
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="nameSearchText">Will filter based on first/last name string match</param>
        /// <returns></returns>
        [Route("SearchUsers", Name = "SearchInternalUsers")]
        public async Task<ActionResult> GetUsers(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var users = await _sqlHelper.SearchInternalUsers(nameSearchText, roleId, specialtyId, user.ProviderID, user.LocationID);

            return Ok(users);
        }

        /// <summary>
        /// List roles
        /// </summary>
        /// <returns></returns>
        [Route("Roles", Name = "GetInternalRoles")]
        public async Task<ActionResult> GetRoles(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var roles = await _sqlHelper.GetInternalRoles(user.ProviderID, user.LocationID);

            return Ok(roles);
        }

        /// <summary>
        /// List vendors
        /// </summary>
        /// <returns></returns>
        [Route("Vendors", Name = "GetVendors")]
        public async Task<ActionResult> GetVendors()
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var roles = await _sqlHelper.GetVendors(user.LocationID);

            return Ok(roles);
        }

        /// <summary>
        /// Setup location with base data
        /// </summary>
        /// <returns></returns>
        [Route("InitializeLocation/{locationId}", Name = "InitializeLocation")]
        [HttpPost]
        public async Task<ActionResult> InitializeLocation(int locationId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var roles = await _sqlHelper.InitializeLocation(locationId);

            return Ok(roles);
        }

        [Route("communicationComments")]
        [HttpPost]
        public async Task<ActionResult> AddCommunicationComments(int locationId, int historyId, [FromBody] TrayCommunicationHistoryUpdatePost post)
        {
            var user = await GetUserSecurity();
            

            if (user.RoleType != "Internal")
                return NotFound();

            var sentDate = DateTime.Today;
            if (DateTime.TryParse(post.SentDate, out var tmpSentDate))
                sentDate = tmpSentDate;

            var communication = await _sqlHelper.UpdateProposedTrayCommunicationHistory(historyId, sentDate, post.Comments,
                locationId);

            return Ok();
        }

        [Route("CommunicationScreen", Name = "GetCommunicationScreen")]
        [HttpGet]
        public async Task<ActionResult> GetCommunicationScreen(int? locationId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            if (locationId == null)
                return Ok(new
                {
                    Phases = new List<string>()
                });

            var phases = await _sqlHelper.GetTrayProposalPhases(locationId.Value);
            var specialties = await _sqlHelper.GetSpecialties(locationId.Value);
            var proposedTrays = await _sqlHelper.GetProposedTrays(null, locationId.Value);
            var users = await _sqlHelper.SearchUsers(null, null, null, locationId.Value);
            var communicationMethods = await _sqlHelper.GetTrayCommunicationMethods(locationId.Value);

            return Ok(new
            {
                Phases = phases,
                Specialties = specialties,
                Proposals = proposedTrays,
                Users = users,
                CommunicationMethods = communicationMethods,
            });
        }

        [Route("communication")]
        [HttpPost]
        public async Task<ActionResult> GetCommunicationHistory([FromBody] InternalTrayCommunicationHistoryPost post)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var communication = await _sqlHelper.GetInternalProposedTrayCommunicationHistory(
                post.LocationID, post.PhaseID,
                post.TrayProposalIds, post.SpecialtyIds, post.UserIds);

            return Ok(new
            {
                Communication = communication
            });
        }

        [Route("communicationHistory")]
        [HttpPost]
        public async Task<ActionResult> InsertCommunicationHistory([FromBody] InternalTrayCommunicationInsertPost post)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var sentDate = DateTime.Today;
            if (DateTime.TryParse(post.SentDate, out var tmpSentDate))
                sentDate = tmpSentDate;

            var communication = await _sqlHelper.InsertProposedTrayCommunicationHistory(
                post.Phase, post.Activity, post.Tray, sentDate, post.Audience, post.Method, post.LocationID);

            return Ok(new
            {
                Communication = communication
            });
        }

        [Route("Specialty/{specialtyId}", Name = "UpdateSpecialtyMaster")]
        [HttpPut]
        public async Task<ActionResult> UpdateSpecialtyMaster(int specialtyId, [FromBody] SpecialtyMasterPost request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            
            var result = await _sqlHelper.UpdateSpecialtyMaster(specialtyId, request.SpecialtyName);

            return Ok(result);
        }

        [Route("Specialty", Name = "CreateSpecialtyMaster")]
        [HttpPost]
        public async Task<ActionResult> CreateSpecialtyMaster([FromBody] SpecialtyMasterPost request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            
            var specialtyId = await _sqlHelper.InsertSpecialtyMaster(request.SpecialtyName);

            return Ok(specialtyId);
        }

        [Route("Specialty/{specialtyId}", Name = "DeleteSpecialtyMaster")]
        [HttpDelete]
        public async Task<ActionResult> DeleteSpecialtyMaster(int specialtyId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            
            var result = await _sqlHelper.DeleteSpecialtyMaster(specialtyId);

            return Ok(specialtyId);
        }
    }
}