using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Services.Protocols;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OpFlow.Data;
using OpFlow.Service.App_Start;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.Controllers
{
    /// <summary>
    /// Interact with Users entities
    /// </summary>
    [Authorize]
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        /// <summary>
        /// Return user entity for currently authenticated user
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("AuthorizedUser", Name = "AuthorizedUser")]
        public async Task<HttpResponseMessage> Get()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserAuthID);
            result.PHILocation = (user.SecureDatabaseName != "InvalidConnection");

            return result == null ? Request.CreateResponse(HttpStatusCode.NotFound, "User not found") : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("VendorPortal", Name = "VendorPortal")]
        public async Task<HttpResponseMessage> GetVendorPortal()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");

            var vendorLocations = await sqlHelper.GetVendorLocations(user.ProviderID);
            var procedureProfiles = await sqlHelper.GetProcedureProfilesVendor(user.ProviderID);
            var surgeons = await sqlHelper.GetSurgeons(null, user.VendorLocationID ?? -1);

            var providers = vendorLocations.GroupBy(v => new { v.ProviderID, v.ProviderName }).Select(v => new OpFlowProvider()
            {
                ProviderID = v.Key.ProviderID,
                ProviderName = v.Key.ProviderName,
                Locations = v.ToList()
            });

            var result = new
            {
                Providers = providers,
                Locations = vendorLocations,
                ProcedureProfiles = procedureProfiles.Where(p => p.Cards.Any()),
                Surgeons = surgeons
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorLocation/{locationId}", Name = "PutVendorLocation")]
        [HttpPost]
        public async Task<HttpResponseMessage> PutVendorLocation(int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Put not found");

            var result = await sqlHelper.UpdateUserVendorLocation(user.UserID, locationId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Vendor Portal Home screen for vendors
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("vendorLocation", Name = "PostVendorLocation")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostVendorLocation([FromBody] VendorCreateLocationPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Put not found");

            var valid = int.TryParse(request.Provider, out var providerId);
            var result = await sqlHelper.InsertVendorLocation(valid ? providerId : (int?)null, 
                request.Provider, request.Location, request.Street, request.City, request.State, request.Zip, user.ProviderID);

            CacheUtil.RefreshUserCache();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Map OPP to surgeon card
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("surgeonProfile/{procedureProfileId}", Name = "PutSurgeonProfile")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutSurgeonProfile(int procedureProfileId, [FromBody] SurgeonProfilePost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Put not found");

            foreach (var surgeonId in request.SurgeonID)
            {
                await sqlHelper.UpdateSurgeonProfile(procedureProfileId, surgeonId, user.VendorLocationID.Value);
            }

            return Request.CreateResponse(HttpStatusCode.OK, procedureProfileId);
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="nameSearchText">Will filter based on first/last name string match</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<User>))]
        [Route("SearchUsers", Name = "SearchUsers")]
        public async Task<HttpResponseMessage> GetUsers(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var users = await sqlHelper.SearchUsers(nameSearchText, roleId, specialtyId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, users);
        }

        /// <summary>
        /// List roles
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<User>))]
        [Route("Roles", Name = "GetRoles")]
        public async Task<HttpResponseMessage> GetRoles(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var roles = await sqlHelper.GetRoles(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roles);
        }

        // POST api/values
        [SwaggerOperation("CheckIn")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("Checkin", Name = "Checkin")]
        public async Task<IHttpActionResult> CheckinUser(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.CheckInUser(user, surgeryId);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("CheckOut")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("Checkout", Name = "Checkout")]
        public async Task<IHttpActionResult> CheckoutUser(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.CheckOutUser(user, surgeryId);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("CheckOut")]
        
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("Reviewed", Name = "Reviewed")]
        public async Task<IHttpActionResult> WorkupReviewed(int surgeryId, [FromBody]User user)
        {
            var userSecurity = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.WorkupReviewed(user, surgeryId);

            return Ok();
        }

        // POST api/Account/ResetPassword
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(int userId, [FromBody]SetPasswordBindingModel model)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var secureUser = await sqlHelper.GetSecureUser(null, userId);
            
            if (secureUser != null && secureUser.ProviderID == user.ProviderID && secureUser.LocationID == user.LocationID)
            {
                var code = await userManager.GeneratePasswordResetTokenAsync(secureUser.UserAuthID.ToString());

                var result =
                    await userManager.ResetPasswordAsync(secureUser.UserAuthID.ToString(), code, model.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest();
                }
            }

            return Ok();
        }

        // PUT api/values/5
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(int))]
        public async Task<IHttpActionResult> Post([FromBody]UserPost model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var existing = await userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return Conflict();

            var authenticationUser = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            var result = await userManager.CreateAsync(authenticationUser, model.Password);

            // Whatever role the current user is in - they can only add users to this role
            var currentAuthId = HttpContext.Current.User.Identity.GetUserId();
            var currentRoles = await userManager.GetRolesAsync(currentAuthId);
            var currentRole = currentRoles?.FirstOrDefault();

            if (currentRole == null)
                throw new Exception("Unable to locate authenticated user");

            userManager.AddToRole(authenticationUser.Id, currentRole);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.FirstOrDefault());
            }

            var userAuthId = new Guid(authenticationUser.Id);

            try
            {
                var applicationUserId = await sqlHelper.CreateUser(userAuthId, model.RoleID, model.SpecialtyID,
                    model.FirstName, model.LastName,
                    model.Email, model.CellPhone, model.Initials, model.Title, user.ProviderID,
                    user.LocationID);

                return Ok(applicationUserId);
            }
            catch (Exception ex)
            {
                // roll back creation of aspnet user
                await userManager.DeleteAsync(authenticationUser);

                throw ex;
            }
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Put(int userId, [FromBody]UserEdit model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var authUserSecurity = await sqlHelper.GetSecureUser(null, userId);

            if (authUserSecurity.ProviderID == user.ProviderID &&
                authUserSecurity.LocationID == user.LocationID)
            {
                var applicationUser = await sqlHelper.UpdateUser(userId, (int)model.RoleID, model.SpecialtyID, model.FirstName, model.LastName,
                    model.Email, model.CellPhone, model.Initials, model.Title, user.ProviderID, user.LocationID);

                // make certain user auth matches what we sent
                var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var authUser = await userManager.FindByIdAsync(authUserSecurity.UserAuthID.ToString());

                if (authUser.Email != model.Email)
                {
                    authUser.UserName = model.Email;
                    authUser.Email = model.Email;

                    await userManager.UpdateAsync(authUser);
                }
            }

            return Ok();
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Delete(int userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var applicationUser = await sqlHelper.GetUser(user.ProviderID, user.LocationID,  userId);

            var authenticationUser = await userManager.FindByEmailAsync(applicationUser.Email);

            if (authenticationUser != null)
            {
                var authResult = await userManager.RemovePasswordAsync(authenticationUser.Id);
            }

            var applicationDeletion = await sqlHelper.DeleteUser(userId, user.ProviderID, user.LocationID);

            return Ok();
        }
    }
}