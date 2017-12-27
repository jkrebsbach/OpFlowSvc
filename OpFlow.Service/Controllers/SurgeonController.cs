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
    public class SurgeonController : ApiController
    {
        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<Surgeon> Get()
        {
            return DataAccess.SecureSqlHelper.GetSurgeons();
        }

        // GET api/values/jdoe
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/SurgeonByUsername", Name = "SurgeonByUsername")]
        public HttpResponseMessage GetSurgeon(string username)
        {
            var surgeons = DataAccess.SecureSqlHelper.GetSurgeons();

            var result = surgeons.FirstOrDefault(s => string.Equals(s.Username, username, StringComparison.CurrentCultureIgnoreCase));

            if (result == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Surgeon not found");

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