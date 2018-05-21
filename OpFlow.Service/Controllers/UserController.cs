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
using OpFlow.Service.Models;

namespace OpFlow.Service.Controllers
{
    /// <summary>
    /// Interact with Users entities
    /// </summary>
    [Authorize]
    public class UserController : ApiController
    {
        /// <summary>
        /// Return user entity for currently authenticated user
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/User/AuthorizedUser", Name = "AuthorizedUser")]
        public HttpResponseMessage Get()
        {
            var user = CacheUtil.GetUserSecurity();
            var username = HttpContext.Current.User.Identity.Name;

            var result = DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, username, null);

            return result == null ? Request.CreateResponse(HttpStatusCode.NotFound, "User not found") : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="nameSearchText">Will filter based on first/last name string match</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<User>))]
        [Route("api/User/SearchUsers", Name = "SearchUsers")]
        public HttpResponseMessage GetUsers(string nameSearchText = null, int? roleId = null, int? specialtyId = null)
        {
            var userSecurity = CacheUtil.GetUserSecurity();

            var users = DataAccess.SqlHelper.SearchUsers(nameSearchText, roleId, specialtyId, userSecurity.ProviderID, userSecurity.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, users);
        }

        // POST api/values
        [SwaggerOperation("CheckIn")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/User/Checkin", Name = "Checkin")]
        public async Task<IHttpActionResult> CheckinUser(int surgeryId, [FromBody]User user)
        {
            DataAccess.SqlHelper.CheckInUser(user, surgeryId);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("CheckOut")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/User/Checkout", Name = "Checkout")]
        public async Task<IHttpActionResult> CheckoutUser(int surgeryId, [FromBody]User user)
        {
            DataAccess.SqlHelper.CheckOutUser(user, surgeryId);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("CheckOut")]
        
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/User/Reviewed", Name = "Reviewed")]
        public async Task<IHttpActionResult> WorkupReviewed(int surgeryId, [FromBody]User user)
        {
            DataAccess.SqlHelper.WorkupReviewed(user, surgeryId);

            return Ok();
        }

        // POST api/Account/ResetPassword
        [Route("api/User/ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(int userId, [FromBody]SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var userSecurity = CacheUtil.GetUserSecurity();
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var secureUser = DataAccess.SqlHelper.GetSecureUser(null, userId);
            
            if (secureUser != null && secureUser.ProviderID == userSecurity.ProviderID && secureUser.LocationID == userSecurity.LocationID)
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
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public async Task<IHttpActionResult> Post([FromBody]UserPost model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userSecurity = CacheUtil.GetUserSecurity();
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var authenticationUser = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            var result = await userManager.CreateAsync(authenticationUser, model.Password);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.FirstOrDefault());
            }

            var userAuthId = new Guid(authenticationUser.Id);

            var applicationUser = DataAccess.SqlHelper.CreateUser(userAuthId, model.RoleID, model.SpecialtyID, model.FirstName, model.LastName,
                model.Email, model.CellPhone, model.Initials, model.Title, userSecurity.ProviderID, userSecurity.LocationID);

            return Ok();
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

            var userSecurity = CacheUtil.GetUserSecurity();
            var authUserSecurity = DataAccess.SqlHelper.GetSecureUser(null, userId);

            if (authUserSecurity.ProviderID == userSecurity.ProviderID &&
                authUserSecurity.LocationID == userSecurity.LocationID)
            {
                var applicationUser = DataAccess.SqlHelper.UpdateUser(userId, (int)model.RoleID, model.SpecialtyID, model.FirstName, model.LastName,
                    model.Email, model.CellPhone, model.Initials, model.Title, userSecurity.ProviderID, userSecurity.LocationID);

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

            var userSecurity = CacheUtil.GetUserSecurity();
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var applicationUser =
                DataAccess.SqlHelper.GetUser(userSecurity.ProviderID, userSecurity.LocationID, null, userId);

            var authenticationUser = await userManager.FindByEmailAsync(applicationUser.Email);

            if (authenticationUser != null)
            {
                var authResult = await userManager.RemovePasswordAsync(authenticationUser.Id);
            }

            var applicationDeletion = DataAccess.SqlHelper.DeleteUser(userId, userSecurity.ProviderID, userSecurity.LocationID);

            return Ok();
        }
    }
}