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
using OpFlow.Data;

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
            var username = HttpContext.Current.User.Identity.Name;
            var result = DataAccess.SqlHelper.GetUser(username);

            return result == null ? Request.CreateResponse(HttpStatusCode.NotFound, "User not found") : Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Search users
        /// </summary>
        /// <param name="nameSearchText">Will filter based on first/last name string match</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<User>))]
        [Route("api/User/SearchUsers", Name = "SearchUsers")]
        public HttpResponseMessage GetUsers(string nameSearchText = null, int? roleId = null)
        {
            var userSecurity = CacheUtil.GetUserSecurity();

            var users = DataAccess.SqlHelper.SearchUsers(nameSearchText, roleId, userSecurity.ProviderID, userSecurity.LocationID);

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

        // POST api/values
        [SwaggerOperation("AssignUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/User/Assign", Name = "AssignUser")]
        public async Task<IHttpActionResult> AssignToCase(int surgeryId, int userId)
        {
            var userSecurity = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignUserToCase(userId, surgeryId, userSecurity.ProviderID, userSecurity.LocationID);

            return Ok();
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}