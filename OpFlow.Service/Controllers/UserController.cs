using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        [Route("api/UserByUsername", Name = "UserByUsername")]
        public HttpResponseMessage GetUser(string username)
        {
            var users = DataAccess.SqlHelper.GetUsers(username);

            var result = users.FirstOrDefault();

            if (result == null || username == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, "User not found");

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
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