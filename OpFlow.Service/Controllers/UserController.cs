using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class UserController : ApiController
    {
        // GET api/values/jdoe
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(User))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/User", Name = "User")]
        public HttpResponseMessage Get(string username)
        {
            var users = DataAccess.SqlHelper.GetUsers(username);

            var result = users.FirstOrDefault();

            if (result == null || username == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");

            return Request.CreateResponse(HttpStatusCode.OK, result);
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